using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Index
{
    public sealed class MusicIndexService : IMusicIndexService
    {
        private readonly LibraryWriteCoordinator _writeCoordinator;

        private readonly ISettingsService _settingsService;
        private readonly IMusicRepository _musicRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly MusicIndexBuilder _indexBuilder;

        public MusicIndexService(IMusicRepository musicRepository, IAlbumRepository albumRepository, IArtistRepository artistRepository, MusicIndexBuilder indexBuilder, LibraryWriteCoordinator writeCoordinator, ISettingsService settingsService)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));
            Guard.NotNull(albumRepository, nameof(albumRepository));
            Guard.NotNull(artistRepository, nameof(artistRepository));
            Guard.NotNull(indexBuilder, nameof(indexBuilder));
            Guard.NotNull(writeCoordinator, nameof(writeCoordinator));
            Guard.NotNull(settingsService, nameof(settingsService));

            _musicRepository = musicRepository;
            _albumRepository = albumRepository;
            _artistRepository = artistRepository;
            _indexBuilder = indexBuilder;
            _writeCoordinator = writeCoordinator;
            _settingsService = settingsService;
        }

        public Task RebuildAsync()
        {
            return _writeCoordinator.ExecuteAsync(RebuildCoreAsync);
        }

        private async Task RebuildCoreAsync()
        {
            var settings = await _settingsService.LoadAsync();
            var groupingMode = settings.Library.ArtistGrouping;

            var allMusic = await _musicRepository.GetAllAsync();

            var existingAlbums = await _albumRepository.GetAllAsync();
            var existingArtists = await _artistRepository.GetAllAsync();

            var existingAlbumsByKey = existingAlbums.ToDictionary(album => MusicIndexBuilder.CreateAlbumKey(album.Title, album.ArtistName));

            var existingArtistsByName = existingArtists.ToDictionary(artist => MusicIndexBuilder.NormalizeArtistName(artist.Name), StringComparer.Ordinal);

            var albums = _indexBuilder.BuildAlbums(allMusic);

            foreach (var album in albums)
            {
                var key = MusicIndexBuilder.CreateAlbumKey(album.Title, album.ArtistName);

                Album existing;

                if (existingAlbumsByKey.TryGetValue(key, out existing))
                {
                    album.Id = existing.Id;
                    album.Description = existing.Description ?? string.Empty;
                    album.AddedAt = existing.AddedAt;
                }
            }

            // 先恢复专辑 ID，再构建艺术家的专辑关联。
            var rebuiltArtists = _indexBuilder.BuildArtists(allMusic, albums, groupingMode);

            var artistsToSave = new List<Artist>();
            var rebuiltArtistIds = new HashSet<ArtistId>();

            foreach (var artist in rebuiltArtists)
            {
                string key = MusicIndexBuilder.NormalizeArtistName(artist.Name);

                Artist existing;

                if (existingArtistsByName.TryGetValue(key, out existing))
                {
                    artist.Id = existing.Id;
                    artist.SortName = existing.SortName;
                    artist.Description = existing.Description ?? string.Empty;
                    artist.AddedAt = existing.AddedAt;
                }

                artistsToSave.Add(artist);
                rebuiltArtistIds.Add(artist.Id);
            }

            foreach (var existing in existingArtists)
            {
                if (rebuiltArtistIds.Contains(existing.Id))
                {
                    continue;
                }

                bool associationsChanged = existing.MusicCount > 0 || existing.AlbumCount > 0 || existing.TotalDuration != TimeSpan.Zero;

                // 当前分类中不再出现：保留资料，清空关联与统计。
                artistsToSave.Add(new Artist
                {
                    Id = existing.Id,
                    Name = existing.Name,
                    SortName = existing.SortName,
                    Description = existing.Description ?? string.Empty,

                    MusicIds = new List<MusicId>(),
                    AlbumIds = new List<AlbumId>(),
                    TotalDuration = TimeSpan.Zero,

                    AddedAt = existing.AddedAt,
                    UpdatedAt = associationsChanged
                        ? DateTimeOffset.Now
                        : existing.UpdatedAt
                });
            }

            await _albumRepository.UpsertRangeAsync(albums);

            // 已重建的项和归零的项，一次批量保存。
            await _artistRepository.UpsertRangeAsync(artistsToSave);

            var albumIds = new HashSet<AlbumId>(albums.Select(album => album.Id));

            // 专辑继续沿用现有清理规则。
            foreach (var existing in existingAlbums)
            {
                if (!albumIds.Contains(existing.Id))
                {
                    await _albumRepository.DeleteAsync(existing.Id);
                }
            }
        }

        public Task SetArtistGroupingAsync(ArtistGroupingMode groupingMode)
        {
            if (!Enum.IsDefined(typeof(ArtistGroupingMode), groupingMode))
            {
                throw new ArgumentOutOfRangeException(nameof(groupingMode));
            }

            return _writeCoordinator.ExecuteAsync(async () =>
            {
                var settings = await _settingsService.LoadAsync();

                settings.Library.ArtistGrouping = groupingMode;

                await _settingsService.SaveAsync(settings);

                try
                {
                    await RebuildCoreAsync();
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        "艺术家分类设置已保存，但索引重建失败。" +
                        "当前列表可能尚未更新，请重新构建索引。",
                        exception);
                }
            });
        }
    }
}

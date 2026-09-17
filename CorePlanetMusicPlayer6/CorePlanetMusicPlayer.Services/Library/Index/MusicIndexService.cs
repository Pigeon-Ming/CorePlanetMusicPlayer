using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Data.Repositories;
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

        private readonly IMusicRepository _musicRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly MusicIndexBuilder _indexBuilder;

        public MusicIndexService(IMusicRepository musicRepository, IAlbumRepository albumRepository, IArtistRepository artistRepository, MusicIndexBuilder indexBuilder, LibraryWriteCoordinator writeCoordinator)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));
            Guard.NotNull(albumRepository, nameof(albumRepository));
            Guard.NotNull(artistRepository, nameof(artistRepository));
            Guard.NotNull(indexBuilder, nameof(indexBuilder));
            Guard.NotNull(writeCoordinator, nameof(writeCoordinator));

            _musicRepository = musicRepository;
            _albumRepository = albumRepository;
            _artistRepository = artistRepository;
            _indexBuilder = indexBuilder;
            _writeCoordinator = writeCoordinator;
        }

        public Task RebuildAsync()
        {
            return _writeCoordinator.ExecuteAsync(RebuildCoreAsync);
        }

        private async Task RebuildCoreAsync()
        {
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

            // 先恢复专辑 ID，再生成艺术家的专辑关联。
            var artists = _indexBuilder.BuildArtists(allMusic, albums);

            foreach (var artist in artists)
            {
                string key = MusicIndexBuilder.NormalizeArtistName(artist.Name);

                Artist existing;

                if (existingArtistsByName.TryGetValue(key, out existing))
                {
                    artist.Id = existing.Id;
                    artist.Description = existing.Description ?? string.Empty;
                    artist.AddedAt = existing.AddedAt;
                }
            }

            await _albumRepository.UpsertRangeAsync(albums);
            await _artistRepository.UpsertRangeAsync(artists);

            var albumIds = new HashSet<AlbumId>(albums.Select(album => album.Id));

            var artistIds = new HashSet<ArtistId>(artists.Select(artist => artist.Id));

            // 新结果保存成功后，清理已经不存在的分类。
            foreach (var existing in existingArtists)
            {
                if (!artistIds.Contains(existing.Id))
                {
                    await _artistRepository.DeleteAsync(
                        existing.Id);
                }
            }

            foreach (var existing in existingAlbums)
            {
                if (!albumIds.Contains(existing.Id))
                {
                    await _albumRepository.DeleteAsync(
                        existing.Id);
                }
            }
        }
    }
}

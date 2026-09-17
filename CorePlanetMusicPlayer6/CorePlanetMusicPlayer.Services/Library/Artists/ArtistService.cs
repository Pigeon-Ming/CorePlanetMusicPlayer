using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Library.MusicQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Artists
{
    public sealed class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly IMusicQueryService _musicQueryService;
        private readonly LibraryWriteCoordinator _writeCoordinator;

        public ArtistService(IArtistRepository artistRepository, IAlbumRepository albumRepository, IMusicQueryService musicQueryService, LibraryWriteCoordinator writeCoordinator)
        {
            Guard.NotNull(artistRepository, nameof(artistRepository));
            Guard.NotNull(albumRepository, nameof(albumRepository));
            Guard.NotNull(musicQueryService, nameof(musicQueryService));
            Guard.NotNull(writeCoordinator, nameof(writeCoordinator));

            _artistRepository = artistRepository;
            _albumRepository = albumRepository;
            _musicQueryService = musicQueryService;
            _writeCoordinator = writeCoordinator;
        }

        public Task<IReadOnlyList<Artist>> GetAllAsync()
        {
            return _artistRepository.GetAllAsync();
        }

        public Task<Artist> GetByIdAsync(ArtistId artistId)
        {
            if (artistId.IsEmpty)
            {
                throw new ArgumentException("艺术家 ID 不能为空。", nameof(artistId));
            }

            return _artistRepository.GetByIdAsync(artistId);
        }

        public async Task<ArtistDetails> GetDetailsAsync(ArtistId artistId)
        {
            var artist = await GetByIdAsync(artistId);

            if (artist == null)
            {
                return null;
            }

            // 先复制关联列表，后续按照这次读取的顺序组织结果。
            var requestedMusicIds = artist.MusicIds == null ? new List<MusicId>() : artist.MusicIds.ToList();

            var requestedAlbumIds = artist.AlbumIds == null ? new List<AlbumId>() : artist.AlbumIds.ToList();

            if (requestedAlbumIds.Any(id => id.IsEmpty))
            {
                throw new InvalidOperationException("艺术家的专辑关联列表包含空 ID。");
            }

            // 歌曲 ID 的校验和按顺序读取由音乐查询服务负责。
            var musicItems = await _musicQueryService.GetByIdsAsync(requestedMusicIds);

            var foundMusicIds = new HashSet<MusicId>(musicItems.Select(music => music.Id));

            var missingMusicIds = new List<MusicId>();
            var recordedMissingMusicIds = new HashSet<MusicId>();

            foreach (var musicId in requestedMusicIds)
            {
                if (!foundMusicIds.Contains(musicId) && recordedMissingMusicIds.Add(musicId))
                {
                    missingMusicIds.Add(musicId);
                }
            }

            var albums = new List<Album>();
            var missingAlbumIds = new List<AlbumId>();
            var recordedMissingAlbumIds = new HashSet<AlbumId>();

            // 本次调用内缓存结果，避免重复查询相同专辑。
            var queriedAlbums = new Dictionary<AlbumId, Album>();

            foreach (var albumId in requestedAlbumIds)
            {
                Album album;

                if (!queriedAlbums.TryGetValue(albumId, out album))
                {
                    album = await _albumRepository.GetByIdAsync(albumId);
                    queriedAlbums.Add(albumId, album);
                }

                if (album != null)
                {
                    albums.Add(album);
                }
                else if (recordedMissingAlbumIds.Add(albumId))
                {
                    missingAlbumIds.Add(albumId);
                }
            }

            return new ArtistDetails(
                artist,
                musicItems,
                albums,
                missingMusicIds,
                missingAlbumIds);
        }

        public Task UpdateDescriptionAsync(ArtistId artistId, string description)
        {
            if (artistId.IsEmpty)
            {
                throw new ArgumentException("艺术家 ID 不能为空。", nameof(artistId));
            }

            string value = description ?? string.Empty;

            return _writeCoordinator.ExecuteAsync(async () =>
            {
                bool updated = await _artistRepository.UpdateDescriptionAsync(artistId, value, DateTimeOffset.Now);

                if (!updated)
                {
                    throw new InvalidOperationException("该艺术家已不存在，请刷新艺术家列表。");
                }
            });
        }
    }
}

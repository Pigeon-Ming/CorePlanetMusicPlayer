using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Library.Artists;
using CorePlanetMusicPlayer.Services.Library.MusicQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Albums
{
    public sealed class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IMusicQueryService _musicQueryService;
        private readonly LibraryWriteCoordinator _writeCoordinator;

        public AlbumService(IAlbumRepository albumRepository, IMusicQueryService musicQueryService, LibraryWriteCoordinator writeCoordinator)
        {
            Guard.NotNull(albumRepository, nameof(albumRepository));

            Guard.NotNull(musicQueryService, nameof(musicQueryService));

            Guard.NotNull(writeCoordinator, nameof(writeCoordinator));

            _albumRepository = albumRepository;
            _musicQueryService = musicQueryService;
            _writeCoordinator = writeCoordinator;
        }

        public Task<IReadOnlyList<Album>> GetAllAsync()
        {
            return _albumRepository.GetAllAsync();
        }

        public Task<Album> GetByIdAsync(AlbumId albumId)
        {
            if (albumId.IsEmpty)
            {
                throw new ArgumentException("专辑 ID 不能为空。", nameof(albumId));
            }

            return _albumRepository.GetByIdAsync(albumId);
        }

        public async Task<AlbumDetails> GetDetailsAsync(AlbumId albumId)
        {
            var album = await GetByIdAsync(albumId);

            if (album == null)
            {
                return null;
            }

            var requestedIds = album.MusicIds == null ? new List<MusicId>() : album.MusicIds.ToList();

            var musicItems = await _musicQueryService.GetByIdsAsync(requestedIds);

            var foundIds = new HashSet<MusicId>(musicItems.Select(music => music.Id));

            var missingIds = new List<MusicId>();
            var recordedMissingIds = new HashSet<MusicId>();

            foreach (var musicId in requestedIds)
            {
                if (!foundIds.Contains(musicId) &&recordedMissingIds.Add(musicId))
                {
                    missingIds.Add(musicId);
                }
            }

            return new AlbumDetails(
                album,
                musicItems,
                missingIds);
        }

        public Task UpdateDescriptionAsync(AlbumId albumId, string description)
        {
            if (albumId.IsEmpty)
            {
                throw new ArgumentException("专辑 ID 不能为空。", nameof(albumId));
            }

            string value = description ?? string.Empty;

            return _writeCoordinator.ExecuteAsync(async () =>
            {
                bool updated = await _albumRepository.UpdateDescriptionAsync(albumId, value, DateTimeOffset.Now);

                if (!updated)
                {
                    throw new InvalidOperationException("该专辑已不存在，请刷新专辑列表。");
                }
            });
        }
    }
}

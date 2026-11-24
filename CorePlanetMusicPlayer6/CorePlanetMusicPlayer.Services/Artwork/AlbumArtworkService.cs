using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Services.Library.Albums;
using CorePlanetMusicPlayer.Services.Library.MusicQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public sealed class AlbumArtworkService : IAlbumArtworkService
    {
        private const string DefaultResourceName =
            "DefaultAlbumArtwork";

        private readonly IAlbumService _albumService;
        private readonly IMusicQueryService _musicQueryService;
        private readonly IArtworkService _artworkService;

        public AlbumArtworkService(
            IAlbumService albumService,
            IMusicQueryService musicQueryService,
            IArtworkService artworkService)
        {
            Guard.NotNull(albumService, nameof(albumService));
            Guard.NotNull(musicQueryService, nameof(musicQueryService));
            Guard.NotNull(artworkService, nameof(artworkService));

            _albumService = albumService;
            _musicQueryService = musicQueryService;
            _artworkService = artworkService;
        }

        public async Task<ArtworkRequest> GetArtworkByAlbumIdAsync(AlbumId albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (albumId.IsEmpty)
            {
                throw new ArgumentException("专辑 ID 不能为空。", nameof(albumId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var album = await _albumService.GetByIdAsync(albumId);

            cancellationToken.ThrowIfCancellationRequested();

            if (album == null)
            {
                return new ArtworkRequest(new ArtworkReference[0], DefaultResourceName);
            }

            return await GetArtworkAsync(album, cancellationToken);
        }

        public async Task<ArtworkRequest> GetArtworkAsync(
            Album album,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Guard.NotNull(album, nameof(album));

            if (album.Id.IsEmpty)
            {
                throw new ArgumentException("专辑 ID 不能为空。", nameof(album));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var musicIds = album.MusicIds == null
                ? new List<MusicId>()
                : album.MusicIds.ToList();

            var musicItems = await _musicQueryService.GetByIdsAsync(
                musicIds,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return await CreateArtworkAsync(musicItems, cancellationToken);
        }

        public async Task<ArtworkRequest> CreateArtworkAsync(IEnumerable<Music> orderedMusic, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (orderedMusic == null)
            {
                throw new ArgumentNullException(nameof(orderedMusic));
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 保存本次顺序，避免等待期间集合被修改。
            var musicItems = orderedMusic.ToList();

            var references = new List<ArtworkReference>();
            var visitedIds = new HashSet<MusicId>();

            foreach (var music in musicItems)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (music == null || music.Id.IsEmpty || !visitedIds.Add(music.Id))
                {
                    continue;
                }

                var reference = await _artworkService.GetArtworkAsync(music);

                cancellationToken.ThrowIfCancellationRequested();

                references.Add(reference);
            }

            return new ArtworkRequest(references, DefaultResourceName);
        }
    }
}

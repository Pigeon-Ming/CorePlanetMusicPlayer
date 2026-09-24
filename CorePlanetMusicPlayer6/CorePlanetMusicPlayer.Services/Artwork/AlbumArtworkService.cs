using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Services.Library.Albums;
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
        private readonly IArtworkService _artworkService;

        public AlbumArtworkService(IAlbumService albumService, IArtworkService artworkService)
        {
            Guard.NotNull(albumService, nameof(albumService));
            Guard.NotNull(artworkService, nameof(artworkService));

            _albumService = albumService;
            _artworkService = artworkService;
        }

        public async Task<ArtworkRequest> GetArtworkByAlbumIdAsync(AlbumId albumId, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (albumId.IsEmpty)
            {
                throw new ArgumentException("专辑 ID 不能为空。", nameof(albumId));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var details = await _albumService.GetDetailsAsync(albumId);

            cancellationToken.ThrowIfCancellationRequested();

            if (details == null)
            {
                return new ArtworkRequest(new ArtworkReference[0], DefaultResourceName);
            }

            return await CreateArtworkAsync(details.MusicItems, cancellationToken);
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

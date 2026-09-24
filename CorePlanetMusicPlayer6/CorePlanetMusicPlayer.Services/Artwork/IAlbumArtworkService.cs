using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public interface IAlbumArtworkService
    {
        Task<ArtworkRequest> GetArtworkByAlbumIdAsync(AlbumId albumId, CancellationToken cancellationToken = default(CancellationToken));

        // 调用方须提供已经按专辑顺序排列的歌曲。
        Task<ArtworkRequest> CreateArtworkAsync(IEnumerable<Music> orderedMusic, CancellationToken cancellationToken = default(CancellationToken));
    }
}

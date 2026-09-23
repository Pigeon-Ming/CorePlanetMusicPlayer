using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public interface IArtworkService
    {
        Task<ArtworkReference> GetArtworkByMusicIdAsync(MusicId musicId);

        Task<ArtworkReference> GetArtworkAsync(Music music);

        Task<ArtworkReference> GetCacheArtworkAsync(MusicId musicId);

        Task<ArtworkReference> GetEmbeddedArtworkAsync(Music music);

        Task<ArtworkReference> GetDefaultArtworkAsync();

        Task<ArtworkReference> GetDefaultArtworkAsync(MusicId musicId);
    }
}

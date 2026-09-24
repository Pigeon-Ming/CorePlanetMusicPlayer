using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Core.Playlists;
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

        Task<ArtworkReference> GetArtworkByArtistIdAsync(ArtistId artistId);

        Task<ArtworkReference> GetArtworkByPlaylistIdAsync(PlaylistId playlistId);

        Task<ArtworkReference> GetArtworkAsync(Music music);

        Task<ArtworkReference> GetDefaultArtworkAsync();

        Task<ArtworkReference> GetDefaultArtworkAsync(MusicId musicId);
    }
}

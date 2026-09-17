using CorePlanetMusicPlayer.Core.Artists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Artists
{
    public interface IArtistService
    {
        Task<IReadOnlyList<Artist>> GetAllAsync();

        Task<Artist> GetByIdAsync(ArtistId artistId);

        Task<ArtistDetails> GetDetailsAsync(ArtistId artistId);

        Task UpdateDescriptionAsync(ArtistId artistId, string description);
    }
}

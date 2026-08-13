using CorePlanetMusicPlayer.Core.Artists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories
{
    public interface IArtistRepository
    {
        Task<IReadOnlyList<Artist>> GetAllAsync();

        Task<Artist> GetByIdAsync(ArtistId id);

        Task<IReadOnlyList<Artist>> SearchAsync(string keyword);

        Task UpsertAsync(Artist artist);

        Task UpsertRangeAsync(IEnumerable<Artist> artists);

        Task DeleteAsync(ArtistId id);

        Task ClearAsync();
    }
}

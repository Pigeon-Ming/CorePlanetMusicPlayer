using CorePlanetMusicPlayer.Core.Artwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories
{
    public interface IArtworkRepository
    {
        Task<ArtworkAssignment> GetByOwnerAsync(ArtworkOwner owner);

        Task<IReadOnlyList<ArtworkAssignment>> GetAllAsync();

        Task UpsertAsync(ArtworkAssignment assignment);

        Task DeleteByOwnerAsync(ArtworkOwner owner);
    }
}

using CorePlanetMusicPlayer.Core.Albums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories
{
    public interface IAlbumRepository
    {
        Task<IReadOnlyList<Album>> GetAllAsync();

        Task<Album> GetByIdAsync(AlbumId id);

        Task<IReadOnlyList<Album>> SearchAsync(string keyword);

        Task UpsertAsync(Album album);

        Task UpsertRangeAsync(IEnumerable<Album> albums);

        Task DeleteAsync(AlbumId id);

        Task ClearAsync();
    }
}

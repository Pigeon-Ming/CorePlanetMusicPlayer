using CorePlanetMusicPlayer.Core.Albums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Albums
{
    public interface IAlbumService
    {
        Task<IReadOnlyList<Album>> GetAllAsync();

        Task<Album> GetByIdAsync(AlbumId albumId);

        Task<AlbumDetails> GetDetailsAsync(AlbumId albumId);

        Task UpdateDescriptionAsync(AlbumId albumId, string description);
    }
}

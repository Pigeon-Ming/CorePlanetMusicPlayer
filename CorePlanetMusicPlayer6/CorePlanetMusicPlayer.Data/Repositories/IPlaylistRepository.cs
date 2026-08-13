using CorePlanetMusicPlayer.Core.Playlists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories
{
    public interface IPlaylistRepository
    {
        Task<IReadOnlyList<Playlist>> GetAllAsync();

        Task<Playlist> GetByIdAsync(PlaylistId id);

        Task UpsertAsync(Playlist playlist);

        Task DeleteAsync(PlaylistId id);

        Task ClearAsync();
    }
}

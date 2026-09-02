using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Core.Playlists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Playlists
{
    public interface IPlaylistService
    {
        Task<IReadOnlyList<Playlist>> GetAllAsync();

        Task<Playlist> GetByIdAsync(PlaylistId playlistId);

        Task<Playlist> CreateAsync(string name, string description);

        Task RenameAsync(PlaylistId playlistId, string name);

        Task UpdateDescriptionAsync(PlaylistId playlistId, string description);

        Task DeleteAsync(PlaylistId playlistId);

        Task<PlaylistItem> AddMusicAsync(PlaylistId playlistId, MusicId musicId);

        Task RemoveItemAsync(PlaylistId playlistId, string itemId);

        Task MoveItemAsync(PlaylistId playlistId, string itemId, int newIndex);

        Task ClearItemsAsync(PlaylistId playlistId);
    }
}

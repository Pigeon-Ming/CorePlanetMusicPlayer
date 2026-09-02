using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Core.Playlists;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Playlists
{
    public sealed class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistService(IPlaylistRepository playlistRepository)
        {
            Guard.NotNull(playlistRepository, nameof(playlistRepository));

            _playlistRepository = playlistRepository;
        }

        public Task<IReadOnlyList<Playlist>> GetAllAsync()
        {
            return _playlistRepository.GetAllAsync();
        }

        public Task<Playlist> GetByIdAsync(PlaylistId playlistId)
        {
            if (playlistId.IsEmpty)
            {
                throw new ArgumentException("Playlist id cannot be empty.", nameof(playlistId));
            }

            return _playlistRepository.GetByIdAsync(playlistId);
        }
        public async Task<Playlist> CreateAsync(string name, string description)
        {
            Guard.NotNullOrWhiteSpace(name, nameof(name));

            var now = DateTimeOffset.Now;

            var playlist = new Playlist
            {
                Id = PlaylistId.NewId(),
                Name = name.Trim(),
                Description = description ?? string.Empty,
                Items = new List<PlaylistItem>(),
                CreatedAt = now,
                UpdatedAt = now
            };

            await _playlistRepository.UpsertAsync(playlist);

            return playlist;
        }

        public async Task UpdateDescriptionAsync(PlaylistId playlistId, string description)
        {
            if (playlistId.IsEmpty)
            {
                throw new ArgumentException("Playlist id cannot be empty.", nameof(playlistId));
            }

            var playlist = await GetExistingPlaylistAsync(playlistId);

            playlist.Description = description ?? string.Empty;
            playlist.UpdatedAt = DateTimeOffset.Now;

            await _playlistRepository.UpsertAsync(playlist);
        }

        public async Task RenameAsync(PlaylistId playlistId, string name)
        {
            if (playlistId.IsEmpty)
            {
                throw new ArgumentException("Playlist id cannot be empty.", nameof(playlistId));
            }

            Guard.NotNullOrWhiteSpace(name, nameof(name));

            var playlist = await GetExistingPlaylistAsync(playlistId);

            playlist.Name = name.Trim();
            playlist.UpdatedAt = DateTimeOffset.Now;

            await _playlistRepository.UpsertAsync(playlist);
        }
        public async Task DeleteAsync(PlaylistId playlistId)
        {
            if (playlistId.IsEmpty)
            {
                throw new ArgumentException("Playlist id cannot be empty.", nameof(playlistId));
            }

            await _playlistRepository.DeleteAsync(playlistId);
        }

        public async Task<PlaylistItem> AddMusicAsync(PlaylistId playlistId, MusicId musicId)
        {
            if (playlistId.IsEmpty)
            {
                throw new ArgumentException("Playlist id cannot be empty.", nameof(playlistId));
            }

            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }

            var playlist = await GetExistingPlaylistAsync(playlistId);
            EnsureItems(playlist);

            var item = new PlaylistItem
            {
                Id = EntityId.New(),
                MusicId = musicId,
                Order = playlist.Items.Count,
                AddedAt = DateTimeOffset.Now
            };

            playlist.Items.Add(item);
            playlist.UpdatedAt = DateTimeOffset.Now;

            await _playlistRepository.UpsertAsync(playlist);

            return item;
        }

        public async Task RemoveItemAsync(PlaylistId playlistId, string itemId)
        {
            if (playlistId.IsEmpty)
            {
                throw new ArgumentException("Playlist id cannot be empty.", nameof(playlistId));
            }

            Guard.NotNullOrWhiteSpace(itemId, nameof(itemId));

            var playlist = await GetExistingPlaylistAsync(playlistId);
            EnsureItems(playlist);

            var removed = false;

            for (int i = playlist.Items.Count - 1; i >= 0; i--)
            {
                var item = playlist.Items[i];

                if (item != null && item.Id == itemId)
                {
                    playlist.Items.RemoveAt(i);
                    removed = true;
                    break;
                }
            }

            if (!removed)
            {
                return;
            }

            ReorderItems(playlist);
            playlist.UpdatedAt = DateTimeOffset.Now;

            await _playlistRepository.UpsertAsync(playlist);
        }

        public async Task MoveItemAsync(PlaylistId playlistId, string itemId, int newIndex)
        {
            if (playlistId.IsEmpty)
            {
                throw new ArgumentException("Playlist id cannot be empty.", nameof(playlistId));
            }

            Guard.NotNullOrWhiteSpace(itemId, nameof(itemId));

            var playlist = await GetExistingPlaylistAsync(playlistId);
            EnsureItems(playlist);

            if(playlist.Items.Count == 0)
            {
                return;
            }

            var oldIndex = FindItemIndex(playlist.Items, itemId);

            if (oldIndex < 0)
            {
                return;
            }

            if (newIndex < 0)
            {
                newIndex = 0;
            }

            if (newIndex >= playlist.Items.Count)
            {
                newIndex = playlist.Items.Count - 1;
            }

            if (oldIndex == newIndex)
            {
                return;
            }

            var item = playlist.Items[oldIndex];
            playlist.Items.RemoveAt(oldIndex);
            playlist.Items.Insert(newIndex, item);

            ReorderItems(playlist);
            playlist.UpdatedAt = DateTimeOffset.Now;

            await _playlistRepository.UpsertAsync(playlist);
        }

        public async Task ClearItemsAsync(PlaylistId playlistId)
        {
            if (playlistId.IsEmpty)
            {
                throw new ArgumentException("Playlist id cannot be empty.", nameof(playlistId));
            }

            var playlist = await GetExistingPlaylistAsync(playlistId);
            EnsureItems(playlist);

            if (playlist.Items.Count == 0)
            {
                return;
            }

            playlist.Items.Clear();
            playlist.UpdatedAt = DateTimeOffset.Now;

            await _playlistRepository.UpsertAsync(playlist);
        }

        private async Task<Playlist> GetExistingPlaylistAsync(PlaylistId playlistId)
        {
            var playlist = await _playlistRepository.GetByIdAsync(playlistId);

            if (playlist == null)
            {
                throw new InvalidOperationException("Playlist does not exist.");
            }

            return playlist;
        }

        private static void EnsureItems(Playlist playlist)
        {
            if (playlist.Items == null)
            {
                playlist.Items = new List<PlaylistItem>();
            }
        }

        private static void ReorderItems(Playlist playlist)
        {
            if (playlist == null || playlist.Items == null)
            {
                return;
            }

            for (int i = 0; i < playlist.Items.Count; i++)
            {
                if (playlist.Items[i] != null)
                {
                    playlist.Items[i].Order = i;
                }
            }
        }

        private static int FindItemIndex(List<PlaylistItem> items, string itemId)
        {
            if (items == null)
            {
                return -1;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (item != null && item.Id == itemId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}

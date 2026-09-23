using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Playlists
{
    public sealed class Playlist
    {
        public PlaylistId Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public int ItemCount
        {
            get { return Items == null ? 0 : Items.Count; }
        }

        public bool HasName
        {
            get { return !string.IsNullOrWhiteSpace(Name); }
        }

        public static Playlist Create(string name)
        {
            Guard.NotNullOrWhiteSpace(name, nameof(name));

            return new Playlist
            {
                Id = PlaylistId.NewId(),
                Name = name,
                Description = string.Empty,
                Items = new List<PlaylistItem>(),
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        public void Rename(string name)
        {
            Guard.NotNullOrWhiteSpace(name ,nameof(name));

            Name = name;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void SetDescription(string description)
        {
            Description = description ?? string.Empty;
            UpdatedAt = DateTimeOffset.Now;
        }

        public PlaylistItem AddMusic(MusicId musicId)
        {
            if (Items == null)
            {
                Items = new List<PlaylistItem>();
            }

            var item = PlaylistItem.Create(musicId, Items.Count);

            Items.Add(item);
            UpdatedAt = DateTimeOffset.Now;

            return item;
        }

        public bool RemoveItem(string itemId)
        {
            Guard.NotNullOrWhiteSpace(itemId, nameof(itemId));

            if(Items == null)
            {
                return false;
            }

            for (int i = 0; i < Items.Count; i++)
            {
                if (string.Equals(Items[i].Id, itemId, StringComparison.Ordinal))
                {
                    Items.RemoveAt(i);
                    ReorderItems();
                    UpdatedAt = DateTimeOffset.Now;
                    return true;
                }
            }

            return false;
        }

        public bool ContainsMusic(MusicId musicId)
        {
            if (Items == null)
            {
                return false;
            }

            for(int i = 0; i < Items.Count; i++)
            {
                if (Items[i].MusicId == musicId)
                {
                    return true;
                }
            }

            return false;
        }

        public void MoveItem(string itemId, int newIndex)
        {
            Guard.NotNullOrWhiteSpace(itemId, nameof(itemId));
            Guard.NotNegative(newIndex, nameof(newIndex));

            if(Items == null || Items.Count == 0)
            {
                return;
            }

            if(newIndex >= Items.Count)
            {
                newIndex = Items.Count - 1;
            }

            PlaylistItem targetItem = null;
            int oldIndex = -1;

            for(int i = 0; i < Items.Count; i++)
            {
                if (string.Equals(Items[i].Id, itemId, StringComparison.Ordinal))
                {
                    targetItem = Items[i];
                    oldIndex = i;
                    break;
                }
            }

            if(targetItem == null || oldIndex == newIndex)
            {
                return;
            }

            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, targetItem);

            ReorderItems();
            UpdatedAt = DateTimeOffset.Now;
        }

        public void Clear()
        {
            if (Items == null)
            {
                Items = new List<PlaylistItem>();
            }
            else
            {
                Items.Clear();
            }

            UpdatedAt = DateTimeOffset.Now;
        }

        public void ReorderItems()
        {
            if(Items == null)
            {
                return;
            }

            for(int i = 0; i < Items.Count; i++)
            {
                Items[i].Order = i;
            }
        }
    }
}

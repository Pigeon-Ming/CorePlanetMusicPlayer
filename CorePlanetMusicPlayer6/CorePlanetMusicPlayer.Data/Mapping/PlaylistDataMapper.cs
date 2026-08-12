using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Core.Playlists;
using CorePlanetMusicPlayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Mapping
{
    public static class PlaylistDataMapper
    {
        public static Playlist ToModel(
            PlaylistEntity playlistEntity,
            IEnumerable<PlaylistItemEntity> itemEntities)
        {
            if (playlistEntity == null)
            {
                return null;
            }

            var playlist = new Playlist
            {
                Id = new PlaylistId(playlistEntity.Id),
                Name = playlistEntity.Name ?? string.Empty,
                Description = playlistEntity.Description ?? string.Empty,
                Items = new List<PlaylistItem>(),
                CreatedAt = DataValueConverter.FromUnixTimeMilliseconds(playlistEntity.CreatedAtUnixTimeMilliseconds),
                UpdatedAt = DataValueConverter.FromUnixTimeMilliseconds(playlistEntity.UpdatedAtUnixTimeMilliseconds)
            };

            if (itemEntities != null)
            {
                foreach (var itemEntity in itemEntities)
                {
                    var item = ToItemModel(itemEntity);

                    if (item != null)
                    {
                        playlist.Items.Add(item);
                    }
                }
            }

            playlist.Items.Sort(ComparePlaylistItems);

            return playlist;
        }

        public static PlaylistEntity ToEntity(Playlist playlist)
        {
            if (playlist == null)
            {
                return null;
            }

            return new PlaylistEntity
            {
                Id = playlist.Id.ToString(),
                Name = playlist.Name ?? string.Empty,
                Description = playlist.Description ?? string.Empty,
                CreatedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(playlist.CreatedAt),
                UpdatedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(playlist.UpdatedAt)
            };
        }

        public static List<PlaylistItemEntity> ToItemEntities(Playlist playlist)
        {
            var result = new List<PlaylistItemEntity>();

            if (playlist == null || playlist.Items == null)
            {
                return result;
            }

            for (int i = 0; i < playlist.Items.Count; i++)
            {
                var item = playlist.Items[i];

                if (item == null)
                {
                    continue;
                }

                result.Add(ToItemEntity(playlist.Id, item));
            }

            return result;
        }

        public static PlaylistItem ToItemModel(PlaylistItemEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new PlaylistItem
            {
                Id = entity.Id ?? string.Empty,
                MusicId = new MusicId(entity.MusicId),
                Order = entity.Order,
                AddedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.AddedAtUnixTimeMilliseconds)
            };
        }

        public static PlaylistItemEntity ToItemEntity(
            PlaylistId playlistId,
            PlaylistItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new PlaylistItemEntity
            {
                Id = item.Id ?? string.Empty,
                PlaylistId = playlistId.ToString(),
                MusicId = item.MusicId.ToString(),
                Order = item.Order,
                AddedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(item.AddedAt)
            };
        }

        private static int ComparePlaylistItems(PlaylistItem left, PlaylistItem right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Order.CompareTo(right.Order);
        }
    }
}

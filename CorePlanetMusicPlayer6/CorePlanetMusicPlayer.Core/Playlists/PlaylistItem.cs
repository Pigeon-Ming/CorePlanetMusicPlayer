using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Playlists
{
    public sealed class PlaylistItem
    {
        public string Id { get; set; } = string.Empty;

        public MusicId MusicId { get; set; }

        public int Order { get; set; }

        public DateTimeOffset AddedAt { get; set; }

        public static PlaylistItem Create(MusicId musicId, int order)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }

            Guard.NotNegative(order, nameof(order));

            return new PlaylistItem
            {
                Id = EntityId.New(),
                MusicId = musicId,
                Order = order,
                AddedAt = DateTimeOffset.Now
            };
        }
    }
}

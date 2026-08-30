using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Queue
{
    public sealed class PlaybackQueueItem
    {
        public string Id { get; set; } = string.Empty;

        public MusicId MusicId { get; set; }

        public int Order { get; set; }

        public static PlaybackQueueItem Create(MusicId musicId, int order)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }

            Guard.NotNegative(order, nameof(order));

            return new PlaybackQueueItem
            {
                Id = EntityId.New(),
                MusicId = musicId,
                Order = order
            };
        }
    }
}

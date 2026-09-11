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
        public string Id { get; }

        public MusicId MusicId { get; }

        public int Order { get; internal set; }

        private PlaybackQueueItem(string id, MusicId musicId, int order)
        {
            Guard.NotNullOrWhiteSpace(id, nameof(id));

            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }

            Guard.NotNegative(order, nameof(order));

            Id = id;
            MusicId = musicId;
            Order = order;
        }

        public static PlaybackQueueItem Create(MusicId musicId, int order)
        {
            return new PlaybackQueueItem(EntityId.New(), musicId, order);
        }

        public static PlaybackQueueItem Restore(string id, MusicId musicId, int order)
        {
            return new PlaybackQueueItem(id, musicId, order);
        }

        public PlaybackQueueItem Clone()
        {
            return new PlaybackQueueItem(Id, MusicId, Order);
        }
    }
}

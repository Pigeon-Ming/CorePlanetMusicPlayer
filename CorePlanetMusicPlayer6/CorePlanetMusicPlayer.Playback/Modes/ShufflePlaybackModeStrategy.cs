using CorePlanetMusicPlayer.Playback.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Modes
{
    /// <summary>
    /// 随机播放
    /// </summary>
    public sealed class ShufflePlaybackModeStrategy : IPlaybackModeStrategy
    {
        public PlaybackMode Mode
        {
            get { return PlaybackMode.Shuffle; }
        }

        public int GetNextIndex(PlaybackQueue queue)
        {
            return GetAdjacentIndex(queue, 1);
        }
         
        public int GetPreviousIndex(PlaybackQueue queue)
        {
            return GetAdjacentIndex(queue, -1);
        }

        private static int GetAdjacentIndex(PlaybackQueue queue, int offset)
        {
            if (queue == null || !queue.HasCurrent)
            {
                return -1;
            }

            var shuffleItemIds = queue.ShuffleItemIds;

            int currentShuffleIndex = queue.CurrentShuffleIndex;

            if (shuffleItemIds.Count == 0 || currentShuffleIndex < 0)
            {
                return -1;
            }

            int targetShuffleIndex = currentShuffleIndex + offset;

            if (targetShuffleIndex >= shuffleItemIds.Count)
            {
                targetShuffleIndex = 0;
            }
            else if (targetShuffleIndex < 0)
            {
                targetShuffleIndex = shuffleItemIds.Count - 1;
            }

            string targetItemId = shuffleItemIds[targetShuffleIndex];

            return queue.GetItemIndex(targetItemId);
        }
    }
}

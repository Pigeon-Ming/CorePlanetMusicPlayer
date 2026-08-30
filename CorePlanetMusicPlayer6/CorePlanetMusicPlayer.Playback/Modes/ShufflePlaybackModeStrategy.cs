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
        private readonly Random _random;

        public ShufflePlaybackModeStrategy()
        {
            _random = new Random();
        }

        public PlaybackMode Mode
        {
            get { return PlaybackMode.Shuffle; }
        }

        public int GetNextIndex(PlaybackQueue queue)
        {
            if (queue == null || !queue.HasCurrent || queue.Count == 0)
            {
                return -1;
            }

            if (queue.Count == 1)
            {
                return queue.CurrentIndex;
            }

            return GetRandomIndexExcept(queue.Count, queue.CurrentIndex);
        }

        public int GetPreviousIndex(PlaybackQueue queue)
        {
            if (queue == null || !queue.HasCurrent || queue.Count == 0)
            {
                return -1;
            }

            if (queue.Count == 1)
            {
                return queue.CurrentIndex;
            }

            return GetRandomIndexExcept(queue.Count, queue.CurrentIndex);
        }

        private int GetRandomIndexExcept(int count, int excludedIndex)
        {
            var index = _random.Next(count);

            while (index == excludedIndex)
            {
                index = _random.Next(count);
            }

            return index;
        }
    }
}

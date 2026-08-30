using CorePlanetMusicPlayer.Playback.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Modes
{
    /// <summary>
    /// 列表循环
    /// </summary>
    public sealed class RepeatAllPlaybackModeStrategy : IPlaybackModeStrategy
    {
        public PlaybackMode Mode
        {
            get { return PlaybackMode.RepeatAll; }
        }

        public int GetNextIndex(PlaybackQueue queue)
        {
            if (queue == null || !queue.HasCurrent || queue.Count == 0)
            {
                return -1;
            }

            var nextIndex = queue.CurrentIndex + 1;

            if (nextIndex >= queue.Count)
            {
                return 0;
            }

            return nextIndex;
        }

        public int GetPreviousIndex(PlaybackQueue queue)
        {
            if (queue == null || !queue.HasCurrent || queue.Count == 0)
            {
                return -1;
            }

            var previousIndex = queue.CurrentIndex - 1;

            if (previousIndex < 0)
            {
                return queue.Count - 1;
            }

            return previousIndex;
        }
    }
}

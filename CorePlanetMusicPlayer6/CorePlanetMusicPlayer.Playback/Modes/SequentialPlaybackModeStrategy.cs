using CorePlanetMusicPlayer.Playback.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Modes
{
    /// <summary>
    /// 顺序播放
    /// </summary>
    public sealed class SequentialPlaybackModeStrategy : IPlaybackModeStrategy
    {
        public PlaybackMode Mode
        {
            get { return PlaybackMode.Sequential; }
        }

        public int GetNextIndex(PlaybackQueue queue)
        {
            if (queue == null || !queue.HasCurrent)
            {
                return -1;
            }

            var nextIndex = queue.CurrentIndex + 1;

            if (nextIndex >= queue.Count)
            {
                return -1;
            }

            return nextIndex;
        }

        public int GetPreviousIndex(PlaybackQueue queue)
        {
            if (queue == null || !queue.HasCurrent)
            {
                return -1;
            }

            var previousIndex = queue.CurrentIndex - 1;

            if (previousIndex < 0)
            {
                return -1;
            }

            return previousIndex;
        }
    }
}

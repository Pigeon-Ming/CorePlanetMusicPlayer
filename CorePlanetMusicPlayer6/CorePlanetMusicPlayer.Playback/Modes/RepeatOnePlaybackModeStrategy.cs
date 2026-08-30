using CorePlanetMusicPlayer.Playback.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Modes
{
    /// <summary>
    /// 单曲播放
    /// </summary>
    public sealed class RepeatOnePlaybackModeStrategy : IPlaybackModeStrategy
    {
        public PlaybackMode Mode
        {
            get { return PlaybackMode.RepeatOne; }
        }

        public int GetNextIndex(PlaybackQueue queue)
        {
            if (queue == null || !queue.HasCurrent || queue.Count == 0)
            {
                return -1;
            }

            return queue.CurrentIndex;
        }

        public int GetPreviousIndex(PlaybackQueue queue)
        {
            if (queue == null || !queue.HasCurrent || queue.Count == 0)
            {
                return -1;
            }

            return queue.CurrentIndex;
        }
    }
}

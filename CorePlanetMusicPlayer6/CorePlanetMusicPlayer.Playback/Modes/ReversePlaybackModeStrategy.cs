using CorePlanetMusicPlayer.Playback.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Modes
{
    public sealed class ReversePlaybackModeStrategy : IPlaybackModeStrategy
    {
        public PlaybackMode Mode
        {
            get {  return PlaybackMode.Reverse; }
        }

        public int GetNextIndex(PlaybackQueue queue)
        {
            if (queue == null || !queue.HasCurrent)
            {
                return -1;
            }

            var nextIndex = queue.CurrentIndex - 1;

            if (nextIndex < 0)
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

            var previousIndex = queue.CurrentIndex + 1;

            if (previousIndex >= queue.Count)
            {
                return -1;
            }

            return previousIndex;
        }
    }
}

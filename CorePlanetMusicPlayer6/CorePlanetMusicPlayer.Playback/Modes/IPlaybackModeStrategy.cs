using CorePlanetMusicPlayer.Playback.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Modes
{
    public interface IPlaybackModeStrategy
    {
        PlaybackMode Mode { get; }

        int GetNextIndex(PlaybackQueue queue);

        int GetPreviousIndex(PlaybackQueue queue);
    }
}

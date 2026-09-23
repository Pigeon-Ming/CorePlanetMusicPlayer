using CorePlanetMusicPlayer.Playback.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Events
{
    public sealed class PlaybackPositionChangedEventArgs
    {
        public PlaybackPosition OldPosition { get; private set; }

        public PlaybackPosition NewPosition { get; private set; }

        public PlaybackPositionChangedEventArgs(PlaybackPosition oldPosition, PlaybackPosition newPosition)
        {
            OldPosition = oldPosition ?? PlaybackPosition.Empty();
            NewPosition = newPosition ?? PlaybackPosition.Empty();
        }
    }
}

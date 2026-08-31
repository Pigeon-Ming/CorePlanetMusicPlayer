using CorePlanetMusicPlayer.Playback.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Events
{
    public sealed class PlaybackStateChangedEventArgs
    {
        public PlaybackStatus OldStatus { get; private set; }
        
        public PlaybackStatus NewStatus { get; private set; }

        public PlaybackState State { get; private set; }

        public PlaybackStateChangedEventArgs(PlaybackStatus oldStatus, PlaybackStatus newStatus, PlaybackState state)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
            State = state;
        }
    }
}

using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Events
{
    public sealed class CurrentMusicChangedEventArgs
    {
        public MusicId? OldMusicId { get; private set; }

        public MusicId? NewMusicId { get; private set; }

        public CurrentMusicChangedEventArgs(MusicId? oldMusicId, MusicId? newMusicId)
        {
            OldMusicId = oldMusicId;
            NewMusicId = newMusicId;
        }
    }
}

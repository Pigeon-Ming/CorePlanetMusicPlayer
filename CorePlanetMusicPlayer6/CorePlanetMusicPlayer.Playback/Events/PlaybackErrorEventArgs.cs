using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Events
{
    public sealed class PlaybackErrorEventArgs
    {
        public MusicId? MusicId { get; private set; }

        public string ErrorMessage { get; private set; }

        public Exception Exception { get; private set; }

        public PlaybackErrorEventArgs(MusicId? musicId, string errorMessage, Exception exception)
        {
            MusicId = musicId;
            ErrorMessage = errorMessage ?? string.Empty;
            Exception = exception;
        }

        public PlaybackErrorEventArgs(MusicId? musicId, string errorMessage) : this(musicId, errorMessage, null)
        {

        }
    }
}

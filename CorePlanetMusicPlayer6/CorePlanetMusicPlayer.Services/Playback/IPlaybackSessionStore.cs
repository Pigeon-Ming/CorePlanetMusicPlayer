using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Playback
{
    public interface IPlaybackSessionStore
    {
        Task<PlaybackSessionData> LoadAsync();

        Task SaveAsync(PlaybackSessionData data);
    }
}

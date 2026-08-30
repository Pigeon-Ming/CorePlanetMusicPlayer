using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    public interface IAudioPlayer
    {
        PlaybackState Staus { get; }

        MusicId? CurrentMusicId { get; }

        PlaybackPosition Position { get; }

        VolumeLevel Volume { get; }

        Task LoadAsync(MusicId musicId);

        Task PlayAsync();

        Task PauseAsync();

        Task ResumeAsync();

        Task StopAsync();

        Task SeekAsync(TimeSpan timeSpan);

        Task SetVolumeAsync(double volume);
    }
}

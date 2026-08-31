using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using CorePlanetMusicPlayer.Playback.Modes;
using CorePlanetMusicPlayer.Playback.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    public interface IPlaybackService
    {
        PlaybackState State { get; }

        PlaybackQueueSnapshot QueueSnapshot { get; }

        event EventHandler<PlaybackStateChangedEventArgs> StateChanged;

        event EventHandler<CurrentMusicChangedEventArgs> CurrentMusicChanged;

        event EventHandler<PlaybackPositionChangedEventArgs> PositionChanged;

        event EventHandler<PlaybackErrorEventArgs> PlaybackError;

        Task PlayAsync(MusicId musicId);

        Task PlayQueueAsync(IEnumerable<MusicId> musicIds, MusicId startMusicId);

        Task PauseAsync();

        Task ResumeAsync();

        Task StopAsync();

        Task NextAsync();

        Task PreviousAsync();

        Task SeekAsync(TimeSpan position);

        Task SetVolumeAsync(double volume);

        Task SetPlaybackModeAsync(PlaybackMode mode);
    }
}

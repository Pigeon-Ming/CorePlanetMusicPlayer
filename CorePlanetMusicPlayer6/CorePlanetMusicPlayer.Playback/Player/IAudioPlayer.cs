using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    public interface IAudioPlayer
    {
        /// <summary>
        /// 底层媒体当前是否真正处于播放状态。
        /// 加载、缓冲、暂停、结束时为 false。
        /// </summary>
        bool IsActuallyPlaying { get; }

        PlaybackStatus Status { get; }

        MusicId? CurrentMusicId { get; }

        PlaybackPosition Position { get; }

        VolumeLevel Volume { get; }

        /// <summary>
        /// 实际播放活动发生变化，接收方应重新读取最新状态。
        /// </summary>
        event EventHandler PlaybackActivityChanged;

        event EventHandler PlaybackEnded;

        event EventHandler<PlaybackErrorEventArgs> PlaybackError;        

        Task LoadAsync(MusicId musicId);

        Task PlayAsync();

        Task PauseAsync();

        Task ResumeAsync();

        Task StopAsync();

        Task SeekAsync(TimeSpan timeSpan);

        Task SetVolumeAsync(double volume);
    }
}

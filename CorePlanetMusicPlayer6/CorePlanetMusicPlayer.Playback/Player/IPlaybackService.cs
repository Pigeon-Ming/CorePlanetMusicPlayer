using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using CorePlanetMusicPlayer.Playback.Hisrory;
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

        bool IsHistoryRecordingEnabled { get; }

        event EventHandler QueueChanged;

        event EventHandler PlaybackModeChanged;

        event EventHandler<PlaybackStateChangedEventArgs> StateChanged;

        event EventHandler<CurrentMusicChangedEventArgs> CurrentMusicChanged;

        event EventHandler<PlaybackPositionChangedEventArgs> PositionChanged;

        event EventHandler<PlaybackErrorEventArgs> PlaybackError;

        /// <summary>
        /// 有历史快照等待保存。
        /// 处理方法只应发送后台处理信号，不应直接执行数据库操作。
        /// </summary>
        event EventHandler HistorySnapshotAvailable;

        Task PlayAsync(MusicId musicId);

        /// <summary>
        /// 播放指定队列项，不替换队列。
        /// </summary>
        Task PlayQueueItemAsync(string itemId);

        Task PlayQueueAsync(IEnumerable<MusicId> musicIds, MusicId startMusicId);

        Task PauseAsync();

        Task ResumeAsync();

        Task StopAsync();

        Task NextAsync();

        Task PreviousAsync();

        Task<int> EnqueueNextAsync(IEnumerable<MusicId> musicIds);

        Task<int> EnqueueAsync(IEnumerable<MusicId> musicIds);

        /// <summary>
        /// 移除指定队列项；移除当前项时停止播放。
        /// </summary>
        Task RemoveQueueItemAsync(string itemId);

        /// <summary>
        /// 在普通顺序中移动一个位置，offset 只能是 -1 或 1。
        /// </summary>
        Task MoveQueueItemAsync(string itemId, int offset, PlaybackQueueOrder order);

        /// <summary>
        /// 停止播放并清空队列。
        /// </summary>
        Task ClearQueueAsync();

        Task SeekAsync(TimeSpan position);

        /// <summary>
        /// 从播放器读取最新位置，并同步播放状态。
        /// </summary>
        PlaybackPosition RefreshPosition();

        /// <summary>
        /// 暂停时继续播放；停止、结束或错误时重新播放当前队列项。
        /// 不替换现有队列。
        /// </summary>
        Task StartOrResumeAsync();

        Task SetVolumeAsync(double volume);

        Task SetPlaybackModeAsync(PlaybackMode mode);

        /// <summary>
        /// 恢复队列和播放模式，不自动播放。
        /// </summary>
        Task RestoreQueueAsync(PlaybackQueueSnapshot snapshot, PlaybackMode mode);

        /// <summary>
        /// 修改历史记录开关，不改变音乐播放状态。
        /// </summary>
        void SetHistoryRecordingEnabled(bool enabled);

        /// <summary>
        /// 读取队首历史快照，不移除。
        /// </summary>
        bool TryPeekHistorySnapshot(out PlaybackHistorySnapshot snapshot);

        /// <summary>
        /// 保存成功后，确认并移除指定的队首快照。
        /// 历史快照队列只允许一个保存服务消费。
        /// </summary>
        void AcknowledgeHistorySnapshot(PlaybackHistorySnapshot snapshot);


        /// <summary>
        /// 将当前播放过程的阶段快照加入保存队列。
        /// 不结束本次记录，不改变计时状态。
        /// </summary>
        void CaptureHistorySnapshot();

        /// <summary>
        /// 应用挂起前暂停历史计时，并产生阶段快照。
        /// </summary>
        void SuspendHistoryTracking();

        /// <summary>
        /// 应用恢复后，根据实际播放状态恢复历史计时。
        /// </summary>
        void ResumeHistoryTracking();
    }
}

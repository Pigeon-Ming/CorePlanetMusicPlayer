using CorePlanetMusicPlayer.Playback.Hisrory;
using CorePlanetMusicPlayer.Playback.Player;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.History
{
    /// <summary>
    /// 消费播放历史快照，并按顺序保存。
    /// 整个应用只创建一个实例。
    /// </summary>
    public sealed class PlaybackHistoryRecorder
    {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

        private readonly IPlaybackService _playbackService;
        private readonly IPlaybackHistoryService _historyService;

        private readonly object _startSync = new object();

        // 合并重复通知
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0, 1);

        // 后台自动保存和显式 FlushAsync 共用同一个保存入口。
        private readonly SemaphoreSlim _saveGate = new SemaphoreSlim(1, 1);

        private Task _workerTask;
        private string _lastError;

        public string LastError
        {
            get { return Volatile.Read(ref _lastError); }
        }

        public PlaybackHistoryRecorder(IPlaybackService playbackService, IPlaybackHistoryService historyService)
        {
            _playbackService = playbackService ?? throw new ArgumentNullException(nameof(playbackService));

            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        /// <summary>
        /// 数据库初始化完成后调用。
        /// 重复调用不会创建多个消费者。
        /// </summary>
        public void Start()
        {
            lock (_startSync)
            {
                if (_workerTask != null)
                {
                    return;
                }

                _playbackService.HistorySnapshotAvailable +=
                    PlaybackService_HistorySnapshotAvailable;

                _workerTask = Task.Run(ProcessLoopAsync);

                // 同时处理订阅之前已经产生的快照。
                SignalWorker();
            }
        }

        /// <summary>
        /// 尝试保存所有已排队快照。
        /// 失败时抛出异常，未成功保存的快照继续保留。
        /// 不会主动采集当前歌曲的新快照。
        /// </summary>
        public Task FlushAsync()
        {
            // 当前 SQLite 仓储执行的是同步数据库操作。
            // 显式转到后台，避免调用方承担同步写入工作。
            return Task.Run(FlushCoreAsync);
        }

        private void PlaybackService_HistorySnapshotAvailable(object sender, EventArgs e)
        {
            // 此处只发送信号，不访问数据库或历史队列。
            SignalWorker();
        }

        private void SignalWorker()
        {
            try
            {
                _signal.Release();
            }
            catch (SemaphoreFullException)
            {
                // 已有待处理信号，无需再增加。
            }
        }

        private async Task ProcessLoopAsync()
        {
            while (true)
            {
                await _signal.WaitAsync().ConfigureAwait(false);

                try
                {
                    await FlushAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"播放历史保存失败，稍后重试：{ex}");

                    // 失败的快照仍然留在原队列中。
                    // 等待期间不占用保存锁，也不阻塞播放。
                    await Task.Delay(RetryDelay).ConfigureAwait(false);

                    SignalWorker();
                }
            }
        }

        private async Task FlushCoreAsync()
        {
            await _saveGate.WaitAsync().ConfigureAwait(false);

            try
            {
                PlaybackHistorySnapshot snapshot;

                while (_playbackService.TryPeekHistorySnapshot(out snapshot))
                {
                    await _historyService.RecordPlaybackAsync(
                        snapshot.HistoryId,
                        snapshot.MusicId,
                        snapshot.PlayedAt,
                        snapshot.MusicDuration,
                        snapshot.PlayedDuration,
                        snapshot.LastPosition,
                        snapshot.TitleSnapshot,
                        snapshot.ArtistNameSnapshot,
                        snapshot.AlbumTitleSnapshot)
                    .ConfigureAwait(false);

                    // 只有写入成功，才移除这一份快照。
                    _playbackService.AcknowledgeHistorySnapshot(snapshot);
                }

                Volatile.Write(ref _lastError, null);
            }
            catch (Exception ex)
            {
                Volatile.Write(ref _lastError, $"播放历史保存失败：{ex.Message}");

                throw;
            }
            finally
            {
                _saveGate.Release();
            }
        }
    }
}

using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using CorePlanetMusicPlayer.Playback.Hisrory;
using CorePlanetMusicPlayer.Playback.Modes;
using CorePlanetMusicPlayer.Playback.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    public class PlaybackService : IPlaybackService
    {
        private readonly IAudioPlayer _audioPlayer;
        private readonly IPlaybackMusicResolver _musicResolver;
        private readonly PlaybackQueue _queue;
        private readonly Dictionary<PlaybackMode, IPlaybackModeStrategy> _strategies;
        private readonly PlaybackState _state;
        private readonly SemaphoreSlim _commandGate = new SemaphoreSlim(1, 1);

        private volatile PlaybackQueueSnapshot _publishedQueueSnapshot;


        private readonly PlaybackHistoryTracker _historyTracker = new PlaybackHistoryTracker();

        private readonly object _historySync = new object();

        private readonly Queue<PlaybackHistorySnapshot> _pendingHistorySnapshots = new Queue<PlaybackHistorySnapshot>();

        private MusicId _historyMusicId;

        private bool _historyRecordingEnabled = true;

        public bool IsHistoryRecordingEnabled
        {
            get
            {
                lock (_historySync)
                {
                    return _historyRecordingEnabled;
                }
            }
        }

        private bool _historyEnabled;
        private bool _historyWasPlaying;

        // 应用是否已暂停历史计时。
        // 防止挂起过程中到达的播放器通知重新启动计时。
        private bool _historySuspended;

        private int _historyGeneration;

        private string _historyTitleSnapshot = string.Empty;
        private string _historyArtistNameSnapshot = string.Empty;
        private string _historyAlbumTitleSnapshot = string.Empty;


        public event EventHandler QueueChanged;

        public event EventHandler PlaybackModeChanged;

        public event EventHandler<PlaybackStateChangedEventArgs> StateChanged;

        public event EventHandler<CurrentMusicChangedEventArgs> CurrentMusicChanged;

        public event EventHandler<PlaybackPositionChangedEventArgs> PositionChanged;

        public event EventHandler<PlaybackErrorEventArgs> PlaybackError;

        public event EventHandler HistorySnapshotAvailable;

        public PlaybackService(IAudioPlayer audioPlayer, PlaybackQueue queue, IEnumerable<IPlaybackModeStrategy> strategies, IPlaybackMusicResolver musicResolver)
        {
            Guard.NotNull(audioPlayer, nameof(audioPlayer));

            _audioPlayer = audioPlayer;
            _queue = queue ?? new PlaybackQueue();
            _publishedQueueSnapshot = _queue.CreateSnapshot();
            _strategies = new Dictionary<PlaybackMode, IPlaybackModeStrategy>();
            _state = PlaybackState.CreateDefault();
            _musicResolver = musicResolver ?? throw new ArgumentNullException(nameof(musicResolver));

            RegisterStrategies(strategies);
            RegisterMissingDefaultStrategies();

            _audioPlayer.PlaybackEnded += OnAudioPlayerPlaybackEnded;
            _audioPlayer.PlaybackError += OnAudioPlayerPlaybackError;
            _audioPlayer.PlaybackActivityChanged += OnAudioPlayerPlaybackActivityChanged;
        }

        public PlaybackState State
        {
            get { return _state; }
        }

        public PlaybackQueueSnapshot QueueSnapshot
        {
            get
            {
                var snapshot = _publishedQueueSnapshot;

                return new PlaybackQueueSnapshot
                {
                    Items = snapshot.Items
                        .Select(item => item.Clone())
                        .ToList(),

                    ShuffleItemIds = new List<string>(snapshot.ShuffleItemIds),

                    CurrentIndex = snapshot.CurrentIndex
                };
            }
        }

        public Task PlayAsync(MusicId musicId)
        {
            return ExecuteCommandAsync(() => PlayCoreAsync(musicId));
        }

        public Task PlayQueueAsync(IEnumerable<MusicId> musicIds, MusicId startMusicId)
        {
            var items = CopyMusicIds(musicIds);

            return ExecuteCommandAsync(() => PlayQueueCoreAsync(items, startMusicId));
        }

        public Task PauseAsync()
        {
            return ExecuteCommandAsync(PauseCoreAsync);
        }

        public Task ResumeAsync()
        {
            return ExecuteCommandAsync(ResumeCoreAsync);
        }

        public Task StopAsync()
        {
            return ExecuteCommandAsync(StopCoreAsync);
        }

        public Task NextAsync()
        {
            return ExecuteCommandAsync(NextCoreAsync);
        }

        public Task PreviousAsync()
        {
            return ExecuteCommandAsync(PreviousCoreAsync);
        }

        public Task<int> EnqueueAsync(IEnumerable<MusicId> musicIds)
        {
            var items = CopyMusicIds(musicIds);

            return ExecuteCommandAsync(() => EnqueueCoreAsync(items));
        }

        public Task<int> EnqueueNextAsync(IEnumerable<MusicId> musicIds)
        {
            var items = CopyMusicIds(musicIds);

            return ExecuteCommandAsync(() => EnqueueNextCoreAsync(items));
        }

        public Task SeekAsync(TimeSpan position)
        {
            return ExecuteCommandAsync(() => SeekCoreAsync(position));
        }

        public Task StartOrResumeAsync()
        {
            return ExecuteCommandAsync(StartOrResumeCoreAsync);
        }

        public Task SetVolumeAsync(double volume)
        {
            return ExecuteCommandAsync(() => SetVolumeCoreAsync(volume));
        }

        public Task PlayQueueItemAsync(string itemId)
        {
            return ExecuteCommandAsync(async () =>
            {
                if (!_queue.SetCurrentItemId(itemId))
                {
                    throw new InvalidOperationException("该队列项已不存在，请刷新队列。");
                }

                await PlayCurrentAsync();
            });
        }

        public Task RemoveQueueItemAsync(string itemId)
        {
            return ExecuteCommandAsync(async () =>
            {
                if (_queue.GetItemIndex(itemId) < 0)
                {
                    throw new InvalidOperationException("该队列项已不存在，请刷新队列。");
                }

                if (_queue.CurrentItemId == itemId)
                {
                    await StopCoreAsync();
                }

                _queue.RemoveItem(itemId);
            });
        }

        public Task MoveQueueItemAsync(string itemId, int offset, PlaybackQueueOrder order)
        {
            if (offset != -1 && offset != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (order != PlaybackQueueOrder.Normal && order != PlaybackQueueOrder.Shuffle)
            {
                throw new ArgumentOutOfRangeException(nameof(order));
            }

            return ExecuteCommandAsync(() =>
            {
                bool useShuffleOrder = order == PlaybackQueueOrder.Shuffle;

                int oldIndex = useShuffleOrder
                    ? _queue.GetShuffleItemIndex(itemId)
                    : _queue.GetItemIndex(itemId);

                if (oldIndex < 0)
                {
                    throw new InvalidOperationException("该队列项已不存在，请刷新队列。");
                }

                int newIndex = oldIndex + offset;

                if (newIndex < 0 || newIndex >= _queue.Count)
                {
                    return Task.CompletedTask;
                }

                if (useShuffleOrder)
                {
                    _queue.MoveShuffleItem(itemId, newIndex);
                }
                else
                {
                    _queue.MoveItem(itemId, newIndex);
                }

                return Task.CompletedTask;
            });
        }

        public Task ClearQueueAsync()
        {
            return ExecuteCommandAsync(async () =>
            {
                await StopCoreAsync();
                _queue.Clear();
            });
        }

        public Task SetPlaybackModeAsync(PlaybackMode mode)
        {
            return ExecuteCommandAsync(() => SetPlaybackModeCoreAsync(mode));
        }

        private async Task PlayCoreAsync(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }

            var musicIds = new List<MusicId>();
            musicIds.Add(musicId);

            _queue.SetItems(musicIds);
            _queue.SetCurrent(musicId);

            await PlayCurrentAsync();
        }

        private async Task PlayQueueCoreAsync(IEnumerable<MusicId> musicIds, MusicId startMusicId)
        {
            _queue.SetItems(musicIds);

            if (!_queue.HasItems)
            {
                await StopCoreAsync();
                return;
            }

            if (!startMusicId.IsEmpty)
            {
                _queue.SetCurrent(startMusicId);
            }

            await PlayCurrentAsync();
        }

        private async Task PauseCoreAsync()
        {
            if (!_state.IsPlaying)
            {
                return;
            }

            var oldStatus = _state.Status;

            await _audioPlayer.PauseAsync();

            SynchronizePlaybackHistory();

            _state.SetPaused();

            RaiseStateChanged(oldStatus, _state.Status);
        }

        private async Task ResumeCoreAsync()
        {
            if (!_state.IsPaused)
            {
                return;
            }

            var oldStatus = _state.Status;

            await _audioPlayer.ResumeAsync();

            SynchronizePlaybackHistory();

            if (_state.CurrentMusicId.HasValue)
            {
                _state.SetPlaying(_state.CurrentMusicId.Value);
            }

            RaiseStateChanged(oldStatus, _state.Status);
        }    

        private async Task StopCoreAsync()
        {
            var oldStatus = _state.Status;
            var oldMusicId = _state.CurrentMusicId;

            FinishPlaybackHistory();

            await _audioPlayer.StopAsync();

            _state.SetStopped();

            RaiseCurrentMusicChanged(oldMusicId, _state.CurrentMusicId);
            RaiseStateChanged(oldStatus, _state.Status);
        }

        private async Task NextCoreAsync()
        {
            var strategy = GetCurrentStrategy();
            var nextIndex = strategy.GetNextIndex(_queue);

            if (nextIndex < 0)
            {
                await EndCurrentPlaybackAsync();
                return;
            }

            await PlayIndexAsync(nextIndex);
        }

        private async Task PreviousCoreAsync()
        {
            var strategy = GetCurrentStrategy();
            var previousIndex = strategy.GetPreviousIndex(_queue);

            if (previousIndex < 0)
            {
                return;
            }

            await PlayIndexAsync(previousIndex);
        }

        private Task<int> EnqueueNextCoreAsync(IEnumerable<MusicId> musicIds)
        {
            int currentIndex = _queue.CurrentIndex;

            if (currentIndex < 0)
            {
                return EnqueueCoreAsync(musicIds);
            }

            int currentShuffleIndex = _queue.CurrentShuffleIndex;

            if (currentShuffleIndex < 0)
            {
                throw new InvalidOperationException("Current item must exist in the shuffle order.");
            }

            int insertIndex = _state.Mode == PlaybackMode.Reverse ? currentIndex : currentIndex + 1;

            int addedCount = _queue.Insert(musicIds, insertIndex, currentShuffleIndex + 1);

            return Task.FromResult(addedCount);
        }

        private Task<int> EnqueueCoreAsync(IEnumerable<MusicId> musicIds)
        {
            int addedCount = _queue.Enqueue(musicIds);

            return Task.FromResult(addedCount);
        }

        private async Task SeekCoreAsync(TimeSpan position)
        {
            Guard.NotNegative(position, nameof(position));

            var oldPosition = _state.Position;

            await _audioPlayer.SeekAsync(position);

            // 使用底层读取到的位置，包含底层对越界位置的修正。
            var newPosition = _audioPlayer.Position ?? PlaybackPosition.Empty();

            _state.UpdatePosition(newPosition);

            SynchronizePlaybackHistory();

            RaisePositionChanged(oldPosition, newPosition);
        }

        public PlaybackPosition RefreshPosition()
        {
            if (_commandGate.CurrentCount == 0)
            {
                return _state.Position ?? PlaybackPosition.Empty();
            }

            if (!_state.IsPlaying && !_state.IsPaused)
            {
                return _state.Position ?? PlaybackPosition.Empty();
            }

            var oldPosition = _state.Position ?? PlaybackPosition.Empty();
            var newPosition = _audioPlayer.Position ?? PlaybackPosition.Empty();

            if (oldPosition.Position != newPosition.Position || oldPosition.Duration != newPosition.Duration)
            {
                _state.UpdatePosition(newPosition);
                RaisePositionChanged(oldPosition, newPosition);
            }

            return newPosition;
        }

        public Task RestoreQueueAsync(PlaybackQueueSnapshot snapshot, PlaybackMode mode)
        {
            if (!Enum.IsDefined(typeof(PlaybackMode), mode))
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            // 先验证并复制输入，失败时不影响当前播放。
            var preparedQueue = new PlaybackQueue();
            preparedQueue.Restore(snapshot);

            var preparedSnapshot = preparedQueue.CreateSnapshot();

            return ExecuteCommandAsync(async () =>
            {
                var oldStatus = _state.Status;
                var oldMusicId = _state.CurrentMusicId;
                var oldPosition = _state.Position;

                FinishPlaybackHistory();

                await _audioPlayer.StopAsync();

                _queue.Restore(preparedSnapshot);

                _state.RestoreStopped(_queue.GetCurrent());
                _state.UpdateMode(mode);

                RaiseCurrentMusicChanged(oldMusicId, _state.CurrentMusicId);
                RaisePositionChanged(oldPosition, _state.Position);
                RaiseStateChanged(oldStatus, _state.Status);
            });
        }

        private async Task StartOrResumeCoreAsync()
        {
            if (_state.IsPlaying || _state.Status == PlaybackStatus.Loading)
            {
                return;
            }

            if (_state.IsPaused)
            {
                await ResumeCoreAsync();
                return;
            }

            if (!_queue.HasItems)
            {
                throw new InvalidOperationException("播放队列为空，请先从音乐库选择歌曲。");
            }

            if (!_queue.HasCurrent)
            {
                _queue.SetCurrentIndex(0);
            }

            await PlayCurrentAsync();
        }

        private async Task SetVolumeCoreAsync(double volume)
        {
            await _audioPlayer.SetVolumeAsync(volume);

            _state.UpdateVolume(VolumeLevel.Create(volume));
        }

        private Task SetPlaybackModeCoreAsync(PlaybackMode mode)
        {
            _state.UpdateMode(mode);

            return Task.CompletedTask;
        }

        private async Task PlayCurrentAsync()
        {
            var currentMusicId = _queue.GetCurrent();

            if (!currentMusicId.HasValue || currentMusicId.Value.IsEmpty)
            {
                await StopCoreAsync();
                return;
            }

            var oldStatus = _state.Status;
            var oldMusicId = _state.CurrentMusicId;

            try
            {
                // 队列可能已经指向新歌曲，
                // 但底层媒体此刻仍然是旧歌曲。
                FinishPlaybackHistory();

                // 明确停止旧媒体，避免加载新媒体时旧歌曲继续发声。
                await _audioPlayer.StopAsync();

                var music = await _musicResolver.GetByIdAsync(
                    currentMusicId.Value);

                if (music == null)
                {
                    throw new InvalidOperationException("歌曲已不在音乐库中，无法播放。");
                }

                PreparePlaybackHistory(music);

                _state.UpdatePosition(PlaybackPosition.Empty());
                _state.SetLoading(currentMusicId.Value);

                RaiseCurrentMusicChanged(
                    oldMusicId,
                    _state.CurrentMusicId);

                RaiseStateChanged(oldStatus, _state.Status);

                oldStatus = _state.Status;

                await _audioPlayer.LoadAsync(currentMusicId.Value);

                EnablePlaybackHistory();

                await _audioPlayer.PlayAsync();

                // 如果底层尚未实际播放，这里不会启动计时；
                // 后续实际状态通知会再次同步。
                SynchronizePlaybackHistory();

                _state.SetPlaying(currentMusicId.Value);

                _state.UpdatePosition(
                    _audioPlayer.Position ?? PlaybackPosition.Empty());

                _state.UpdateVolume(
                    _audioPlayer.Volume ?? VolumeLevel.Default());

                RaiseStateChanged(oldStatus, _state.Status);
            }
            catch (Exception ex)
            {
                HandlePlaybackError(currentMusicId, "播放失败。", ex);
            }
        }

        private async Task PlayIndexAsync(int index)
        {
            if (!_queue.SetCurrentIndex(index))
            {
                return;
            }

            await PlayCurrentAsync();
        }

        private async Task EndCurrentPlaybackAsync()
        {
            var oldStatus = _state.Status;
            var finalPosition = _audioPlayer.Position ?? PlaybackPosition.Empty();

            FinishPlaybackHistory(finalPosition);

            await _audioPlayer.StopAsync();

            _state.UpdatePosition(finalPosition);
            _state.SetEnded();

            RaiseStateChanged(oldStatus, _state.Status);
        }

        private IPlaybackModeStrategy GetCurrentStrategy()
        {
            IPlaybackModeStrategy strategy;

            if (_strategies.TryGetValue(_state.Mode, out strategy))
            {
                return strategy;
            }

            return _strategies[PlaybackMode.Sequential];
        }

        private void RegisterStrategies(IEnumerable<IPlaybackModeStrategy> strategies)
        {
            if (strategies == null)
            {
                return;
            }

            foreach (var strategy in strategies)
            {
                if (strategy == null)
                {
                    continue;
                }

                _strategies[strategy.Mode] = strategy;
            }
        }

        private void RegisterMissingDefaultStrategies()
        {
            if (!_strategies.ContainsKey(PlaybackMode.Sequential))
            {
                _strategies[PlaybackMode.Sequential] = new SequentialPlaybackModeStrategy();
            }

            if (!_strategies.ContainsKey(PlaybackMode.RepeatAll))
            {
                _strategies[PlaybackMode.RepeatAll] = new RepeatAllPlaybackModeStrategy();
            }

            if (!_strategies.ContainsKey(PlaybackMode.RepeatOne))
            {
                _strategies[PlaybackMode.RepeatOne] = new RepeatOnePlaybackModeStrategy();
            }

            if (!_strategies.ContainsKey(PlaybackMode.Shuffle))
            {
                _strategies[PlaybackMode.Shuffle] = new ShufflePlaybackModeStrategy();
            }

            if (!_strategies.ContainsKey(PlaybackMode.Reverse))
            {
                _strategies[PlaybackMode.Reverse] = new ReversePlaybackModeStrategy();
            }
        }

        private void HandlePlaybackError(MusicId? musicId, string message,Exception exception)
        {
            var oldStatus = _state.Status;

            FinishPlaybackHistory();

            _state.SetError(message);

            RaisePlaybackError(musicId, message, exception);
            RaiseStateChanged(oldStatus, _state.Status);
        }

        private void RaiseStateChanged(PlaybackStatus oldStatus, PlaybackStatus newStatus)
        {
            var handler = StateChanged;

            if (handler != null)
            {
                handler(this, new PlaybackStateChangedEventArgs(oldStatus, newStatus, _state));
            }
        }

        private void RaiseCurrentMusicChanged(MusicId? oldMusicId, MusicId? newMusicId)
        {
            if (AreSameMusic(oldMusicId, newMusicId))
            {
                return;
            }

            var handler = CurrentMusicChanged;

            if (handler != null)
            {
                handler(this, new CurrentMusicChangedEventArgs(oldMusicId, newMusicId));
            }
        }

        private void RaisePositionChanged(PlaybackPosition oldPosition, PlaybackPosition newPosition)
        {
            var handler = PositionChanged;

            if (handler != null)
            {
                handler(this, new PlaybackPositionChangedEventArgs(oldPosition, newPosition));
            }
        }

        private void RaisePlaybackError(MusicId? musicId, string message, Exception exception)
        {
            var handler = PlaybackError;

            if (handler != null)
            {
                handler(this, new PlaybackErrorEventArgs(musicId, message, exception));
            }
        }

        private static bool AreSameMusic(MusicId? left, MusicId? right)
        {
            if (!left.HasValue && !right.HasValue)
            {
                return true;
            }

            if (!left.HasValue || !right.HasValue)
            {
                return false;
            }

            return left.Value == right.Value;
        }

        private async void OnAudioPlayerPlaybackEnded(object sender, EventArgs e)
        {
            // 不等命令锁，先停止本次计时。
            int generation = FreezePlaybackHistory();

            try
            {
                await ExecuteCommandAsync(async () =>
                {
                    // 等待期间可能已经发生了手动切歌或停止。
                    if (!IsCurrentHistoryGeneration(generation))
                    {
                        return;
                    }

                    await NextCoreAsync();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void OnAudioPlayerPlaybackError(object sender, PlaybackErrorEventArgs e)
        {
            int generation = FreezePlaybackHistory();

            var musicId = e == null ? _state.CurrentMusicId : e.MusicId;

            var message = e == null ? "播放失败。" : e.ErrorMessage;

            var exception = e?.Exception;

            try
            {
                await ExecuteCommandAsync(() =>
                {
                    if (!IsCurrentHistoryGeneration(generation))
                    {
                        return Task.CompletedTask;
                    }

                    HandlePlaybackError(musicId, string.IsNullOrWhiteSpace(message) ? "播放失败。" : message, exception);

                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async Task ExecuteCommandAsync(Func<Task> operation)
        {
            await _commandGate.WaitAsync();

            var previousMode = _state.Mode;

            bool queueChanged = false;
            bool modeChanged = false;

            try
            {
                await operation();
            }
            finally
            {
                try
                {
                    var previous = _publishedQueueSnapshot;
                    var current = _queue.CreateSnapshot();

                    queueChanged =
                        previous.CurrentIndex != current.CurrentIndex ||
                        !previous.Items.Select(item => item.Id)
                            .SequenceEqual(current.Items.Select(item => item.Id)) ||
                        !previous.ShuffleItemIds
                            .SequenceEqual(current.ShuffleItemIds);

                    modeChanged = previousMode != _state.Mode;

                    _publishedQueueSnapshot = current;
                }
                finally
                {
                    _commandGate.Release();
                }

                if (queueChanged)
                {
                    QueueChanged?.Invoke(this, EventArgs.Empty);
                }

                if (modeChanged)
                {
                    PlaybackModeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private async Task<T> ExecuteCommandAsync<T>(Func<Task<T>> operation)
        {
            T result = default(T);

            await ExecuteCommandAsync(async () =>
            {
                result = await operation();
            });

            return result;
        }

        private static List<MusicId> CopyMusicIds(IEnumerable<MusicId> musicIds)
        {
            if (musicIds == null)
            {
                throw new ArgumentNullException(nameof(musicIds));
            }

            return musicIds.ToList();
        }

        public void SetHistoryRecordingEnabled(bool enabled)
        {
            lock (_historySync)
            {
                if (_historyRecordingEnabled == enabled)
                {
                    return;
                }

                _historyRecordingEnabled = enabled;

                if (!enabled)
                {
                    // 先停止计时，关闭期间不再累计。
                    _historyTracker.SetPlaying(false);
                    _historyWasPlaying = false;

                    if (_historyTracker.HasSession)
                    {
                        PlaybackPosition position = null;

                        try
                        {
                            position = _audioPlayer.Position;
                        }
                        catch (Exception ex)
                        {
                            // 无法取得当前位置时，保留跟踪器中最后的位置。
                            Debug.WriteLine(
                                $"关闭历史记录时读取播放位置失败：{ex}");
                        }

                        EnqueueHistorySnapshotCore(
                            _historyTracker.Finish(position));
                    }

                    return;
                }

                // 当前确实正在播放时，从现在开始一条新记录。
                // 暂停、加载或挂起状态下，由后续状态变化启动记录。
                SynchronizePlaybackHistory();
            }
        }

        /// <summary>
        /// 为即将加载的新歌曲建立跟踪上下文。
        /// 此时尚未开始计时。
        /// </summary>
        private void PreparePlaybackHistory(Music music)
        {
            if (music == null)
            {
                throw new ArgumentNullException(nameof(music));
            }

            if (music.Id.IsEmpty)
            {
                throw new ArgumentException("歌曲 ID 不能为空。", nameof(music));
            }

            lock (_historySync)
            {
                _historyGeneration++;

                _historyMusicId = music.Id;
                _historyEnabled = false;
                _historyWasPlaying = false;

                // 复制文字，不长期持有可变的 Music 对象。
                _historyTitleSnapshot = music.Title ?? string.Empty;
                _historyArtistNameSnapshot = music.ArtistName ?? string.Empty;
                _historyAlbumTitleSnapshot = music.AlbumTitle ?? string.Empty;
            }
        }

        private void EnablePlaybackHistory()
        {
            lock (_historySync)
            {
                _historyEnabled = true;
            }
        }

        /// <summary>
        /// 按底层当前状态同步计时。
        /// 不依赖界面的进度刷新定时器。
        /// </summary>
        private void SynchronizePlaybackHistory()
        {
            lock (_historySync)
            {
                if (!_historyRecordingEnabled || !_historyEnabled || _historySuspended)
                {
                    return;
                }

                var currentMusicId = _audioPlayer.CurrentMusicId;

                if (!currentMusicId.HasValue || currentMusicId.Value != _historyMusicId)
                {
                    return;
                }

                bool isPlaying = _audioPlayer.IsActuallyPlaying;
                var position = _audioPlayer.Position;

                if (isPlaying)
                {
                    if (!_historyTracker.HasSession)
                    {
                        // 只有实际开始播放，才创建历史 ID 和开始时间。
                        _historyTracker.Start(
                            _historyMusicId,
                            position,
                            _historyTitleSnapshot,
                            _historyArtistNameSnapshot,
                            _historyAlbumTitleSnapshot);
                    }
                    else
                    {
                        _historyTracker.UpdatePosition(position);
                        _historyTracker.SetPlaying(true);
                    }
                }
                else
                {
                    _historyTracker.SetPlaying(false);
                    _historyTracker.UpdatePosition(position);

                    // 从实际播放进入暂停或缓冲时，产生一个阶段快照。
                    if (_historyWasPlaying)
                    {
                        EnqueueHistorySnapshotCore(_historyTracker.CaptureSnapshot());
                    }
                }

                _historyWasPlaying = isPlaying;
            }
        }

        /// <summary>
        /// 暂停计时并取得当前播放代次，不结束历史记录。
        /// 用于处理已经到达的结束或错误通知。
        /// </summary>
        private int FreezePlaybackHistory()
        {
            lock (_historySync)
            {
                _historyTracker.SetPlaying(false);
                _historyWasPlaying = false;

                return _historyGeneration;
            }
        }

        private bool IsCurrentHistoryGeneration(int generation)
        {
            lock (_historySync)
            {
                return generation == _historyGeneration;
            }
        }

        /// <summary>
        /// 在旧媒体被替换或清空之前完成结算。
        /// </summary>
        private void FinishPlaybackHistory(PlaybackPosition position = null)
        {
            lock (_historySync)
            {
                _historyEnabled = false;
                _historyWasPlaying = false;

                // 使仍在等待执行的旧通知失效。
                _historyGeneration++;

                if (!_historyTracker.HasSession)
                {
                    return;
                }

                var finalPosition = position ?? _audioPlayer.Position;

                EnqueueHistorySnapshotCore(_historyTracker.Finish(finalPosition));
            }
        }

        // 调用此方法时必须已经持有 _historySync。
        private void EnqueueHistorySnapshotCore(PlaybackHistorySnapshot snapshot)
        {
            if (snapshot == null || snapshot.PlayedDuration <= TimeSpan.Zero)
            {
                return;
            }

            _pendingHistorySnapshots.Enqueue(snapshot);

            Debug.WriteLine(
                $"历史快照：{snapshot.HistoryId}，" +
                $"歌曲：{snapshot.MusicId}，" +
                $"累计时长：{snapshot.PlayedDuration}，" +
                $"最后位置：{snapshot.LastPosition}");

            HistorySnapshotAvailable?.Invoke(this, EventArgs.Empty);
        }

        private void OnAudioPlayerPlaybackActivityChanged(object sender, EventArgs e)
        {
            try
            {
                // 通知可能来自媒体线程，不在这里操作界面。
                SynchronizePlaybackHistory();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        public bool TryPeekHistorySnapshot(out PlaybackHistorySnapshot snapshot)
        {
            lock (_historySync)
            {
                if (_pendingHistorySnapshots.Count == 0)
                {
                    snapshot = null;
                    return false;
                }

                snapshot = _pendingHistorySnapshots.Peek();
                return true;
            }
        }

        public void AcknowledgeHistorySnapshot(PlaybackHistorySnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            lock (_historySync)
            {
                if (_pendingHistorySnapshots.Count == 0 || !ReferenceEquals(_pendingHistorySnapshots.Peek(), snapshot))
                {
                    throw new InvalidOperationException("待确认的历史快照不是当前队首，请检查是否存在多个历史保存服务。");
                }

                _pendingHistorySnapshots.Dequeue();
            }
        }


        /// <summary>
        /// 采集阶段快照，不结束当前播放过程。
        /// </summary>
        public void CaptureHistorySnapshot()
        {
            lock (_historySync)
            {
                if (!_historyRecordingEnabled || !_historyTracker.HasSession)
                {
                    return;
                }

                EnqueueHistorySnapshotCore(
                    _historyTracker.CaptureSnapshot(
                        _audioPlayer.Position));
            }
        }

        /// <summary>
        /// 挂起前暂停计时，并保留当前历史记录。
        /// </summary>
        public void SuspendHistoryTracking()
        {
            lock (_historySync)
            {
                if (_historySuspended)
                {
                    return;
                }

                _historySuspended = true;

                _historyTracker.SetPlaying(false);
                _historyWasPlaying = false;

                CaptureHistorySnapshot();
            }
        }

        /// <summary>
        /// 恢复后重新同步实际播放状态。
        /// </summary>
        public void ResumeHistoryTracking()
        {
            lock (_historySync)
            {
                if (!_historySuspended)
                {
                    return;
                }

                _historySuspended = false;

                SynchronizePlaybackHistory();
            }
        }
    }
}

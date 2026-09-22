using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using CorePlanetMusicPlayer.Playback.Modes;
using CorePlanetMusicPlayer.Playback.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    public class PlaybackService : IPlaybackService
    {
        private readonly IAudioPlayer _audioPlayer;
        private readonly PlaybackQueue _queue;
        private readonly Dictionary<PlaybackMode, IPlaybackModeStrategy> _strategies;
        private readonly PlaybackState _state;
        private readonly SemaphoreSlim _commandGate = new SemaphoreSlim(1, 1);

        private volatile PlaybackQueueSnapshot _publishedQueueSnapshot;


        public event EventHandler QueueChanged;

        public event EventHandler PlaybackModeChanged;

        public event EventHandler<PlaybackStateChangedEventArgs> StateChanged;

        public event EventHandler<CurrentMusicChangedEventArgs> CurrentMusicChanged;

        public event EventHandler<PlaybackPositionChangedEventArgs> PositionChanged;

        public event EventHandler<PlaybackErrorEventArgs> PlaybackError;

        public PlaybackService(IAudioPlayer audioPlayer, PlaybackQueue queue, IEnumerable<IPlaybackModeStrategy> strategies)
        {
            Guard.NotNull(audioPlayer, nameof(audioPlayer));

            _audioPlayer = audioPlayer;
            _queue = queue ?? new PlaybackQueue();
            _publishedQueueSnapshot = _queue.CreateSnapshot();
            _strategies = new Dictionary<PlaybackMode, IPlaybackModeStrategy>();
            _state = PlaybackState.CreateDefault();

            RegisterStrategies(strategies);
            RegisterMissingDefaultStrategies();

            _audioPlayer.PlaybackEnded += OnAudioPlayerPlaybackEnded;
            _audioPlayer.PlaybackError += OnAudioPlayerPlaybackError;
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

                    ShuffleItemIds =
                        new List<string>(snapshot.ShuffleItemIds),

                    CurrentIndex = snapshot.CurrentIndex
                };
            }
        }

        public Task PlayAsync(MusicId musicId)
        {
            return ExecuteCommandAsync(() => PlayCoreAsync(musicId));
        }

        public Task PlayQueueAsync(
            IEnumerable<MusicId> musicIds,
            MusicId startMusicId)
        {
            var items = CopyMusicIds(musicIds);

            return ExecuteCommandAsync(
                () => PlayQueueCoreAsync(items, startMusicId));
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

            var duration = oldPosition == null ? TimeSpan.Zero : oldPosition.Duration;

            var newPosition = PlaybackPosition.Create(position, duration);

            _state.UpdatePosition(newPosition);

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

            if(!currentMusicId.HasValue || currentMusicId.Value.IsEmpty)
            {
                await StopCoreAsync();
                return;
            }

            var oldStatus = _state.Status;
            var oldMusicId = _state.CurrentMusicId;

            try
            {
                _state.UpdatePosition(PlaybackPosition.Empty());

                _state.SetLoading(currentMusicId.Value);
                RaiseCurrentMusicChanged(oldMusicId, _state.CurrentMusicId);
                RaiseStateChanged(oldStatus, _state.Status);

                oldStatus = _state.Status;

                await _audioPlayer.LoadAsync(currentMusicId.Value);
                await _audioPlayer.PlayAsync();

                _state.SetPlaying(currentMusicId.Value);
                _state.UpdatePosition(_audioPlayer.Position ?? PlaybackPosition.Empty());
                _state.UpdateVolume(_audioPlayer.Volume ?? VolumeLevel.Default());

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
            try
            {
                await NextAsync();
            }
            catch (Exception ex)
            {
                HandlePlaybackError(_state.CurrentMusicId, "自动切歌失败。", ex);
            }
        }

        private void OnAudioPlayerPlaybackError(object sender, PlaybackErrorEventArgs e)
        {
            var musicId = e == null ? _state.CurrentMusicId : e.MusicId;
            var message = e == null ? "播放失败。" : e.ErrorMessage;
            var exception = e == null ? null : e.Exception;

            HandlePlaybackError(musicId, string.IsNullOrWhiteSpace(message) ? "播放失败。" : message, exception);
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

        private async Task<T> ExecuteCommandAsync<T>(
            Func<Task<T>> operation)
        {
            T result = default(T);

            await ExecuteCommandAsync(async () =>
            {
                result = await operation();
            });

            return result;
        }

        private static List<MusicId> CopyMusicIds(
            IEnumerable<MusicId> musicIds)
        {
            if (musicIds == null)
            {
                throw new ArgumentNullException(nameof(musicIds));
            }

            return musicIds.ToList();
        }
    }
}

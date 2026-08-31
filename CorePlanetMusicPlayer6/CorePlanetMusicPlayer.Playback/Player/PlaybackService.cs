using CorePlanetMusicPlayer.Core.Common;
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
    public class PlaybackService : IPlaybackService
    {
        private readonly IAudioPlayer _audioPlayer;
        private readonly PlaybackQueue _queue;
        private readonly Dictionary<PlaybackMode, IPlaybackModeStrategy> _strategies;
        private readonly PlaybackState _state;

        public event EventHandler<PlaybackStateChangedEventArgs> StateChanged;

        public event EventHandler<CurrentMusicChangedEventArgs> CurrentMusicChanged;

        public event EventHandler<PlaybackPositionChangedEventArgs> PositionChanged;

        public event EventHandler<PlaybackErrorEventArgs> PlaybackError;

        public PlaybackService(IAudioPlayer audioPlayer, PlaybackQueue queue, IEnumerable<IPlaybackModeStrategy> strategies)
        {
            Guard.NotNull(audioPlayer, nameof(audioPlayer));

            _audioPlayer = audioPlayer;
            _queue = queue ?? new PlaybackQueue();
            _strategies = new Dictionary<PlaybackMode, IPlaybackModeStrategy>();
            _state = PlaybackState.CreateDefault();

            RegisterStrategies(strategies);
            RegisterMissingDefaultStrategies();
        }

        public PlaybackState State
        {
            get { return _state; }
        }

        public PlaybackQueueSnapshot QueueSnapshot
        {
            get { return _queue.CreateSnapshot(); }
        }

        public async Task PlayAsync(MusicId musicId)
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

        public async Task PlayQueueAsync(IEnumerable<MusicId> musicIds, MusicId startMusicId)
        {
            _queue.SetItems(musicIds);

            if (!_queue.HasItems)
            {
                await StopAsync();
                return;
            }

            if (!startMusicId.IsEmpty)
            {
                _queue.SetCurrent(startMusicId);
            }

            await PlayCurrentAsync();
        }

        public async Task PauseAsync()
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

        public async Task ResumeAsync()
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

        public async Task StopAsync()
        {
            var oldStatus = _state.Status;
            var oldMusicId = _state.CurrentMusicId;

            await _audioPlayer.StopAsync();

            RaiseCurrentMusicChanged(oldMusicId, _state.CurrentMusicId);
            RaiseStateChanged(oldStatus, _state.Status);
        }

        public async Task NextAsync()
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

        public async Task PreviousAsync()
        {
            var strategy = GetCurrentStrategy();
            var previousIndex = strategy.GetPreviousIndex(_queue);

            if (previousIndex < 0)
            {
                return;
            }

            await PlayIndexAsync(previousIndex);
        }

        public async Task SeekAsync(TimeSpan position)
        {
            Guard.NotNegative(position, nameof(position));

            var oldPosition = _state.Position;

            await _audioPlayer.SeekAsync(position);

            var duration = oldPosition == null ? TimeSpan.Zero : oldPosition.Duration;

            var newPosition = PlaybackPosition.Create(position, duration);

            _state.UpdatePosition(newPosition);

            RaisePositionChanged(oldPosition, newPosition);
        }

        public async Task SetVolumeAsync(double volume)
        {
            await _audioPlayer.SetVolumeAsync(volume);

            _state.UpdateVolume(VolumeLevel.Create(volume));
        }

        public Task SetPlaybackModeAsync(PlaybackMode mode)
        {
            _state.UpdateMode(mode);

            return Task.CompletedTask;
        }

        private async Task PlayCurrentAsync()
        {
            var currentMusicId = _queue.GetCurrent();

            if(!currentMusicId.HasValue || currentMusicId.Value.IsEmpty)
            {
                await StopAsync();
                return;
            }

            var oldStatus = _state.Status;
            var oldMusicId = _state.CurrentMusicId;

            try
            {
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

            await _audioPlayer.StopAsync();

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
    }
}

using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using CorePlanetMusicPlayer.Playback.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace CorePlanetMusicPlayer.Uwp.Platform.Playback
{
    public sealed class UwpAudioPlayer : IAudioPlayer, IDisposable
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly UwpMediaSourceFactory _mediaSourceFactory;

        private PlaybackStatus _status;
        private MusicId? _currentMusicId;
        private PlaybackPosition _position;
        private VolumeLevel _volume;

        public event EventHandler PlaybackEnded;

        public event EventHandler<PlaybackErrorEventArgs> PlaybackError;

        public UwpAudioPlayer(UwpMediaSourceFactory mediaSourceFactory)
        {
            Guard.NotNull(mediaSourceFactory, nameof(mediaSourceFactory));

            _mediaSourceFactory = mediaSourceFactory;
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.AutoPlay = false;
            _mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;

            _status = PlaybackStatus.Stopped;
            _currentMusicId = null;
            _position = PlaybackPosition.Empty();
            _volume = VolumeLevel.Default();

            _mediaPlayer.MediaEnded += OnMediaEnded;
            _mediaPlayer.MediaFailed += OnMediaFailed;
        }

        public PlaybackStatus Status
        {
            get { return _status; }
        }

        public MusicId? CUrrentMusicId
        {
            get { return _currentMusicId; }
        }

        public PlaybackPosition Position
        {
            get { return GetCurrentPosition(); }
        }

        public VolumeLevel Volume
        {
            get { return _volume; }
        }

        public MediaPlayer NativeMediaPlayer
        {
            get { return _mediaPlayer; }
        }

        public async Task LoadAsync(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentNullException("Music id cannot be empty.", nameof(musicId));
            }

            _status = PlaybackStatus.Loading;
            _currentMusicId = musicId;

            var mediaSource = await _mediaSourceFactory.CreateAsync(musicId);

            if (mediaSource == null)
            {
                _status = PlaybackStatus.Error;
                throw new InvalidOperationException("无法创建媒体源。");
            }

            _mediaPlayer.Source = mediaSource;
            _position = PlaybackPosition.Empty();
        }

        public Task PlayAsync()
        {
            if (_mediaPlayer.Source == null)
            {
                return Task.FromResult<object>(null);
            }

            _mediaPlayer.Play();
            _status = PlaybackStatus.Playing;
            _position = GetCurrentPosition();

            return Task.FromResult<object>(null);
        }

        public Task PauseAsync()
        {
            if (_mediaPlayer.Source == null)
            {
                return Task.FromResult<object>(null);
            }

            _mediaPlayer.Pause();
            _status = PlaybackStatus.Paused;
            _position = GetCurrentPosition();

            return Task.FromResult<object>(null);
        }

        public Task ResumeAsync()
        {
            if (_mediaPlayer.Source == null)
            {
                return Task.FromResult<object>(null);
            }

            _mediaPlayer.Play();
            _status = PlaybackStatus.Playing;
            _position = GetCurrentPosition();

            return Task.FromResult<object>(null);
        }

        public Task StopAsync()
        {
            if (_mediaPlayer.Source != null)
            {
                _mediaPlayer.Pause();
                _mediaPlayer.Source = null;
            }

            _status = PlaybackStatus.Stopped;
            _currentMusicId = null;
            _position = PlaybackPosition.Empty();

            return Task.FromResult<object>(null);
        }

        public Task SeekAsync(TimeSpan position)
        {
            Guard.NotNegative(position, nameof(position));

            if (_mediaPlayer.Source == null)
            {
                return Task.FromResult<object>(null);
            }

            var session = _mediaPlayer.PlaybackSession;
            var duration = GetNaturalDuration();

            if (duration > TimeSpan.Zero && position > duration)
            {
                position = duration;
            }

            session.Position = position;
            _position = PlaybackPosition.Create(position, duration);

            return Task.FromResult<object>(null);
        }

        public Task SetVolumeAsync(double volume)
        {
            _volume = VolumeLevel.Create(volume);
            _mediaPlayer.Volume = _volume.Value;

            return Task.FromResult<object>(null);
        }
        public void Dispose()
        {
            _mediaPlayer.MediaEnded -= OnMediaEnded;
            _mediaPlayer.MediaFailed -= OnMediaFailed;
            _mediaPlayer.Dispose();
        }

        private PlaybackPosition GetCurrentPosition()
        {
            if (_mediaPlayer == null || _mediaPlayer.Source == null)
            {
                return _position ?? PlaybackPosition.Empty();
            }

            var session = _mediaPlayer.PlaybackSession;

            if (session == null)
            {
                return _position ?? PlaybackPosition.Empty();
            }

            var position = session.Position;
            var duration = GetNaturalDuration();

            _position = PlaybackPosition.Create(position, duration);

            return _position;
        }

        private TimeSpan GetNaturalDuration()
        {
            var session = _mediaPlayer.PlaybackSession;

            if (session == null)
            {
                return TimeSpan.Zero;
            }

            var duration = session.NaturalDuration;

            if (duration < TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            return duration;
        }

        private void OnMediaEnded(MediaPlayer sender, object args)
        {
            _status = PlaybackStatus.Ended;
            _position = GetCurrentPosition();

            var handler = PlaybackEnded;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            _status = PlaybackStatus.Error;
            
            var message = args == null ? "播放失败。" : args.ErrorMessage;
            
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "播放失败。";
            }

            var handler = PlaybackError;

            if (handler != null)
            {
                handler(this, new PlaybackErrorEventArgs(_currentMusicId, message));
            }
        }
    }
}

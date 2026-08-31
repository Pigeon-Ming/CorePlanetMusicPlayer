using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    public sealed class PlaybackState
    {
        public PlaybackStatus Status { get; private set; }

        public MusicId? CurrentMusicId { get; private set; }

        public PlaybackPosition Position { get; private set; }

        public VolumeLevel Volume { get; private set; }

        public PlaybackMode Mode { get; private set; }

        public string ErrorMessage { get; private set; }

        public DateTimeOffset UpdatedAt { get; private set; }
        
        public bool HasCurrentMusic
        {
            get { return CurrentMusicId.HasValue && !CurrentMusicId.Value.IsEmpty; }
        }

        public bool IsPlaying
        {
            get { return Status == PlaybackStatus.Playing; }
        }

        public bool IsPaused
        {
            get { return Status == PlaybackStatus.Paused; }
        }

        public bool HasError
        {
            get { return Status == PlaybackStatus.Error; }
        }

        public static PlaybackState CreateDefault()
        {
            return new PlaybackState
            {
                Status = PlaybackStatus.Stopped,
                CurrentMusicId = null,
                Position = PlaybackPosition.Empty(),
                Volume = VolumeLevel.Default(),
                Mode = PlaybackMode.Sequential,
                ErrorMessage = string.Empty,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        public void SetLoading(MusicId musicId)
        {
            CurrentMusicId = musicId;
            Status = PlaybackStatus.Loading;
            ErrorMessage = string.Empty;
            Touch();
        }

        public void SetPlaying(MusicId musicId)
        {
            CurrentMusicId = musicId;
            Status = PlaybackStatus.Playing;
            ErrorMessage = string.Empty;
            Touch();
        }

        public void SetPaused()
        {
            Status = PlaybackStatus.Paused;
            ErrorMessage = string.Empty;
            Touch();
        }

        public void SetStopped()
        {
            Status = PlaybackStatus.Stopped;
            CurrentMusicId = null;
            Position = PlaybackPosition.Empty();
            ErrorMessage = string.Empty;
            Touch();
        }

        public void SetEnded()
        {
            Status = PlaybackStatus.Ended;
            ErrorMessage = string.Empty;
            Touch();
        }

        public void SetError(string errorMessage)
        {
            Status = PlaybackStatus.Error;
            ErrorMessage = errorMessage ?? string.Empty;
            Touch();
        }

        public void UpdatePosition(PlaybackPosition position)
        {
            Position = position ?? PlaybackPosition.Empty();
            Touch();
        }

        public void UpdateVolume(VolumeLevel volume)
        {
            Volume = volume ?? VolumeLevel.Default();
            Touch();
        }

        public void UpdateMode(PlaybackMode mode)
        {
            Mode = mode;
            Touch();
        }

        private void Touch()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}

using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;

namespace CorePlanetMusicPlayer.Uwp.Platform.Playback
{
    public sealed class UwpSystemMediaControlsService
    {
        private readonly MediaPlayer _mediaPlayer;
        private readonly SystemMediaTransportControls _controls;

        public event EventHandler PlayRequested;

        public event EventHandler PauseRequested;

        public event EventHandler NextRequested;

        public event EventHandler PreviousRequested;

        public event EventHandler StopRequested;

        public UwpSystemMediaControlsService(MediaPlayer mediaPlayer)
        {
            if (mediaPlayer == null)
            {
                throw new ArgumentNullException(nameof(mediaPlayer));
            }
            
            _mediaPlayer = mediaPlayer;
            _controls = _mediaPlayer.SystemMediaTransportControls;

            Configure();
        }

        public void UpdatePlaybackStatus(PlaybackStatus status)
        {
            if(_controls == null)
            {
                return;
            }

            if (status == PlaybackStatus.Playing)
            {
                _controls.PlaybackStatus = MediaPlaybackStatus.Playing;
                return;
            }

            if (status == PlaybackStatus.Paused)
            {
                _controls.PlaybackStatus =
                    MediaPlaybackStatus.Paused;
                return;
            }

            if (status == PlaybackStatus.Loading)
            {
                _controls.PlaybackStatus =
                    MediaPlaybackStatus.Changing;
                return;
            }

            if (status == PlaybackStatus.Ended)
            {
                _controls.PlaybackStatus =
                    MediaPlaybackStatus.Stopped;
                return;
            }

            if (status == PlaybackStatus.Error)
            {
                _controls.PlaybackStatus =
                    MediaPlaybackStatus.Stopped;
                return;
            }

            _controls.PlaybackStatus = MediaPlaybackStatus.Stopped;
        }

        public void UpdateDisplay(Music music)
        {
            if (_controls == null)
            {
                return;
            }

            var updater = _controls.DisplayUpdater;

            updater.Type = MediaPlaybackType.Music;

            if (music == null)
            {
                updater.MusicProperties.Title = string.Empty;
                updater.MusicProperties.Artist = string.Empty;
                updater.MusicProperties.AlbumTitle = string.Empty;
                updater.Update();
                return;
            }

            updater.MusicProperties.Title = music.Title ?? string.Empty;
            updater.MusicProperties.Artist = music.ArtistName ?? string.Empty;
            updater.MusicProperties.AlbumArtist = music.AlbumTitle ?? string.Empty;

            updater.Update();
        }

        private void Configure()
        {
            if (_controls == null)
            {
                return;
            }

            _controls.IsEnabled = true;
            _controls.IsPlayEnabled = true;
            _controls.IsPauseEnabled = true;
            _controls.IsNextEnabled = true;
            _controls.IsPreviousEnabled = true;
            _controls.IsStopEnabled = true;

            _controls.PlaybackStatus = MediaPlaybackStatus.Stopped;
            _controls.ButtonPressed += OnButtonPressed;
        }

        private void OnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            if (args == null)
            {
                return;
            }

            if (args.Button == SystemMediaTransportControlsButton.Play)
            {
                Raise(PlayRequested);
            }

            if (args.Button == SystemMediaTransportControlsButton.Pause)
            {
                Raise(PauseRequested);
            }

            if (args.Button == SystemMediaTransportControlsButton.Next)
            {
                Raise(NextRequested);
            }

            if (args.Button == SystemMediaTransportControlsButton.Previous)
            {
                Raise(PreviousRequested);
            }

            if (args.Button == SystemMediaTransportControlsButton.Stop)
            {
                Raise(StopRequested);
            }
        }

        private void Raise(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}

using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using CorePlanetMusicPlayer.Playback.Modes;
using CorePlanetMusicPlayer.Playback.Player;
using CorePlanetMusicPlayer6.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.Dev
{
    public sealed partial class PlayingControl : UserControl
    {
        private AppServices _services;
        private IPlaybackService _playbackService;

        private bool _isAttached;
        private bool _isBusy;
        private bool _isUpdatingUi;

        private int _lifetimeVersion;
        private int _metadataVersion;
        private int _trackVersion;

        private bool _metadataInitialized;
        private MusicId? _metadataMusicId;

        private bool _seekPending;
        private double _pendingSeekSeconds;
        private MusicId? _pendingSeekMusicId;
        private int _pendingSeekTrackVersion;

        private bool _volumePending;
        private double _pendingVolume;

        private string _operationMessage = string.Empty;

        private readonly DispatcherTimer _refreshTimer =
            new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };

        private readonly DispatcherTimer _inputTimer =
            new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };

        private readonly KeyValuePair<PlaybackMode, string>[] _modeItems =
        {
            new KeyValuePair<PlaybackMode, string>(PlaybackMode.Sequential, "顺序播放"),
            new KeyValuePair<PlaybackMode, string>(PlaybackMode.RepeatAll, "列表循环"),
            new KeyValuePair<PlaybackMode, string>(PlaybackMode.RepeatOne, "单曲循环"),
            new KeyValuePair<PlaybackMode, string>(PlaybackMode.Shuffle, "随机播放"),
            new KeyValuePair<PlaybackMode, string>(PlaybackMode.Reverse, "倒序播放")
        };

        public PlayingControl()
        {
            this.InitializeComponent();

            PlaybackModeComboBox.ItemsSource = _modeItems;

            _refreshTimer.Tick += RefreshTimer_Tick;
            _inputTimer.Tick += InputTimer_Tick;
        }

        private void UserControl_Loaded(
    object sender,
    RoutedEventArgs e)
        {
            Detach();

            _services = AppRuntime.Services;
            _playbackService = _services?.PlaybackService;

            if (_playbackService == null)
            {
                StatusTextBlock.Text = "播放服务尚未就绪。";
                return;
            }

            _isAttached = true;
            _metadataInitialized = false;
            _operationMessage = string.Empty;

            _playbackService.StateChanged += PlaybackService_StateChanged;

            RefreshSafely();
            _refreshTimer.Start();
        }

        private void UserControl_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            Detach();
        }

        private void Detach()
        {
            _isAttached = false;

            _lifetimeVersion++;
            _metadataVersion++;

            _refreshTimer.Stop();
            _inputTimer.Stop();

            if (_playbackService != null)
            {
                _playbackService.StateChanged -= PlaybackService_StateChanged;
            }

            _seekPending = false;
            _volumePending = false;
            _isBusy = false;

            _playbackService = null;
            _services = null;

            PreviousButton.IsEnabled = false;
            PlayPauseButton.IsEnabled = false;
            NextButton.IsEnabled = false;
            PositionSlider.IsEnabled = false;
            VolumeSlider.IsEnabled = false;
            PlaybackModeComboBox.IsEnabled = false;
        }

        private async void PlaybackService_StateChanged(
    object sender,
    PlaybackStateChangedEventArgs e)
        {
            int lifetime = _lifetimeVersion;

            try
            {
                Action update = () =>
                {
                    if (!_isAttached ||
                        lifetime != _lifetimeVersion ||
                        !ReferenceEquals(sender, _playbackService))
                    {
                        return;
                    }

                    if (e.NewStatus == PlaybackStatus.Loading)
                    {
                        _trackVersion++;
                        CancelPendingSeek();
                        _operationMessage = string.Empty;
                    }

                    RefreshSafely();
                };

                if (Dispatcher.HasThreadAccess)
                {
                    update();
                }
                else
                {
                    await Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        () => update());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void RefreshTimer_Tick(object sender, object e)
        {
            RefreshSafely();
        }

        private void RefreshSafely()
        {
            if (!_isAttached)
            {
                return;
            }

            try
            {
                RefreshView();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                StatusTextBlock.Text = $"状态读取失败：{ex.Message}";
            }
        }

        private void RefreshView()
        {
            var position = _playbackService.RefreshPosition();
            var state = _playbackService.State;

            UpdateMusicInfo(state.CurrentMusicId);

            bool canOperate =
                !_isBusy &&
                state.Status != PlaybackStatus.Loading;

            bool canSeek =
                canOperate &&
                (state.IsPlaying || state.IsPaused) &&
                position.Duration > TimeSpan.Zero;

            _isUpdatingUi = true;

            try
            {
                PlayPauseButtonFontIcon.Glyph = state.IsPlaying ? "\uE769" : "\uE768";

                PlayPauseButton.IsEnabled = canOperate;
                PreviousButton.IsEnabled = canOperate && state.HasCurrentMusic;
                NextButton.IsEnabled = canOperate && state.HasCurrentMusic;

                PositionSlider.IsEnabled = canSeek;
                VolumeSlider.IsEnabled = canOperate;
                PlaybackModeComboBox.IsEnabled = canOperate;

                DurationTextBlock.Text = FormatTime(position.Duration);

                if (!_seekPending)
                {
                    PositionSlider.Maximum = Math.Max(
                        1,
                        position.Duration.TotalSeconds);

                    PositionSlider.Value = Math.Max(
                        0,
                        Math.Min(
                            PositionSlider.Maximum,
                            position.Position.TotalSeconds));

                    PositionTextBlock.Text = FormatTime(position.Position);
                }

                if (!_volumePending)
                {
                    double volume = state.Volume?.Value ?? 1;

                    VolumeSlider.Value = volume * 100;
                    VolumeTextBlock.Text = $"{volume * 100:0}%";
                }

                if (!_isBusy)
                {
                    PlaybackModeComboBox.SelectedIndex = Array.FindIndex(
                        _modeItems,
                        item => item.Key == state.Mode);
                }

                StatusTextBlock.Text = state.HasError
                    ? $"播放失败：{state.ErrorMessage}"
                    : string.IsNullOrEmpty(_operationMessage)
                        ? GetStatusText(state.Status)
                        : _operationMessage;
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }

        private static string FormatTime(TimeSpan value)
        {
            if (value < TimeSpan.Zero)
            {
                value = TimeSpan.Zero;
            }

            return value.TotalHours >= 1
                ? $"{(long)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}"
                : $"{(long)value.TotalMinutes:00}:{value.Seconds:00}";
        }

        private static string GetStatusText(PlaybackStatus status)
        {
            switch (status)
            {
                case PlaybackStatus.Loading:
                    return "正在加载歌曲……";

                case PlaybackStatus.Playing:
                    return "正在播放。";

                case PlaybackStatus.Paused:
                    return "已暂停。";

                case PlaybackStatus.Ended:
                    return "播放已结束。";

                case PlaybackStatus.Error:
                    return "播放发生错误。";

                default:
                    return "尚未播放。";
            }
        }

        private void UpdateMusicInfo(MusicId? musicId)
        {
            if (_metadataInitialized &&
                Nullable.Equals(_metadataMusicId, musicId))
            {
                return;
            }

            _metadataInitialized = true;
            _metadataMusicId = musicId;

            int version = ++_metadataVersion;
            int lifetime = _lifetimeVersion;

            ArtistNameTextBlock.Text = string.Empty;
            AlbumTitleTextBlock.Text = string.Empty;

            if (!musicId.HasValue || musicId.Value.IsEmpty)
            {
                MusicTitleTextBlock.Text = "尚未播放";
                return;
            }

            MusicTitleTextBlock.Text = "正在读取歌曲资料……";

            _ = LoadMusicInfoAsync(
                musicId.Value,
                version,
                lifetime,
                _services);
        }

        private async Task LoadMusicInfoAsync(
            MusicId musicId,
            int version,
            int lifetime,
            AppServices services)
        {
            try
            {
                if (services.MusicQueryService == null)
                {
                    throw new InvalidOperationException(
                        "音乐查询服务尚未就绪。");
                }

                var music = await services.MusicQueryService.GetByIdAsync(
                    musicId);

                if (!CanApplyMusicInfo(musicId, version, lifetime))
                {
                    return;
                }

                if (music == null)
                {
                    MusicTitleTextBlock.Text = "歌曲资料已不在音乐库中";
                    return;
                }

                MusicTitleTextBlock.Text =
                    string.IsNullOrWhiteSpace(music.Title)
                        ? "未知标题"
                        : music.Title;

                ArtistNameTextBlock.Text =
                    string.IsNullOrWhiteSpace(music.ArtistName)
                        ? "未知艺术家"
                        : music.ArtistName;

                AlbumTitleTextBlock.Text =
                    string.IsNullOrWhiteSpace(music.AlbumTitle)
                        ? "未知专辑"
                        : music.AlbumTitle;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (CanApplyMusicInfo(musicId, version, lifetime))
                {
                    MusicTitleTextBlock.Text = "歌曲资料读取失败";
                    _operationMessage = ex.Message;
                }
            }
        }

        private bool CanApplyMusicInfo(
            MusicId musicId,
            int version,
            int lifetime)
        {
            return _isAttached &&
                   lifetime == _lifetimeVersion &&
                   version == _metadataVersion &&
                   Nullable.Equals(
                       _playbackService.State.CurrentMusicId,
                       (MusicId?)musicId);
        }

        private async Task RunCommandAsync(
    Func<IPlaybackService, Task> operation,
    bool cancelSeek = true)
        {
            if (!_isAttached || _isBusy)
            {
                return;
            }

            if (cancelSeek)
            {
                CancelPendingSeek();
            }

            var service = _playbackService;
            int lifetime = _lifetimeVersion;

            _isBusy = true;
            _operationMessage = string.Empty;

            RefreshSafely();

            try
            {
                await operation(service);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (_isAttached && lifetime == _lifetimeVersion)
                {
                    _operationMessage = $"操作失败：{ex.Message}";
                }
            }
            finally
            {
                if (_isAttached && lifetime == _lifetimeVersion)
                {
                    _isBusy = false;
                    RefreshSafely();
                }
            }
        }

        private async void PlayPauseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunCommandAsync(service =>
                service.State.IsPlaying
                    ? service.PauseAsync()
                    : service.StartOrResumeAsync());
        }

        private async void PreviousButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunCommandAsync(service => service.PreviousAsync());
        }

        private async void NextButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunCommandAsync(service => service.NextAsync());
        }

        private async void PlaybackModeComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!_isAttached || _isUpdatingUi || _isBusy)
            {
                return;
            }

            int index = PlaybackModeComboBox.SelectedIndex;

            if (index < 0 || index >= _modeItems.Length)
            {
                return;
            }

            var mode = _modeItems[index].Key;

            if (mode == _playbackService.State.Mode)
            {
                return;
            }

            await RunCommandAsync(
                service => service.SetPlaybackModeAsync(mode));
        }

        private void PositionSlider_ValueChanged(
    object sender,
    RangeBaseValueChangedEventArgs e)
        {
            if (!_isAttached ||
                _isUpdatingUi ||
                _isBusy ||
                !PositionSlider.IsEnabled)
            {
                return;
            }

            var state = _playbackService.State;
            double duration = state.Position?.Duration.TotalSeconds ?? 0;

            if (duration <= 0)
            {
                return;
            }

            _pendingSeekSeconds = Math.Max(
                0,
                Math.Min(duration, e.NewValue));

            _pendingSeekMusicId = state.CurrentMusicId;
            _pendingSeekTrackVersion = _trackVersion;
            _seekPending = true;

            PositionTextBlock.Text = FormatTime(
                TimeSpan.FromSeconds(_pendingSeekSeconds));

            RestartInputTimer();
        }

        private void VolumeSlider_ValueChanged(
            object sender,
            RangeBaseValueChangedEventArgs e)
        {
            if (!_isAttached || _isUpdatingUi || _isBusy)
            {
                return;
            }

            _pendingVolume = Math.Max(0, Math.Min(100, e.NewValue)) / 100;
            _volumePending = true;

            VolumeTextBlock.Text = $"{_pendingVolume * 100:0}%";

            RestartInputTimer();
        }

        private void RestartInputTimer()
        {
            _inputTimer.Stop();
            _inputTimer.Start();
        }

        private void CancelPendingSeek()
        {
            _seekPending = false;
            _pendingSeekMusicId = null;
        }

        private async void InputTimer_Tick(object sender, object e)
        {
            _inputTimer.Stop();

            if (!_isAttached)
            {
                return;
            }

            if (_isBusy)
            {
                _inputTimer.Start();
                return;
            }

            int lifetime = _lifetimeVersion;

            if (_seekPending)
            {
                var state = _playbackService.State;

                bool samePlayback =
                    _pendingSeekTrackVersion == _trackVersion &&
                    Nullable.Equals(
                        _pendingSeekMusicId,
                        state.CurrentMusicId);

                if (samePlayback && (state.IsPlaying || state.IsPaused))
                {
                    var target = TimeSpan.FromSeconds(
                        _pendingSeekSeconds);

                    await RunCommandAsync(
                        service => service.SeekAsync(target),
                        cancelSeek: false);
                }

                if (!_isAttached || lifetime != _lifetimeVersion)
                {
                    return;
                }

                _seekPending = false;
                RefreshSafely();
            }

            if (_volumePending)
            {
                double volume = _pendingVolume;

                await RunCommandAsync(
                    service => service.SetVolumeAsync(volume),
                    cancelSeek: false);

                if (!_isAttached || lifetime != _lifetimeVersion)
                {
                    return;
                }

                _volumePending = false;
                RefreshSafely();
            }
        }
    }
}

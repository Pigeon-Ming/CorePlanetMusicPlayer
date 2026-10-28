using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Playback.Events;
using CorePlanetMusicPlayer.Playback.Player;
using CorePlanetMusicPlayer6.Composition;
using CorePlanetMusicPlayer6.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace CorePlanetMusicPlayer6.Controls.Dev
{
    public sealed partial class LyricControl : UserControl
    {
        private const double CurrentLyricViewportRatio = 0.4;

        private readonly DispatcherTimer _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };

        private AppServices _services;
        private IPlaybackService _playbackService;
        private ScrollViewer _lyricsScrollViewer;

        private bool _isAttached;
        private bool _isBusy;
        private bool _isSeeking;
        private bool _isAutoFollowEnabled = true;

        private int _lifetimeVersion;
        private int _loadVersion;
        private int _scrollVersion;

        private MusicId? _currentMusicId;
        private LyricDocument _currentDocument;
        private List<LyricLineViewModel> _lyricItems =
            new List<LyricLineViewModel>();
        private int _currentLyricIndex = -1;

        public LyricControl()
        {
            this.InitializeComponent();

            _positionTimer.Tick += PositionTimer_Tick;

            LyricsListView.AddHandler(PointerPressedEvent, new PointerEventHandler(LyricsListView_PointerInteraction), true);

            LyricsListView.AddHandler(PointerWheelChangedEvent, new PointerEventHandler(LyricsListView_PointerInteraction), true);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            /*
             * Hub 中的内容可能被重新加载。
             * 先解除旧订阅，防止同一个控件重复订阅事件。
             */
            Detach();

            _services = AppRuntime.Services;
            _playbackService = _services?.PlaybackService;

            if (_services == null)
            {
                ApplyNoMusicState();
                StatusTextBlock.Text = "服务尚未就绪，请检查应用初始化。";
                return;
            }

            if (_playbackService == null)
            {
                ApplyNoMusicState();
                StatusTextBlock.Text = "播放服务尚未就绪。";
                return;
            }

            if (_services.MusicQueryService == null)
            {
                ApplyNoMusicState();
                StatusTextBlock.Text = "音乐查询服务尚未就绪。";
                return;
            }

            if (_services.LyricService == null)
            {
                ApplyNoMusicState();
                StatusTextBlock.Text = "歌词服务尚未就绪。";
                return;
            }

            _isAttached = true;

            _playbackService.CurrentMusicChanged += PlaybackService_CurrentMusicChanged;
            _playbackService.PositionChanged += PlaybackService_PositionChanged;

            _positionTimer.Start();

            await ReloadCurrentMusicAsync(_playbackService.State.CurrentMusicId);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Detach();
        }

        private void Detach()
        {
            _isAttached = false;
            _isBusy = false;
            _isSeeking = false;

            _positionTimer.Stop();

            /*
             * 所有已经开始的异步加载和滚动都会因为版本不匹配
             * 而放弃更新 UI。
             */
            _lifetimeVersion++;
            _loadVersion++;
            _scrollVersion++;

            if (_playbackService != null)
            {
                _playbackService.CurrentMusicChanged -= PlaybackService_CurrentMusicChanged;
                _playbackService.PositionChanged -= PlaybackService_PositionChanged;
            }

            _playbackService = null;
            _services = null;
            _lyricsScrollViewer = null;

            _currentMusicId = null;
            _currentDocument = null;
            _lyricItems = new List<LyricLineViewModel>();

            SetCurrentLyricIndex(-1);
            ResetAutoFollowState();
            ClearLyricView();

            ReloadLyricsButton.IsEnabled = false;
            FollowLyricsButton.IsEnabled = false;
        }

        private async void PlaybackService_CurrentMusicChanged(
            object sender,
            CurrentMusicChangedEventArgs e)
        {
            int lifetime = _lifetimeVersion;

            try
            {
                Action reload = () =>
                {
                    if (!_isAttached ||
                        lifetime != _lifetimeVersion ||
                        !ReferenceEquals(sender, _playbackService))
                    {
                        return;
                    }

                    MusicId? musicId = e == null
                        ? _playbackService.State.CurrentMusicId
                        : e.NewMusicId;

                    /*
                     * 事件处理器不能阻塞播放服务。
                     * ReloadCurrentMusicAsync 自己负责捕获异常。
                     */
                    _ = ReloadCurrentMusicAsync(musicId);
                };

                if (Dispatcher.HasThreadAccess)
                {
                    reload();
                }
                else
                {
                    await Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        () => reload());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void PlaybackService_PositionChanged(object sender, PlaybackPositionChangedEventArgs e)
        {
            int lifetime = _lifetimeVersion;

            TimeSpan position = e == null || e.NewPosition == null
                ? TimeSpan.Zero
                : e.NewPosition.Position;

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

                    /*
                     * PositionChanged 本身不携带 MusicId，
                     * 因此再次确认控件加载的歌曲仍是当前歌曲。
                     */
                    if (!_currentMusicId.HasValue ||
                        _playbackService == null ||
                        !Nullable.Equals(
                            _currentMusicId,
                            _playbackService.State.CurrentMusicId))
                    {
                        return;
                    }

                    UpdateCurrentLyric(position);
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

        private void PositionTimer_Tick(object sender, object e)
        {
            if (!_isAttached ||
                _playbackService == null ||
                _lyricItems == null ||
                _lyricItems.Count == 0)
            {
                return;
            }

            try
            {
                /*
                 * PlaybackService 的位置更新是拉取式的。
                 * RefreshPosition 会读取播放器当前位置，
                 * 并在位置发生变化时触发 PositionChanged。
                 */
                _playbackService.RefreshPosition();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private async void ReloadLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAttached || _playbackService == null)
            {
                return;
            }

            await ReloadCurrentMusicAsync(_playbackService.State.CurrentMusicId);
        }

        private void FollowLyricsButton_Click(object sender, RoutedEventArgs e)
        {
            bool shouldFollow = FollowLyricsButton.IsChecked == true;

            SetAutoFollowEnabled(shouldFollow);

            StatusTextBlock.Text = shouldFollow
                ? "已恢复自动跟随当前歌词。"
                : "已暂停自动跟随，仍会继续高亮当前歌词。";
        }

        private void LyricsListView_PointerInteraction(
            object sender,
            PointerRoutedEventArgs e)
        {
            if (!_isAttached ||
                _isBusy ||
                _lyricItems == null ||
                _lyricItems.Count == 0)
            {
                return;
            }

            if (_isAutoFollowEnabled)
            {
                SetAutoFollowEnabled(false);
                StatusTextBlock.Text = "手动浏览中，已暂停自动跟随。";
            }
        }

        private async void LyricsListView_ItemClick(
            object sender,
            ItemClickEventArgs e)
        {
            LyricLineViewModel item = e.ClickedItem as LyricLineViewModel;

            if (item == null ||
                !_isAttached ||
                _isBusy ||
                _isSeeking ||
                _playbackService == null ||
                !_currentMusicId.HasValue ||
                _currentMusicId.Value.IsEmpty)
            {
                return;
            }

            IPlaybackService playbackService = _playbackService;
            MusicId musicId = _currentMusicId.Value;
            int lifetime = _lifetimeVersion;

            _isSeeking = true;
            UpdateOperationState();

            try
            {
                await playbackService.SeekAsync(item.Time);

                if (!CanContinueInteraction(playbackService, musicId, lifetime))
                {
                    return;
                }

                UpdateCurrentLyric(item.Time);
                SetAutoFollowEnabled(true);

                StatusTextBlock.Text = $"已跳转至 {FormatLyricTime(item.Time)}。";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (CanContinueInteraction(playbackService, musicId, lifetime))
                {
                    StatusTextBlock.Text = $"跳转失败：{ex.Message}";
                }
            }
            finally
            {
                if (CanContinueInteraction(playbackService, musicId, lifetime))
                {
                    _isSeeking = false;
                    UpdateOperationState();
                }
            }
        }

        private async Task ReloadCurrentMusicAsync(MusicId? musicId)
        {
            int version = ++_loadVersion;
            int lifetime = _lifetimeVersion;

            AppServices services = _services;
            bool musicChanged = !Nullable.Equals(_currentMusicId, musicId);

            _currentMusicId = musicId;
            _currentDocument = null;
            _lyricItems = new List<LyricLineViewModel>();

            if (musicChanged)
            {
                ResetAutoFollowState();
            }

            ClearLyricView();

            if (!_isAttached || services == null)
            {
                return;
            }

            if (!musicId.HasValue || musicId.Value.IsEmpty)
            {
                _isBusy = false;
                ApplyNoMusicState();
                return;
            }

            _isBusy = true;
            ApplyLoadingState();
            UpdateOperationState();

            try
            {
                /*
                 * 歌曲资料和歌词互不依赖，可以同时读取。
                 */
                Task<Music> musicTask =
                    services.MusicQueryService.GetByIdAsync(musicId.Value);
                Task<LyricDocument> lyricTask =
                    services.LyricService.GetPreferredByMusicIdAsync(musicId.Value);

                await Task.WhenAll(musicTask, lyricTask);

                Music music = await musicTask;
                LyricDocument document = await lyricTask;

                if (!CanApplyResult(musicId.Value, version, lifetime, services))
                {
                    return;
                }

                ApplyLoadedData(music, document);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (CanApplyResult(musicId.Value, version, lifetime, services))
                {
                    ApplyLoadFailure(ex);
                }
            }
            finally
            {
                if (CanApplyResult(musicId.Value, version, lifetime, services))
                {
                    _isBusy = false;
                    UpdateOperationState();
                }
            }
        }

        private bool CanApplyResult(
            MusicId musicId,
            int version,
            int lifetime,
            AppServices services)
        {
            return _isAttached &&
                   lifetime == _lifetimeVersion &&
                   version == _loadVersion &&
                   ReferenceEquals(services, _services) &&
                   _playbackService != null &&
                   Nullable.Equals(
                       _playbackService.State.CurrentMusicId,
                       (MusicId?)musicId);
        }

        private bool CanContinueInteraction(
            IPlaybackService playbackService,
            MusicId musicId,
            int lifetime)
        {
            return _isAttached &&
                   lifetime == _lifetimeVersion &&
                   ReferenceEquals(playbackService, _playbackService) &&
                   Nullable.Equals(_currentMusicId, (MusicId?)musicId) &&
                   Nullable.Equals(
                       playbackService.State.CurrentMusicId,
                       (MusicId?)musicId);
        }

        private void ApplyNoMusicState()
        {
            _currentMusicId = null;
            _currentDocument = null;
            _lyricItems = new List<LyricLineViewModel>();

            ResetAutoFollowState();

            MusicTitleTextBlock.Text = "尚未播放";
            MusicArtistTextBlock.Text = string.Empty;
            LyricInfoTextBlock.Text = "尚未加载歌词";

            ClearLyricView();

            EmptyLyricsTextBlock.Text = "尚未播放歌曲";
            StatusTextBlock.Text = "请先从音乐列表播放一首歌曲。";

            UpdateOperationState();
        }

        private void ApplyLoadingState()
        {
            MusicTitleTextBlock.Text = "正在读取歌曲资料……";
            MusicArtistTextBlock.Text = string.Empty;
            LyricInfoTextBlock.Text = "正在加载歌词……";
            EmptyLyricsTextBlock.Text = "正在加载歌词……";
            StatusTextBlock.Text = "正在读取当前歌曲和歌词。";
        }

        private void ApplyLoadedData(Music music, LyricDocument document)
        {
            _currentDocument = document;

            if (music == null)
            {
                MusicTitleTextBlock.Text = "歌曲资料已不在音乐库中";
                MusicArtistTextBlock.Text = string.Empty;
            }
            else
            {
                MusicTitleTextBlock.Text = string.IsNullOrWhiteSpace(music.Title)
                    ? "未知标题"
                    : music.Title;

                MusicArtistTextBlock.Text = string.IsNullOrWhiteSpace(music.ArtistName)
                    ? "未知艺术家"
                    : music.ArtistName;
            }

            if (document == null)
            {
                _lyricItems = new List<LyricLineViewModel>();

                LyricInfoTextBlock.Text = "当前歌曲暂无歌词";

                ClearLyricView();

                EmptyLyricsTextBlock.Text = "当前歌曲暂无歌词";
                StatusTextBlock.Text = "歌词加载完成，当前歌曲没有已保存的歌词。";

                return;
            }

            _lyricItems = CreateLyricItems(document.Lines);

            string sourceText = GetLyricSourceText(document);

            LyricInfoTextBlock.Text = $"{sourceText}，共 {_lyricItems.Count} 行";

            if (_lyricItems.Count == 0)
            {
                ClearLyricView();

                EmptyLyricsTextBlock.Text = "歌词已读取，但没有可显示的同步歌词行";
                StatusTextBlock.Text = "歌词文档存在，但没有解析出同步歌词行。";

                return;
            }

            LyricsListView.ItemsSource = _lyricItems;
            LyricsListView.Visibility = Visibility.Visible;

            EmptyLyricsTextBlock.Visibility = Visibility.Collapsed;

            /*
             * 歌词可能在播放开始后才加载完成，
             * 因此加载后立即按照当前位置定位一次。
             */
            PlaybackPosition playbackPosition = _playbackService == null
                ? PlaybackPosition.Empty()
                : _playbackService.RefreshPosition();

            UpdateCurrentLyric(playbackPosition.Position);

            StatusTextBlock.Text = $"已加载 {_lyricItems.Count} 行歌词。";
        }

        private void ApplyLoadFailure(Exception exception)
        {
            _currentDocument = null;
            _lyricItems = new List<LyricLineViewModel>();

            ClearLyricView();

            LyricInfoTextBlock.Text = "歌词加载失败";
            EmptyLyricsTextBlock.Text = "无法加载当前歌曲的歌词";
            StatusTextBlock.Text = $"操作失败：{exception.Message}";
        }

        private void UpdateCurrentLyric(TimeSpan position)
        {
            if (_lyricItems == null ||
                _lyricItems.Count == 0 ||
                LyricsListView.ItemsSource == null)
            {
                SetCurrentLyricIndex(-1);
                return;
            }

            int index = FindCurrentLyricIndex(position);

            /*
             * 同一歌词行内可能收到很多次位置更新，
             * 只有索引变化时才更新高亮和滚动位置。
             */
            if (index == _currentLyricIndex)
            {
                return;
            }

            SetCurrentLyricIndex(index);

            if (_isAutoFollowEnabled)
            {
                RequestCurrentLyricScroll();
            }
        }

        private int FindCurrentLyricIndex(TimeSpan position)
        {
            if (_lyricItems == null || _lyricItems.Count == 0)
            {
                return -1;
            }

            if (position < TimeSpan.Zero)
            {
                position = TimeSpan.Zero;
            }

            int left = 0;
            int right = _lyricItems.Count - 1;
            int result = -1;

            while (left <= right)
            {
                int middle = left + ((right - left) / 2);
                LyricLineViewModel item = _lyricItems[middle];

                if (item.Time <= position)
                {
                    result = middle;
                    left = middle + 1;
                }
                else
                {
                    right = middle - 1;
                }
            }

            return result;
        }

        private void SetCurrentLyricIndex(int index)
        {
            if (_lyricItems != null &&
                _currentLyricIndex >= 0 &&
                _currentLyricIndex < _lyricItems.Count)
            {
                _lyricItems[_currentLyricIndex].SetCurrent(false);
            }

            _currentLyricIndex = index;

            if (_lyricItems != null &&
                _currentLyricIndex >= 0 &&
                _currentLyricIndex < _lyricItems.Count)
            {
                _lyricItems[_currentLyricIndex].SetCurrent(true);
            }
        }

        private void SetAutoFollowEnabled(bool isEnabled)
        {
            if (_isAutoFollowEnabled == isEnabled)
            {
                FollowLyricsButton.IsChecked = isEnabled;

                if (isEnabled)
                {
                    RequestCurrentLyricScroll();
                }

                return;
            }

            _isAutoFollowEnabled = isEnabled;
            FollowLyricsButton.IsChecked = isEnabled;
            FollowLyricsButton.Label = isEnabled ? "自动跟随" : "继续跟随";

            _scrollVersion++;

            if (isEnabled)
            {
                RequestCurrentLyricScroll();
            }
        }

        private void ResetAutoFollowState()
        {
            _isAutoFollowEnabled = true;
            FollowLyricsButton.IsChecked = true;
            FollowLyricsButton.Label = "自动跟随";
            _scrollVersion++;
        }

        private void RequestCurrentLyricScroll()
        {
            if (!_isAttached ||
                !_isAutoFollowEnabled ||
                _lyricItems == null ||
                _currentLyricIndex < 0 ||
                _currentLyricIndex >= _lyricItems.Count ||
                LyricsListView.Visibility != Visibility.Visible)
            {
                return;
            }

            int index = _currentLyricIndex;
            int version = ++_scrollVersion;
            int lifetime = _lifetimeVersion;
            LyricLineViewModel item = _lyricItems[index];

            /*
             * 先让虚拟化列表生成目标容器，再根据真实行高把歌词中心
             * 放到视口高度的 40% 处。翻译行换行后也能得到正确位置。
             */
            LyricsListView.ScrollIntoView(item);
            _ = PositionCurrentLyricAsync(item, index, version, lifetime);
        }

        private async Task PositionCurrentLyricAsync(
            LyricLineViewModel item,
            int index,
            int version,
            int lifetime)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => { });

            if (!CanApplyScroll(item, index, version, lifetime))
            {
                return;
            }

            LyricsListView.UpdateLayout();

            ListViewItem container =
                LyricsListView.ContainerFromItem(item) as ListViewItem;
            ScrollViewer scrollViewer = GetLyricsScrollViewer();

            if (container == null ||
                scrollViewer == null ||
                scrollViewer.ViewportHeight <= 0)
            {
                return;
            }

            Point itemPosition = container
                .TransformToVisual(scrollViewer)
                .TransformPoint(new Point(0, 0));

            double itemCenter = itemPosition.Y + (container.ActualHeight / 2);
            double targetInViewport =
                scrollViewer.ViewportHeight * CurrentLyricViewportRatio;
            double targetOffset =
                scrollViewer.VerticalOffset + itemCenter - targetInViewport;

            targetOffset = Math.Max(
                0,
                Math.Min(scrollViewer.ScrollableHeight, targetOffset));

            if (Math.Abs(targetOffset - scrollViewer.VerticalOffset) < 0.5)
            {
                return;
            }

            scrollViewer.ChangeView(null, targetOffset, null, false);
        }

        private bool CanApplyScroll(
            LyricLineViewModel item,
            int index,
            int version,
            int lifetime)
        {
            return _isAttached &&
                   _isAutoFollowEnabled &&
                   lifetime == _lifetimeVersion &&
                   version == _scrollVersion &&
                   _lyricItems != null &&
                   index == _currentLyricIndex &&
                   index >= 0 &&
                   index < _lyricItems.Count &&
                   ReferenceEquals(item, _lyricItems[index]);
        }

        private ScrollViewer GetLyricsScrollViewer()
        {
            if (_lyricsScrollViewer != null)
            {
                return _lyricsScrollViewer;
            }

            LyricsListView.ApplyTemplate();
            _lyricsScrollViewer = FindDescendant<ScrollViewer>(LyricsListView);

            return _lyricsScrollViewer;
        }

        private void ClearLyricView()
        {
            _scrollVersion++;

            SetCurrentLyricIndex(-1);

            LyricsListView.ItemsSource = null;
            LyricsListView.Visibility = Visibility.Collapsed;

            EmptyLyricsTextBlock.Visibility = Visibility.Visible;
        }

        private void UpdateOperationState()
        {
            bool hasMusic = _currentMusicId.HasValue && !_currentMusicId.Value.IsEmpty;
            bool hasLyrics = _lyricItems != null && _lyricItems.Count > 0;
            bool canOperate = _isAttached && !_isBusy && !_isSeeking;

            ReloadLyricsButton.IsEnabled = canOperate && hasMusic;
            FollowLyricsButton.IsEnabled = canOperate && hasLyrics;

        }

        private static List<LyricLineViewModel> CreateLyricItems(
            IEnumerable<LyricLine> lines)
        {
            var items = new List<LyricLineViewModel>();

            if (lines == null)
            {
                return items;
            }

            foreach (LyricLine line in lines)
            {
                if (line != null)
                {
                    items.Add(new LyricLineViewModel(line));
                }
            }

            return items;
        }

        private static T FindDescendant<T>(DependencyObject root)
            where T : DependencyObject
        {
            if (root == null)
            {
                return null;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(root);

            for (int index = 0; index < childCount; index++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, index);

                if (child is T match)
                {
                    return match;
                }

                T descendant = FindDescendant<T>(child);

                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        private static string FormatLyricTime(TimeSpan time)
        {
            if (time < TimeSpan.Zero)
            {
                time = TimeSpan.Zero;
            }

            return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
        }

        private static string GetLyricSourceText(LyricDocument document)
        {
            if (document == null)
            {
                return "未知来源";
            }

            if (document.IsUserOverride)
            {
                return "应用缓存";
            }

            switch (document.SourceType)
            {
                case LyricSourceType.ExternalFile:
                    return "外部歌词";
                case LyricSourceType.Embedded:
                    return "内嵌歌词";
                case LyricSourceType.Online:
                    return "在线歌词";
                default:
                    return "未知来源";
            }
        }
    }
}

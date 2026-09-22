using CorePlanetMusicPlayer.Playback.Modes;
using CorePlanetMusicPlayer.Playback.Player;
using CorePlanetMusicPlayer.Playback.Queue;
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
    public sealed partial class PlayQueueControl : UserControl
    {
        private AppServices _services;
        private IPlaybackService _playbackService;

        private PlaybackMode _displayMode = PlaybackMode.Sequential;

        private bool _isAttached;
        private bool _isBusy;
        private bool _isLoading;

        private int _loadVersion;
        private int _lifetimeVersion;

        private List<PlayQueueEntryView> _queueItems = new List<PlayQueueEntryView>();

        public PlayQueueControl()
        {
            this.InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Detach();

            _services = AppRuntime.Services;
            _playbackService = _services?.PlaybackService;

            if (_playbackService == null ||
                _services.MusicQueryService == null)
            {
                StatusTextBlock.Text = "播放或音乐查询服务尚未就绪。";
                return;
            }

            _isAttached = true;

            _playbackService.QueueChanged += PlaybackService_QueueChanged;
            _playbackService.PlaybackModeChanged += PlaybackService_QueueChanged;

            StatusTextBlock.Text = string.Empty;

            await ReloadAsync();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Detach();
        }

        private void Detach()
        {
            _isAttached = false;

            _loadVersion++;
            _lifetimeVersion++;

            if (_playbackService != null)
            {
                _playbackService.QueueChanged -= PlaybackService_QueueChanged;
                _playbackService.PlaybackModeChanged -= PlaybackService_QueueChanged;
            }

            _playbackService = null;
            _services = null;

            _isBusy = false;
            _isLoading = false;

            _queueItems = new List<PlayQueueEntryView>();
            QueueListView.ItemsSource = null;

            UpdateSelectionState();
        }

        public async Task ReloadAsync()
        {
            if (!_isAttached)
            {
                return;
            }

            int version = ++_loadVersion;

            var services = _services;
            var playbackService = _playbackService;

            _isLoading = true;
            UpdateSelectionState();

            try
            {
                var snapshot = playbackService.QueueSnapshot;
                var mode = playbackService.State.Mode;

                var displayItems = GetDisplayItems(snapshot, mode);

                // CurrentIndex 始终对应普通顺序，先取出当前项 ID。
                string currentItemId =
                    snapshot.CurrentIndex >= 0 &&
                    snapshot.CurrentIndex < snapshot.Items.Count
                        ? snapshot.Items[snapshot.CurrentIndex].Id
                        : null;

                var musicItems = await services.MusicQueryService.GetByIdsAsync(
                    snapshot.Items.Select(item => item.MusicId));

                // 查询结果可能含重复歌曲，字典只用于资料查找。
                var musicById = musicItems
                    .GroupBy(music => music.Id)
                    .ToDictionary(group => group.Key, group => group.First());

                var rows = new List<PlayQueueEntryView>();

                for (int i = 0; i < displayItems.Count; i++)
                {
                    var item = displayItems[i];

                    CorePlanetMusicPlayer.Core.Music.Music music;
                    musicById.TryGetValue(item.MusicId, out music);

                    rows.Add(new PlayQueueEntryView
                    {
                        ItemId = item.Id,
                        Number = i + 1,
                        Title = music?.Title ?? "歌曲已不在音乐库中",
                        ArtistName = music?.ArtistName ?? string.Empty,
                        AlbumTitle = music?.AlbumTitle ?? string.Empty,
                        IsCurrent = string.Equals(
                            item.Id,
                            currentItemId,
                            StringComparison.Ordinal),
                        IsMissing = music == null
                    });
                }

                if (!_isAttached || version != _loadVersion)
                {
                    return;
                }

                if (mode != playbackService.State.Mode)
                {
                    await ReloadAsync();
                    return;
                }

                string selectedItemId =
                    (QueueListView.SelectedItem as PlayQueueEntryView)?.ItemId;

                _displayMode = mode;
                _queueItems = rows;

                QueueListView.ItemsSource = _queueItems;

                QueueListView.SelectedItem = _queueItems.FirstOrDefault(
                    item => item.ItemId == selectedItemId);

                int missingCount = _queueItems.Count(item => item.IsMissing);

                string orderText = _displayMode == PlaybackMode.Shuffle
                    ? "随机顺序"
                    : "普通顺序";

                SummaryTextBlock.Text =
                    $"{orderText} · {_queueItems.Count} 个队列项" +
                    (missingCount > 0
                        ? $" · {missingCount} 个失效项"
                        : string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (_isAttached && version == _loadVersion)
                {
                    StatusTextBlock.Text =
                        $"队列列表读取失败，请刷新重试：{ex.Message}";
                }
            }
            finally
            {
                if (_isAttached && version == _loadVersion)
                {
                    _isLoading = false;
                    UpdateSelectionState();
                }
            }
        }

        private static List<PlaybackQueueItem> GetDisplayItems(PlaybackQueueSnapshot snapshot, PlaybackMode mode)
        {
            if (mode != PlaybackMode.Shuffle)
            {
                return snapshot.Items.ToList();
            }

            var itemsById = snapshot.Items.ToDictionary(
                item => item.Id,
                StringComparer.Ordinal);

            return snapshot.ShuffleItemIds
                .Select(itemId => itemsById[itemId])
                .ToList();
        }

        private async void PlaybackService_QueueChanged(object sender, EventArgs e)
        {
            int lifetime = _lifetimeVersion;

            try
            {
                await Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal,
                    () =>
                    {
                        if (!_isAttached ||
                            lifetime != _lifetimeVersion ||
                            !ReferenceEquals(sender, _playbackService))
                        {
                            return;
                        }

                        // ReloadAsync 内部负责捕获异常和过滤过期结果。
                        _ = ReloadAsync();
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void UpdateSelectionState()
        {
            if (StatusTextBlock == null)
            {
                return;
            }

            bool canOperate =
                _isAttached &&
                !_isBusy &&
                !_isLoading;

            QueueCommandBar.IsEnabled = canOperate;
            QueueListView.IsEnabled = canOperate;

            var selectedItem =
                QueueListView.SelectedItem as PlayQueueEntryView;

            int index = selectedItem == null
                ? -1
                : _queueItems.IndexOf(selectedItem);

            PlayButton.IsEnabled =
                canOperate &&
                selectedItem != null &&
                !selectedItem.IsMissing;

            RemoveButton.IsEnabled =
                canOperate && selectedItem != null;

            MoveUpButton.IsEnabled =
                canOperate && index > 0;

            MoveDownButton.IsEnabled =
                canOperate &&
                index >= 0 &&
                index < _queueItems.Count - 1;

            ClearButton.IsEnabled =
                canOperate && _queueItems.Count > 0;
        }

        private async Task RunOperationAsync(Func<IPlaybackService, Task> operation, string successMessage, bool checkPlaybackError = false)
        {
            if (!_isAttached || _isBusy || _isLoading)
            {
                return;
            }

            var service = _playbackService;
            int lifetime = _lifetimeVersion;

            _isBusy = true;
            UpdateSelectionState();

            StatusTextBlock.Text = "正在执行……";

            try
            {
                await operation(service);

                if (checkPlaybackError && service.State.HasError)
                {
                    throw new InvalidOperationException(
                        service.State.ErrorMessage);
                }

                if (_isAttached && lifetime == _lifetimeVersion)
                {
                    StatusTextBlock.Text = successMessage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (_isAttached && lifetime == _lifetimeVersion)
                {
                    StatusTextBlock.Text = $"操作失败：{ex.Message}";
                }
            }
            finally
            {
                if (_isAttached && lifetime == _lifetimeVersion)
                {
                    _isBusy = false;

                    // 即使操作失败，也重新读取实际队列。
                    await ReloadAsync();
                }
            }
        }

        private async void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = string.Empty;
            await ReloadAsync();
        }

        private void QueueListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionState();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var item = QueueListView.SelectedItem as PlayQueueEntryView;

            if (item == null)
            {
                return;
            }

            string itemId = item.ItemId;

            await RunOperationAsync(
                service => service.PlayQueueItemAsync(itemId),
                "已请求播放选中的队列项。",
                checkPlaybackError: true);
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var item = QueueListView.SelectedItem as PlayQueueEntryView;

            if (item == null)
            {
                return;
            }

            string itemId = item.ItemId;

            await RunOperationAsync(
                service => service.RemoveQueueItemAsync(itemId),
                "已移除选中的队列项。");
        }

        private async Task MoveSelectedAsync(int offset)
        {
            if (!_isAttached || _isBusy || _isLoading)
            {
                return;
            }

            var item = QueueListView.SelectedItem as PlayQueueEntryView;

            if (item == null)
            {
                return;
            }

            string itemId = item.ItemId;

            // 使用当前已经显示的顺序，固定本次操作的目标。
            var order = _displayMode == PlaybackMode.Shuffle
                ? PlaybackQueueOrder.Shuffle
                : PlaybackQueueOrder.Normal;

            string successMessage = order == PlaybackQueueOrder.Shuffle
                ? "随机队列顺序已更新。"
                : "普通队列顺序已更新。";

            await RunOperationAsync(
                service => service.MoveQueueItemAsync(itemId, offset, order),
                successMessage);
        }

        private async void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            await MoveSelectedAsync(-1);
        }

        private async void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            await MoveSelectedAsync(1);
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(
                service => service.ClearQueueAsync(),
                "已停止播放并清空队列。");
        }
    }

    public sealed class PlayQueueEntryView
    {
        public string ItemId { get; set; }

        public int Number { get; set; }

        public string Title { get; set; }

        public string ArtistName { get; set; }

        public string AlbumTitle { get; set; }

        public bool IsCurrent { get; set; }

        public bool IsMissing { get; set; }

        public string CurrentText
        {
            get { return IsCurrent ? "当前项" : string.Empty; }
        }
    }
}

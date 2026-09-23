using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Core.Playlists;
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
    public sealed partial class PlaylistsControl : UserControl
    {
        private bool _isBusy;

        private bool _isUpdatingPlaylistSelection;

        private List<Playlist> _playlistItems = new List<Playlist>();

        public PlaylistsControl()
        {
            this.InitializeComponent();

            PlaylistDetailsControl.PlaylistChanged += PlaylistDetailsControl_PlaylistChanged;

            PlaylistDetailsControl.SavingStateChanged += PlaylistDetailsControl_SavingStateChanged;
        }

        public Task ReloadAsync()
        {
            return RunOperationAsync(ReloadPlaylistsAsync);
        }

        private async Task<string> ReloadPlaylistsAsync(AppServices services)
        {
            if (services.PlaylistService == null)
            {
                throw new InvalidOperationException("播放列表服务尚未就绪。");
            }

            var playlists = await services.PlaylistService.GetAllAsync();

            _playlistItems = playlists.ToList();

            PlaylistsListView.SelectedItem = null;
            PlaylistDetailsControl.Clear();

            PlaylistsListView.ItemsSource = _playlistItems;

            UpdateSelectionState();

            return _playlistItems.Count == 0 ? "暂无播放列表，可以输入名称后新建。" : $"已读取 {_playlistItems.Count} 个播放列表。";
        }

        private async Task<string> CreatePlaylistAsync(AppServices services)
        {
            if (services.PlaylistService == null)
            {
                throw new InvalidOperationException("播放列表服务尚未就绪。");
            }

            // 在异步操作前读取输入。
            string name = NewPlaylistNameTextBox.Text;
            string description = NewPlaylistDescriptionTextBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("请输入播放列表名称。");
            }

            var playlist = await services.PlaylistService.CreateAsync(name.Trim(), description);

            // 到这里已经保存成功，再清空输入。
            NewPlaylistNameTextBox.Text = string.Empty;
            NewPlaylistDescriptionTextBox.Text = string.Empty;

            try
            {
                await ReloadPlaylistsAsync(services);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                return $"播放列表“{playlist.Name}”已创建，但列表刷新失败，请点击刷新列表。原因：{ex.Message}";
            }

            var createdItem = _playlistItems.FirstOrDefault(item => item.Id == playlist.Id);

            if (createdItem != null)
            {
                PlaylistsListView.SelectedItem = createdItem;
                PlaylistsListView.ScrollIntoView(createdItem);
            }

            return $"已创建播放列表“{playlist.Name}”，当前包含 0 个条目。";
        }

        private static async Task<List<MusicId>> GetPlaylistMusicIdsAsync(AppServices services, Playlist selectedPlaylist)
        {
            if (selectedPlaylist == null)
            {
                throw new InvalidOperationException("请先选择播放列表。");
            }

            if (services.PlaylistService == null)
            {
                throw new InvalidOperationException("播放列表服务尚未就绪。");
            }

            if (services.MusicQueryService == null)
            {
                throw new InvalidOperationException("音乐查询服务尚未就绪。");
            }

            var playlist = await services.PlaylistService.GetByIdAsync(selectedPlaylist.Id);

            if (playlist == null)
            {
                throw new InvalidOperationException("该播放列表已不存在，请刷新列表。");
            }

            if (playlist.Items == null || playlist.Items.Count == 0)
            {
                throw new InvalidOperationException("选中的播放列表没有歌曲。");
            }

            if (playlist.Items.Any(item => item == null || item.MusicId.IsEmpty))
            {
                throw new InvalidOperationException("播放列表包含无效条目，请检查播放列表内容。");
            }

            var musicIds = playlist.Items.OrderBy(item => item.Order).Select(item => item.MusicId).ToList();

            // GetByIdsAsync 保留重复项，并跳过数据库中不存在的歌曲。
            var musicItems = await services.MusicQueryService.GetByIdsAsync(musicIds);

            int missingCount = musicIds.Count - musicItems.Count;

            if (missingCount > 0)
            {
                throw new InvalidOperationException($"播放列表中有 {missingCount} 个条目对应的歌曲已不在音乐库中，请先整理播放列表。");
            }

            return musicIds;
        }

        private async Task<string> PlaySelectedAsync(AppServices services, Playlist selectedPlaylist)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = await GetPlaylistMusicIdsAsync(services, selectedPlaylist);

            await services.PlaybackService.PlayQueueAsync(musicIds, musicIds[0]);

            var state = services.PlaybackService.State;

            if (state.HasError)
            {
                return $"播放失败：{state.ErrorMessage}";
            }

            return $"已请求播放列表“{selectedPlaylist.Name}”，共 {musicIds.Count} 个条目；当前状态：{state.Status}。";
        }

        private async Task<string> EnqueueNextAsync(AppServices services, Playlist selectedPlaylist)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = await GetPlaylistMusicIdsAsync(services, selectedPlaylist);

            int addedCount = await services.PlaybackService.EnqueueNextAsync(musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            return $"已按下一首规则插入播放列表“{selectedPlaylist.Name}”的 {addedCount} 个条目，队列共 {queueCount} 项。";
        }

        private async Task<string> EnqueueSelectedAsync(AppServices services, Playlist selectedPlaylist)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = await GetPlaylistMusicIdsAsync(services, selectedPlaylist);

            int addedCount = await services.PlaybackService.EnqueueAsync(
                musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            return $"已将播放列表“{selectedPlaylist.Name}”的 {addedCount} 个条目加入队列，队列共 {queueCount} 项。";
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            bool isBlocked = _isBusy || PlaylistDetailsControl.IsSaving;

            PlaylistsCommandBar.IsEnabled = !isBlocked;
            PlaylistsListView.IsEnabled = !isBlocked;

            NewPlaylistNameTextBox.IsEnabled = !isBlocked;
            NewPlaylistDescriptionTextBox.IsEnabled = !isBlocked;

            // 父控件操作时禁用详情；
            // 详情自己的保存状态由其内部控制。
            PlaylistDetailsControl.IsEnabled = !_isBusy;

            UpdateSelectionState();
        }

        private void UpdateSelectionState()
        {
            var selectedPlaylist = PlaylistsListView.SelectedItem as Playlist;

            bool canPlay = !_isBusy && !(PlaylistDetailsControl?.IsSaving ?? false) && selectedPlaylist != null && selectedPlaylist.ItemCount > 0;

            PlayButton.IsEnabled = canPlay;
            PlayNextButton.IsEnabled = canPlay;
            AddToPlayQueueButton.IsEnabled = canPlay;
        }

        private async Task RunOperationAsync(Func<AppServices, Task<string>> operation)
        {
            if (_isBusy || PlaylistDetailsControl.IsSaving)
            {
                return;
            }

            var services = AppRuntime.Services;

            if (services == null)
            {
                StatusTextBlock.Text = "服务尚未就绪，请检查应用初始化。";
                return;
            }

            SetBusy(true);
            StatusTextBlock.Text = "正在执行……";

            try
            {
                StatusTextBlock.Text = await operation(services);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                StatusTextBlock.Text = $"操作失败：{ex.Message}";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private async void ReloadPlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private async void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            await RunOperationAsync(CreatePlaylistAsync);
        }

        private async void PlaylistsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionState();

            if (_isUpdatingPlaylistSelection || PlaylistDetailsControl == null)
            {
                return;
            }

            var selectedPlaylist =
                PlaylistsListView.SelectedItem as Playlist;

            if (selectedPlaylist == null)
            {
                PlaylistDetailsControl.Clear();
                return;
            }

            await PlaylistDetailsControl.ShowPlaylistAsync(selectedPlaylist.Id);
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPlaylist = PlaylistsListView.SelectedItem as Playlist;

            if (selectedPlaylist == null)
            {
                return;
            }

            await RunOperationAsync(services => PlaySelectedAsync(services, selectedPlaylist));
        }

        private async void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPlaylist = PlaylistsListView.SelectedItem as Playlist;

            if (selectedPlaylist == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueNextAsync(services, selectedPlaylist));
        }

        private async void AddToPlayQueueButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPlaylist = PlaylistsListView.SelectedItem as Playlist;

            if (selectedPlaylist == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueSelectedAsync(services, selectedPlaylist));
        }

        private void PlaylistDetailsControl_PlaylistChanged(Playlist playlist)
        {
            int index = _playlistItems.FindIndex(item => item.Id == playlist.Id);

            if (index < 0)
            {
                return;
            }

            var selectedPlaylist = PlaylistsListView.SelectedItem as Playlist;

            _playlistItems[index] = playlist;

            _isUpdatingPlaylistSelection = true;

            try
            {
                // Playlist 没有属性变更通知，重新绑定以更新名称和数量。
                PlaylistsListView.ItemsSource = null;
                PlaylistsListView.ItemsSource = _playlistItems;

                PlaylistsListView.SelectedItem = selectedPlaylist == null ? null : _playlistItems.FirstOrDefault(item => item.Id == selectedPlaylist.Id);
            }
            finally
            {
                _isUpdatingPlaylistSelection = false;
            }

            UpdateSelectionState();
        }

        private void PlaylistDetailsControl_SavingStateChanged(
    object sender,
    EventArgs e)
        {
            SetBusy(_isBusy);
        }
    }
}

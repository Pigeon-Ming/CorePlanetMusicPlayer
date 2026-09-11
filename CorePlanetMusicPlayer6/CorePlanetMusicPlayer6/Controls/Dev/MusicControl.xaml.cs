using CorePlanetMusicPlayer.Core.Music;
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
    public sealed partial class MusicControl : UserControl
    {
        private bool _isBusy;

        private List<Music> _musicItems = new List<Music>();

        public MusicControl()
        {
            this.InitializeComponent();
        }

        public Task ReloadAsync()
        {
            return RunOperationAsync(ReloadMusicAsync);
        }

        private async Task<string> ReloadMusicAsync(AppServices services)
        {
            if (services.MusicLibraryService == null)
            {
                throw new InvalidOperationException("音乐库服务尚未就绪。");
            }

            var musicList = await services.MusicLibraryService.GetAllMusicAsync();

            _musicItems = musicList.ToList();

            MusicListView.ItemsSource = _musicItems;

            UpdateSelectionState();

            return _musicItems.Count == 0 ? "音乐库中暂无歌曲。" : $"已读取 {_musicItems.Count} 首歌曲。";
        }

        private void UpdateSelectionState()
        {
            int selectedCount = MusicListView.SelectedItems.Count;

            bool canOperate = !_isBusy && selectedCount > 0;

            PlayButton.IsEnabled = canOperate;
            AddToPlayQueueButton.IsEnabled = canOperate;
            PlayNextButton.IsEnabled = canOperate;

            SelectionTextBlock.Text =
                $"共 {_musicItems.Count} 首，已选择 {selectedCount} 首。";
        }


        private List<Music> GetSelectedMusic()
        {
            var selectedItems = new HashSet<Music>(
                MusicListView.SelectedItems.OfType<Music>());

            // 保持列表中的展示顺序，不按用户点击的先后排列。
            return _musicItems.Where(music => selectedItems.Contains(music)).ToList();
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            MusicCommandBar.IsEnabled = !isBusy;
            MusicListView.IsEnabled = !isBusy;

            UpdateSelectionState();
        }

        private async Task<string> PlaySelectedAsync(AppServices services, IReadOnlyList<Music> selectedMusic)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            if (selectedMusic.Any(music => music.Id.IsEmpty))
            {
                return "选中歌曲包含空 ID，请检查音乐库数据。";
            }

            var musicIds = selectedMusic
                .Select(music => music.Id)
                .ToList();

            await services.PlaybackService.PlayQueueAsync(
                musicIds,
                musicIds[0]);

            var state = services.PlaybackService.State;

            if (state.HasError)
            {
                return $"播放失败：{state.ErrorMessage}";
            }

            return $"播放请求已处理，共 {musicIds.Count} 首；当前状态：{state.Status}。";
        }

        private async Task<string> EnqueueNextSelectedAsync(AppServices services, IReadOnlyList<Music> selectedMusic)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            if (selectedMusic.Count == 0)
            {
                return "请先选择歌曲。";
            }

            if (selectedMusic.Any(music => music.Id.IsEmpty))
            {
                return "选中歌曲包含空 ID，请检查音乐库数据。";
            }

            var musicIds = selectedMusic.Select(music => music.Id).ToList();

            int addedCount =
                await services.PlaybackService.EnqueueNextAsync(musicIds);

            var snapshot = services.PlaybackService.QueueSnapshot;

            return $"已插入 {addedCount} 首歌曲，播放队列共 {snapshot.Items.Count} 项。";
        }

        private async Task<string> EnqueueSelectedAsync(AppServices services, IReadOnlyList<Music> selectedMusic)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            if (selectedMusic.Count == 0)
            {
                return "请先选择歌曲。";
            }

            if (selectedMusic.Any(music => music.Id.IsEmpty))
            {
                return "选中歌曲包含空 ID，请检查音乐库数据。";
            }

            var musicIds = selectedMusic
                .Select(music => music.Id)
                .ToList();

            int addedCount = await services.PlaybackService.EnqueueAsync(
                musicIds);

            var snapshot = services.PlaybackService.QueueSnapshot;

            return $"已加入 {addedCount} 首歌曲，播放队列共 {snapshot.Items.Count} 项。";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadAsync();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedMusic = GetSelectedMusic();

            if (selectedMusic.Count == 0)
            {
                return;
            }

            await RunOperationAsync(services => PlaySelectedAsync(services, selectedMusic));
        }

        private void MultipleSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            MusicListView.SelectedItems.Clear();

            MusicListView.SelectionMode = MultipleSelectionButton.IsChecked == true ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;

            UpdateSelectionState();
        }

        private async void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedMusic = GetSelectedMusic();

            if (selectedMusic.Count == 0)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueNextSelectedAsync(services, selectedMusic));
        }

        private async void AddToPlayQueueButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedMusic = GetSelectedMusic();

            if (selectedMusic.Count == 0)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueSelectedAsync(services, selectedMusic));
        }

        private void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MusicListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionState();
        }

        private async Task RunOperationAsync(Func<AppServices, Task<string>> operation)
        {
            if (_isBusy)
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
    }
}

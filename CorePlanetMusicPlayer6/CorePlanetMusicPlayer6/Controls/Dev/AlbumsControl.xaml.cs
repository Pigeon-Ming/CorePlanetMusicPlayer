using CorePlanetMusicPlayer.Core.Albums;
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
    public sealed partial class AlbumsControl : UserControl
    {
        private bool _isBusy;

        private List<Album> _albumItems = new List<Album>();

        public AlbumsControl()
        {
            this.InitializeComponent();
        }

        public Task ReloadAsync()
        {
            return RunOperationAsync(ReloadAlbumAsync);
        }

        private async Task<string> ReloadAlbumAsync(AppServices services)
        {
            if (services.MusicLibraryService == null)
            {
                throw new InvalidOperationException("音乐库服务尚未就绪。");
            }

            var albums = await services.MusicLibraryService.GetAllAlbumsAsync();

            _albumItems = albums.ToList();

            AlbumsListView.ItemsSource = _albumItems;

            UpdateSelectionState();

            return _albumItems.Count == 0 ? "音乐库中暂无专辑。" : $"已读取 {_albumItems.Count} 张专辑。";
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            AlbumsCommandBar.IsEnabled = !isBusy;
            AlbumsListView.IsEnabled = !isBusy;

            UpdateSelectionState();
        }

        private static List<MusicId> GetAlbumMusicIds(Album album)
        {
            if (album == null)
            {
                throw new InvalidOperationException("请先选择专辑。");
            }

            if (album.MusicIds == null || album.MusicIds.Count == 0)
            {
                throw new InvalidOperationException("选中专辑没有歌曲。");
            }

            // 创建副本，保持专辑中现有的歌曲顺序。
            var musicIds = album.MusicIds.ToList();

            if (musicIds.Any(id => id.IsEmpty))
            {
                throw new InvalidOperationException("专辑包含无效歌曲 ID，请刷新音乐库后重试。");
            }

            return musicIds;
        }

        private async Task<string> PlaySelectedAsync(AppServices services, Album selectedAlbum)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = GetAlbumMusicIds(selectedAlbum);

            await services.PlaybackService.PlayQueueAsync(musicIds, musicIds[0]);

            var state = services.PlaybackService.State;

            if (state.HasError)
            {
                return $"播放失败：{state.ErrorMessage}";
            }

            return $"已请求播放专辑“{selectedAlbum.Title}”，共 {musicIds.Count} 首；当前状态：{state.Status}。";
        }

        private async Task<string> EnqueueNextAsync(AppServices services, Album selectedAlbum)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = GetAlbumMusicIds(selectedAlbum);

            int addedCount = await services.PlaybackService.EnqueueNextAsync(musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            return $"已按下一首规则插入专辑“{selectedAlbum.Title}”的 {addedCount} 首歌曲，队列共 {queueCount} 项。";
        }

        private async Task<string> EnqueueSelectedAsync(AppServices services, Album selectedAlbum)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = GetAlbumMusicIds(selectedAlbum);

            int addedCount = await services.PlaybackService.EnqueueAsync(musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            return $"已将专辑“{selectedAlbum.Title}”的 {addedCount} 首歌曲加入队列，队列共 {queueCount} 项。";
        }

        private void UpdateSelectionState()
        {
            var selectedAlbum = AlbumsListView.SelectedItem as Album;

            bool canOperate =!_isBusy && selectedAlbum != null && selectedAlbum.MusicCount > 0;

            PlayButton.IsEnabled = canOperate;
            PlayNextButton.IsEnabled = canOperate;
            AddToPlayQueueButton.IsEnabled = canOperate;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private void AlbumsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionState();
        }

        private async void ReloadAlbumsButton_Click(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedAlbum = AlbumsListView.SelectedItem as Album;

            if (selectedAlbum == null)
            {
                return;
            }

            await RunOperationAsync(services => PlaySelectedAsync(services, selectedAlbum));
        }

        private async void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedAlbum = AlbumsListView.SelectedItem as Album;

            if (selectedAlbum == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueNextAsync(services, selectedAlbum));
        }

        private async void AddToPlayQueueButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedAlbum = AlbumsListView.SelectedItem as Album;

            if (selectedAlbum == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueSelectedAsync(services, selectedAlbum));
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

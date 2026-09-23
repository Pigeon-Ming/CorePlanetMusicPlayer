using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Services.Library.Genres;
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
    public sealed partial class GenresControl : UserControl
    {

        private bool _isBusy;

        private List<GenreGroup> _genreItems = new List<GenreGroup>();

        public GenresControl()
        {
            this.InitializeComponent();
        }

        public Task ReloadAsync()
        {
            return RunOperationAsync(ReloadGenresAsync);
        }

        private async Task<string> ReloadGenresAsync(AppServices services)
        {
            if (services.GenreQueryService == null)
            {
                throw new InvalidOperationException("流派查询服务尚未就绪。");
            }

            var genres = await services.GenreQueryService.GetAllAsync();

            _genreItems = genres.ToList();

            GenresListView.SelectedItem = null;
            GenresListView.ItemsSource = _genreItems;

            UpdateSelectionState();

            return _genreItems.Count == 0 ? "音乐库中暂无流派分类。" : $"已读取 {_genreItems.Count} 个流派分类。";
        }

        private static async Task<List<MusicId>> GetGenreMusicIdsAsync(AppServices services, GenreGroup selectedGenre)
        {
            if (selectedGenre == null)
            {
                throw new InvalidOperationException("请先选择流派。");
            }

            if (services.GenreQueryService == null)
            {
                throw new InvalidOperationException("流派查询服务尚未就绪。");
            }

            // 保留原始分类值，null 正确表示未知流派。
            var musicItems = await services.GenreQueryService.GetMusicAsync(selectedGenre.Genre);

            var musicIds = musicItems.Select(music => music.Id).ToList();

            if (musicIds.Count == 0)
            {
                throw new InvalidOperationException("选中流派当前没有歌曲，请刷新列表。");
            }

            if (musicIds.Any(id => id.IsEmpty))
            {
                throw new InvalidOperationException("流派查询结果包含无效歌曲 ID，请刷新音乐库后重试。");
            }

            return musicIds;
        }

        private static string GetGenreDisplayName(GenreGroup genre)
        {
            return genre.Genre ?? "未知流派";
        }

        private async Task<string> PlaySelectedAsync(AppServices services, GenreGroup selectedGenre)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = await GetGenreMusicIdsAsync(services, selectedGenre);

            await services.PlaybackService.PlayQueueAsync(musicIds, musicIds[0]);

            var state = services.PlaybackService.State;

            if (state.HasError)
            {
                return $"播放失败：{state.ErrorMessage}";
            }

            string name = GetGenreDisplayName(selectedGenre);

            return $"已请求播放流派“{name}”，共 {musicIds.Count} 首；当前状态：{state.Status}。";
        }

        private async Task<string> EnqueueNextAsync(AppServices services, GenreGroup selectedGenre)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = await GetGenreMusicIdsAsync(services, selectedGenre);

            int addedCount = await services.PlaybackService.EnqueueNextAsync(musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            string name = GetGenreDisplayName(selectedGenre);

            return $"已按下一首规则插入流派“{name}”的 {addedCount} 首歌曲，队列共 {queueCount} 项。";
        }

        private async Task<string> EnqueueSelectedAsync(AppServices services, GenreGroup selectedGenre)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = await GetGenreMusicIdsAsync(services, selectedGenre);

            int addedCount = await services.PlaybackService.EnqueueAsync(musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            string name = GetGenreDisplayName(selectedGenre);

            return $"已将流派“{name}”的 {addedCount} 首歌曲加入队列，队列共 {queueCount} 项。";
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            GenresCommandBar.IsEnabled = !isBusy;
            GenresListView.IsEnabled = !isBusy;

            UpdateSelectionState();
        }

        private void UpdateSelectionState()
        {
            var selectedGenre = GenresListView.SelectedItem as GenreGroup;

            bool canOperate = !_isBusy && selectedGenre != null && selectedGenre.MusicCount > 0;

            PlayButton.IsEnabled = canOperate;
            PlayNextButton.IsEnabled = canOperate;
            AddToPlayQueueButton.IsEnabled = canOperate;
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

        private async void ReloadGenresButton_Click(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedGenre = GenresListView.SelectedItem as GenreGroup;

            if (selectedGenre == null)
            {
                return;
            }

            await RunOperationAsync(services => PlaySelectedAsync(services, selectedGenre));
        }

        private async void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedGenre = GenresListView.SelectedItem as GenreGroup;

            if (selectedGenre == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueNextAsync(services, selectedGenre));
        }

        private async void AddToPlayQueueButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedGenre = GenresListView.SelectedItem as GenreGroup;

            if (selectedGenre == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueSelectedAsync(services, selectedGenre));
        }

        private async void GenresListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionState();

            // 防止 XAML 初始化期间详情控件尚未创建。
            if (GenreDetailsControl == null)
            {
                return;
            }

            var selectedGenre = GenresListView.SelectedItem as GenreGroup;

            if (selectedGenre == null)
            {
                GenreDetailsControl.Clear();
                return;
            }

            await GenreDetailsControl.ShowGenreAsync(selectedGenre.Genre);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }
    }
}

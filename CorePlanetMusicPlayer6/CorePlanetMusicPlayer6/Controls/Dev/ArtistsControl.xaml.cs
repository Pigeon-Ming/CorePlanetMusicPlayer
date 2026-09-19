using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer6.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.Dev
{
    public sealed partial class ArtistsControl : UserControl
    {
        private bool _isBusy;

        private List<Artist> _artistItems = new List<Artist>();

        public ArtistsControl()
        {
            this.InitializeComponent();
        }

        public Task ReloadAsync()
        {
            return RunOperationAsync(ReloadArtistsAsync);
        }

        private async Task<string> ReloadArtistsAsync(AppServices services)
        {
            if (services.ArtistService == null)
            {
                throw new InvalidOperationException("艺术家服务尚未就绪。");
            }

            var artits = await services.ArtistService.GetAllAsync();

            _artistItems = artits.ToList();

            ArtistsListView.SelectedItem = null;
            ArtistDetailsControl.Clear();

            ArtistsListView.ItemsSource = _artistItems;

            UpdateSelectionState();

            return _artistItems.Count == 0 ? "音乐库中暂无艺术家。" : $"已读取 {_artistItems.Count} 个艺术家。";
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            ArtistsCommandBar.IsEnabled = !isBusy;
            ArtistsListView.IsEnabled = !isBusy;

            UpdateSelectionState();
        }

        private static List<MusicId> GetArtistMusicIds(Artist artist)
        {
            if (artist == null)
            {
                throw new InvalidOperationException("请先选择艺术家。");
            }

            if(artist.MusicIds == null || artist.MusicIds.Count == 0)
            {
                throw new InvalidOperationException("选中艺术家没有歌曲。");
            }

            var musicIds = artist.MusicIds.ToList();

            if (musicIds.Any(id => id.IsEmpty))
            {
                throw new InvalidOperationException("艺术家包含无效歌曲 ID，请刷新音乐库后重试。");
            }

            return musicIds;
        }

        private async Task<string> PlaySelectedAsync(AppServices services, Artist selectedArtist)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = GetArtistMusicIds(selectedArtist);

            await services.PlaybackService.PlayQueueAsync(musicIds, musicIds[0]);

            var state = services.PlaybackService.State;

            if (state.HasError)
            {
                return $"播放失败：{state.ErrorMessage}";
            }

            return $"已请求播放艺术家“{selectedArtist.Name}”，共 {musicIds.Count}首；当前状态：{state.Status}。";
        }

        private async Task<string> EnqueueNextAsync(AppServices services, Artist selectedArtist)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = GetArtistMusicIds(selectedArtist);

            int addedCount = await services.PlaybackService.EnqueueNextAsync(musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            return $"已按下一首规则插入艺术家“{selectedArtist.Name}”的 {addedCount} 首歌曲，队列共 {queueCount} 项。";
        }

        private async Task<string> EnqueueSelectedAsync(AppServices services, Artist selectedArtist)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = GetArtistMusicIds(selectedArtist);

            int addedCount = await services.PlaybackService.EnqueueAsync(musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            return $"已将艺术家“{selectedArtist.Name}”的 {addedCount} 首歌曲加入队列，队列共 {queueCount} 项。";
        }

        private void UpdateSelectionState()
        {
            var selectedArtist = ArtistsListView.SelectedItem as Artist;

            bool canOperate = !_isBusy && selectedArtist != null && selectedArtist.MusicCount > 0;

            PlayButton.IsEnabled = canOperate;
            PlayNextButton.IsEnabled = canOperate;
            AddToPlayQueueButton.IsEnabled = canOperate;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private async void ReloadArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedArtist = ArtistsListView.SelectedItem as Artist;

            if (selectedArtist == null)
            {
                return;
            }

            await RunOperationAsync(services => PlaySelectedAsync(services, selectedArtist));
        }

        private async void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedArtist = ArtistsListView.SelectedItem as Artist;

            if (selectedArtist == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueNextAsync(services, selectedArtist));
        }

        private async void AddToPlayQueueButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedArtist = ArtistsListView.SelectedItem as Artist;

            if (selectedArtist == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueSelectedAsync(services, selectedArtist));
        }

        private async void ArtistsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionState();

            if (ArtistDetailsControl == null)
            {
                return;
            }

            var selectedArtist = ArtistsListView.SelectedItem as Artist;

            if (selectedArtist == null)
            {
                ArtistDetailsControl.Clear();
                return;
            }

            await ArtistDetailsControl.ShowArtistAsync(selectedArtist.Id);
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

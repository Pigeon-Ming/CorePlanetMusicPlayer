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

        private ArtistGroupingMode? _artistGrouping;

        private bool _showEmptyArtists;

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

            await ReloadArtistGroupingAsync(services);

            var artists = await services.ArtistService.GetAllAsync();

            _artistItems = artists.ToList();

            return ApplyArtistFilter();
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            ArtistsCommandBar.IsEnabled = !isBusy;
            ArtistsListView.IsEnabled = !isBusy;
            ArtistDetailsControl.IsEnabled = !isBusy;

            // 同时控制已经展开的菜单项。
            SeparateArtistsMenuItem.IsEnabled = !isBusy;
            CombinedArtistsMenuItem.IsEnabled = !isBusy;

            ShowEmptyArtistsButton.IsEnabled = !isBusy;

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

            bool canPlay = !_isBusy && selectedArtist != null && selectedArtist.MusicCount > 0;

            bool canDelete = !_isBusy && selectedArtist != null && selectedArtist.MusicCount == 0;

            PlayButton.IsEnabled = canPlay;
            PlayNextButton.IsEnabled = canPlay;
            AddToPlayQueueButton.IsEnabled = canPlay;

            DeleteArtistButton.IsEnabled = canDelete;
        }

        private void UpdateArtistGroupingState()
        {
            SeparateArtistsMenuItem.IsChecked = _artistGrouping == ArtistGroupingMode.Separate;

            CombinedArtistsMenuItem.IsChecked = _artistGrouping == ArtistGroupingMode.Combined;
        }

        private string ApplyArtistFilter()
        {
            var visibleArtists = _showEmptyArtists
                ? _artistItems.ToList()
                : _artistItems
                    .Where(artist => artist.MusicCount > 0)
                    .ToList();

            ArtistsListView.SelectedItem = null;
            ArtistDetailsControl.Clear();

            ArtistsListView.ItemsSource = visibleArtists;

            UpdateSelectionState();

            if (_artistItems.Count == 0)
            {
                return "音乐库中暂无艺术家。";
            }

            int hiddenCount = _artistItems.Count - visibleArtists.Count;

            return hiddenCount > 0
                ? $"已显示 {visibleArtists.Count} 个艺术家，" +
                  $"隐藏 {hiddenCount} 个无歌曲艺术家。"
                : $"已显示 {visibleArtists.Count} 个艺术家。";
        }

        private async Task<string> DeleteSelectedArtistAsync(AppServices services, Artist selectedArtist)
        {
            if (services.ArtistService == null)
            {
                throw new InvalidOperationException("艺术家服务尚未就绪。");
            }

            if (selectedArtist == null)
            {
                throw new InvalidOperationException("请先选择艺术家。");
            }

            if (selectedArtist.MusicCount > 0)
            {
                throw new InvalidOperationException("只能删除没有歌曲的艺术家。");
            }

            await services.ArtistService.DeleteAsync(selectedArtist.Id);

            // 删除已经成功，立即移除界面中的旧记录。
            _artistItems.RemoveAll(artist => artist.Id == selectedArtist.Id);

            ApplyArtistFilter();

            string listMessage;

            try
            {
                listMessage = await ReloadArtistsAsync(services);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("艺术家已删除，但列表重新读取失败，请刷新列表。", exception);
            }

            return $"已删除艺术家“{selectedArtist.Name}”。{listMessage}";
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private async Task ReloadArtistGroupingAsync(AppServices services)
        {
            _artistGrouping = null;
            UpdateArtistGroupingState();

            if (services.SettingsService == null)
            {
                throw new InvalidOperationException("设置服务尚未就绪。");
            }

            var settings = await services.SettingsService.LoadAsync();

            _artistGrouping = settings.Library.ArtistGrouping;

            UpdateArtistGroupingState();
        }

        private async Task<string> ChangeArtistGroupingAsync(AppServices services, ArtistGroupingMode groupingMode)
        {
            if (services.MusicIndexService == null)
            {
                throw new InvalidOperationException("音乐索引服务尚未就绪。");
            }

            if (services.SettingsService == null ||
                services.ArtistService == null)
            {
                throw new InvalidOperationException("设置或艺术家服务尚未就绪。");
            }

            // 分类重建后旧列表可能失效，先清除旧列表和详情。
            _artistItems = new List<Artist>();
            ApplyArtistFilter();

            try
            {
                await services.MusicIndexService.SetArtistGroupingAsync(groupingMode);
            }
            catch
            {
                // 设置可能已经保存，因此重新读取实际值，
                // 不能直接恢复为切换前的勾选状态。
                try
                {
                    await ReloadArtistGroupingAsync(services);
                }
                catch (Exception reloadException)
                {
                    Debug.WriteLine(reloadException);

                    _artistGrouping = null;
                    UpdateArtistGroupingState();
                }

                // 保留原始切换异常，交给 RunOperationAsync 显示。
                throw;
            }

            string listMessage;

            try
            {
                listMessage = await ReloadArtistsAsync(services);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "分类规则已切换并完成索引重建，但列表读取失败。" +
                    "请点击刷新列表。",
                    exception);
            }

            string groupingName =
                groupingMode == ArtistGroupingMode.Separate
                    ? "拆分多位艺术家"
                    : "合并多位艺术家";

            return $"已切换为“{groupingName}”。{listMessage}";
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

        private async void SeparateArtistsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UpdateArtistGroupingState();

            await RunOperationAsync(services => ChangeArtistGroupingAsync(services, ArtistGroupingMode.Separate));
        }

        private async void CombinedArtistsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UpdateArtistGroupingState();

            await RunOperationAsync(services => ChangeArtistGroupingAsync(services, ArtistGroupingMode.Combined));
        }

        private void ShowEmptyArtistsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isBusy)
            {
                ShowEmptyArtistsButton.IsChecked = _showEmptyArtists;
                return;
            }

            _showEmptyArtists = ShowEmptyArtistsButton.IsChecked == true;

            StatusTextBlock.Text = ApplyArtistFilter();
        }

        private async void DeleteArtistButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedArtist = ArtistsListView.SelectedItem as Artist;

            if (selectedArtist == null)
            {
                return;
            }

            await RunOperationAsync(services => DeleteSelectedArtistAsync(services, selectedArtist));
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

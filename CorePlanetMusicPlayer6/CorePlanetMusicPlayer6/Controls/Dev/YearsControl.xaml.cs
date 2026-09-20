using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Services.Library.Years;
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
    public sealed partial class YearsControl : UserControl
    {
        private bool _isBusy;

        private List<YearGroup> _yearItems = new List<YearGroup>();

        public YearsControl()
        {
            this.InitializeComponent();
        }

        public Task ReloadAsync()
        {
            return RunOperationAsync(ReloadYearsAsync);
        }

        private async Task<string> ReloadYearsAsync(AppServices services)
        {
            if (services.YearQueryService == null)
            {
                throw new InvalidOperationException("年份查询服务尚未就绪。");
            }

            var years = await services.YearQueryService.GetAllAsync();

            _yearItems = years.ToList();

            YearsListView.SelectedItem = null;
            YearsListView.ItemsSource = _yearItems;

            UpdateSelectionState();

            return _yearItems.Count == 0 ? "音乐库中暂无年份分类。" : $"已读取 {_yearItems.Count} 个年份分类。";
        }

        private static async Task<List<MusicId>> GetYearMusicIdsAsync(AppServices services, YearGroup selectedYear)
        {
            if (selectedYear == null)
            {
                throw new InvalidOperationException("请先选择年份。");
            }

            if (services.YearQueryService == null)
            {
                throw new InvalidOperationException("年份查询服务尚未就绪。");
            }

            // 保留原始分类值，null 正确表示未知年份。
            var musicItems = await services.YearQueryService.GetMusicAsync(selectedYear.Year);

            var musicIds = musicItems.Select(music => music.Id).ToList();

            if (musicIds.Count == 0)
            {
                throw new InvalidOperationException("选中年份当前没有歌曲，请刷新列表。");
            }

            if (musicIds.Any(id => id.IsEmpty))
            {
                throw new InvalidOperationException("年份查询结果包含无效歌曲 ID，请刷新音乐库后重试。");
            }

            return musicIds;
        }

        private static string GetYearDisplayName(YearGroup year)
        {
            return year.Year.HasValue ? $"{year.Year.Value}年" : "未知年份";
        }

        private async Task<string> PlaySelectedAsync(AppServices services, YearGroup selectedYear)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = await GetYearMusicIdsAsync(services, selectedYear);

            await services.PlaybackService.PlayQueueAsync(musicIds, musicIds[0]);

            var state = services.PlaybackService.State;

            if (state.HasError)
            {
                return $"播放失败：{state.ErrorMessage}";
            }

            string name = GetYearDisplayName(selectedYear);

            return $"已请求播放年份“{name}”，共 {musicIds.Count} 首；当前状态：{state.Status}。";
        }

        private async Task<string> EnqueueNextAsync(AppServices services, YearGroup selectedYear)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = await GetYearMusicIdsAsync(services, selectedYear);

            int addedCount = await services.PlaybackService.EnqueueNextAsync(musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            string name = GetYearDisplayName(selectedYear);

            return $"已按下一首规则插入年份“{name}”的 {addedCount} 首歌曲，队列共 {queueCount} 项。";
        }

        private async Task<string> EnqueueSelectedAsync(AppServices services, YearGroup selectedYear)
        {
            if (services.PlaybackService == null)
            {
                throw new InvalidOperationException("播放服务尚未就绪。");
            }

            var musicIds = await GetYearMusicIdsAsync(services, selectedYear);

            int addedCount = await services.PlaybackService.EnqueueAsync(musicIds);

            int queueCount = services.PlaybackService.QueueSnapshot.Items.Count;

            string name = GetYearDisplayName(selectedYear);

            return $"已将年份“{name}”的 {addedCount} 首歌曲加入队列，队列共 {queueCount} 项。";
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;

            YearsCommandBar.IsEnabled = !isBusy;
            YearsListView.IsEnabled = !isBusy;

            UpdateSelectionState();
        }

        private void UpdateSelectionState()
        {
            var selectedYear = YearsListView.SelectedItem as YearGroup;

            bool canOperate = !_isBusy && selectedYear != null && selectedYear.MusicCount > 0;

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

        private async void ReloadYearsButton_Click(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedYear = YearsListView.SelectedItem as YearGroup;

            if (selectedYear == null)
            {
                return;
            }

            await RunOperationAsync(services => PlaySelectedAsync(services, selectedYear));
        }

        private async void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedYear = YearsListView.SelectedItem as YearGroup;

            if (selectedYear == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueNextAsync(services, selectedYear));
        }

        private async void AddToPlayQueueButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedYear = YearsListView.SelectedItem as YearGroup;

            if (selectedYear == null)
            {
                return;
            }

            await RunOperationAsync(services => EnqueueSelectedAsync(services, selectedYear));
        }

        private async void YearsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionState();

            if (YearDetailsControl == null)
            {
                return;
            }

            var selectedYear = YearsListView.SelectedItem as YearGroup;

            if (selectedYear == null)
            {
                YearDetailsControl.Clear();
                return;
            }

            await YearDetailsControl.ShowYearAsync(selectedYear.Year);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadAsync();
        }
    }
}

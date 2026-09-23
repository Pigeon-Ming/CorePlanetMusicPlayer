using CorePlanetMusicPlayer.Core.History;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer6.Composition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    public sealed partial class PlayHistoryControl : UserControl
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";

        private bool _isInitialized;
        private bool _isAttached;
        private bool _isBusy;

        private bool _isSavingHistorySetting;

        private int _loadVersion;

        private List<PlayHistoryEntryView> _historyItems =
            new List<PlayHistoryEntryView>();

        public PlayHistoryControl()
        {
            InitializeComponent();

            InitializeTimePickers();

            _isInitialized = true;
            UpdateControlState();
        }

        private void InitializeTimePickers()
        {
            var today = DateTimeOffset.Now;

            StartDatePicker.SelectedDate = today;
            EndDatePicker.SelectedDate = today;

            StartTimePicker.SelectedTime = new TimeSpan(0, 0, 0);
            EndTimePicker.SelectedTime = new TimeSpan(23, 59, 0);

            UseStartTimeCheckBox.IsChecked = false;
            UseEndTimeCheckBox.IsChecked = false;
        }

        private async void UserControl_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (_isAttached)
            {
                return;
            }

            _isAttached = true;
            UpdateControlState();

            await ReloadAsync();
        }

        private void UserControl_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            _isAttached = false;

            // 使尚未完成的查询结果失效。
            _loadVersion++;

            SetBusy(false);
        }

        public async Task ReloadAsync()
        {
            if (!_isAttached || _isBusy)
            {
                return;
            }

            var services = AppRuntime.Services;

            if (services?.PlaybackHistoryService == null ||
                services.MusicQueryService == null)
            {
                StatusTextBlock.Text =
                    "历史记录或歌曲查询服务尚未就绪。";
                return;
            }

            int version = ++_loadVersion;

            SetBusy(true);
            StatusTextBlock.Text = "正在读取播放历史……";

            try
            {
                // 在异步查询之前固定本次条件。
                DateTimeOffset? startTime = ReadBoundary(
                    UseStartTimeCheckBox.IsChecked == true,
                    StartDatePicker,
                    StartTimePicker,
                    "开始时间");

                DateTimeOffset? endTime = ReadBoundary(
                    UseEndTimeCheckBox.IsChecked == true,
                    EndDatePicker,
                    EndTimePicker,
                    "结束时间");

                if (startTime.HasValue &&
                    endTime.HasValue &&
                    endTime.Value < startTime.Value)
                {
                    throw new ArgumentException(
                        "结束时间不能早于开始时间。");
                }

                // 服务使用包含边界的查询。
                // 将用户选择的结束分钟扩展到这一分钟的末尾。
                DateTimeOffset? inclusiveEndTime =
                    endTime.HasValue
                        ? endTime.Value.AddTicks(
                            TimeSpan.TicksPerMinute - 1)
                        : (DateTimeOffset?)null;

                var histories = await services
                    .PlaybackHistoryService
                    .GetByDateRangeAsync(
                        startTime,
                        inclusiveEndTime);

                if (!_isAttached || version != _loadVersion)
                {
                    return;
                }

                // 只对歌曲资料查询去重。
                // 同一首歌对应的多条历史仍分别显示。
                var musicIds = histories
                    .Select(history => history.MusicId)
                    .Where(id => !id.IsEmpty)
                    .Distinct()
                    .ToList();

                var musicItems = await services
                    .MusicQueryService
                    .GetByIdsAsync(musicIds);

                if (!_isAttached || version != _loadVersion)
                {
                    return;
                }

                var musicById = musicItems
                    .GroupBy(music => music.Id)
                    .ToDictionary(
                        group => group.Key,
                        group => group.First());

                var rows = new List<PlayHistoryEntryView>();

                // 仓储已经按记录时间倒序排列。
                foreach (var history in histories)
                {
                    Music music;

                    musicById.TryGetValue(
                        history.MusicId,
                        out music);

                    rows.Add(new PlayHistoryEntryView
                    {
                        HistoryId = history.Id,
                        MusicId = history.MusicId,
                        PlayedAt = history.PlayedAt,

                        MusicDuration = history.MusicDuration,
                        PlayedDuration = history.PlayedDuration,
                        LastPosition = history.LastPosition,
                        IsCompleted = history.IsCompleted,

                        Title = GetDisplayText(
                            music == null
                                ? history.TitleSnapshot
                                : music.Title,
                            "未知歌曲"),

                        ArtistName = GetDisplayText(
                            music == null
                                ? history.ArtistNameSnapshot
                                : music.ArtistName,
                            "未知艺术家"),

                        AlbumTitle = GetDisplayText(
                            music == null
                                ? history.AlbumTitleSnapshot
                                : music.AlbumTitle,
                            "未知专辑"),

                        IsMissing = music == null
                    });
                }

                _historyItems = rows;
                HistoryListView.ItemsSource = _historyItems;

                int missingCount = rows.Count(
                    item => item.IsMissing);

                string scopeText = GetScopeText(
                    startTime,
                    endTime);

                SummaryTextBlock.Text =
                    $"{scopeText} · {rows.Count} 条记录" +
                    (missingCount > 0
                        ? $" · {missingCount} 条对应歌曲已不在当前音乐库中"
                        : string.Empty);

                StatusTextBlock.Text = rows.Count == 0
                    ? "没有符合条件的播放历史。"
                    : "播放历史读取完成。";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (_isAttached && version == _loadVersion)
                {
                    StatusTextBlock.Text =
                        $"查询失败：{ex.Message}";
                }
            }
            finally
            {
                if (_isAttached && version == _loadVersion)
                {
                    SetBusy(false);
                }
            }
        }

        private static DateTimeOffset? ReadBoundary(
            bool enabled,
            DatePicker datePicker,
            TimePicker timePicker,
            string fieldName)
        {
            if (!enabled)
            {
                return null;
            }

            if (!datePicker.SelectedDate.HasValue)
            {
                throw new ArgumentException(
                    $"请选择{fieldName}的日期。");
            }

            if (!timePicker.SelectedTime.HasValue)
            {
                throw new ArgumentException(
                    $"请选择{fieldName}的时、分。");
            }

            var date = datePicker.SelectedDate.Value;
            var time = timePicker.SelectedTime.Value;

            // DatePicker 只取年月日。
            // 重新按所选日期、时间计算本地时区偏移。
            var localTime = new DateTime(
                date.Year,
                date.Month,
                date.Day,
                time.Hours,
                time.Minutes,
                0,
                DateTimeKind.Unspecified);

            var timeZone = TimeZoneInfo.Local;

            if (timeZone.IsInvalidTime(localTime))
            {
                throw new ArgumentException(
                    $"{fieldName}处于夏令时跳过的时段，请调整时间。");
            }

            if (timeZone.IsAmbiguousTime(localTime))
            {
                throw new ArgumentException(
                    $"{fieldName}处于夏令时重复的时段，" +
                    "当前控件无法区分，请扩大查询区间。");
            }

            return new DateTimeOffset(
                localTime,
                timeZone.GetUtcOffset(localTime));
        }

        private static string GetScopeText(
            DateTimeOffset? startTime,
            DateTimeOffset? endTime)
        {
            if (startTime.HasValue && endTime.HasValue)
            {
                return
                    $"{FormatTime(startTime.Value)} 至 " +
                    $"{FormatTime(endTime.Value)}";
            }

            if (startTime.HasValue)
            {
                return $"{FormatTime(startTime.Value)} 起";
            }

            if (endTime.HasValue)
            {
                return $"截至 {FormatTime(endTime.Value)}";
            }

            return "全部记录";
        }

        private static string FormatTime(DateTimeOffset time)
        {
            return time.ToLocalTime().ToString(
                DateTimeFormat,
                CultureInfo.InvariantCulture);
        }

        private static string GetDisplayText(
            string value,
            string fallback)
        {
            return string.IsNullOrWhiteSpace(value)
                ? fallback
                : value;
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            UpdateControlState();
        }

        private void UpdateControlState()
        {
            // Checked/Unchecked 可能在 XAML 初始化时触发。
            if (!_isInitialized)
            {
                return;
            }

            bool canOperate = _isAttached && !_isBusy && !_isSavingHistorySetting;

            HistoryCommandBar.IsEnabled = canOperate;
            HistoryListView.IsEnabled = canOperate;

            UseStartTimeCheckBox.IsEnabled = canOperate;
            UseEndTimeCheckBox.IsEnabled = canOperate;

            bool canEditStart =
                canOperate &&
                UseStartTimeCheckBox.IsChecked == true;

            StartDatePicker.IsEnabled = canEditStart;
            StartTimePicker.IsEnabled = canEditStart;

            bool canEditEnd =
                canOperate &&
                UseEndTimeCheckBox.IsChecked == true;

            EndDatePicker.IsEnabled = canEditEnd;
            EndTimePicker.IsEnabled = canEditEnd;

            var playbackService = AppRuntime.Services?.PlaybackService;

            EnablePlaybackHistoryToggleButton.IsChecked =
                playbackService?.IsHistoryRecordingEnabled ?? false;

            EnablePlaybackHistoryToggleButton.IsEnabled =
                canOperate &&
                playbackService != null &&
                AppRuntime.Services?.SettingsService != null;
        }

        private void TimeFilterCheckBox_Changed(
            object sender,
            RoutedEventArgs e)
        {
            UpdateControlState();
        }

        private async void QueryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await ReloadAsync();
        }

        private async void QueryAllButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_isAttached || _isBusy)
            {
                return;
            }

            UseStartTimeCheckBox.IsChecked = false;
            UseEndTimeCheckBox.IsChecked = false;

            await ReloadAsync();
        }

        private async void EnablePlaybackHistoryToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var services = AppRuntime.Services;

            if (!_isAttached ||
                _isBusy ||
                _isSavingHistorySetting ||
                services?.PlaybackService == null ||
                services.SettingsService == null)
            {
                UpdateControlState();
                return;
            }

            // 必须在 UpdateControlState 前取得用户选择，
            // 因为该方法会根据实际运行状态同步按钮。
            bool enabled =
                EnablePlaybackHistoryToggleButton.IsChecked == true;

            _isSavingHistorySetting = true;
            UpdateControlState();

            try
            {
                // 读取最新设置，避免使用启动时的旧对象覆盖其他设置。
                var settings = await services.SettingsService.LoadAsync();

                settings.Playback.EnablePlaybackHistory = enabled;

                // 保存成功后再应用到当前播放服务。
                await services.SettingsService.SaveAsync(settings);

                services.PlaybackService.SetHistoryRecordingEnabled(enabled);

                if (_isAttached)
                {
                    StatusTextBlock.Text = enabled
                        ? "已开启播放历史记录，关闭期间的播放不会补记。"
                        : "已关闭播放历史记录，已有记录保留。";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (_isAttached)
                {
                    StatusTextBlock.Text =
                        $"修改播放历史设置失败：{ex.Message}";
                }
            }
            finally
            {
                _isSavingHistorySetting = false;

                // 根据实际运行状态显示勾选结果。
                UpdateControlState();
            }
        }
    }

    public sealed class PlayHistoryEntryView
    {
        public PlaybackHistoryId HistoryId { get; set; }

        public MusicId MusicId { get; set; }

        public DateTimeOffset PlayedAt { get; set; }

        public TimeSpan MusicDuration { get; set; }

        public TimeSpan PlayedDuration { get; set; }

        public TimeSpan LastPosition { get; set; }

        public bool IsCompleted { get; set; }

        public string Title { get; set; }

        public string ArtistName { get; set; }

        public string AlbumTitle { get; set; }

        public bool IsMissing { get; set; }

        public string ArtistText
        {
            get { return $"艺术家：{ArtistName}"; }
        }

        public string AlbumText
        {
            get { return $"专辑：{AlbumTitle}"; }
        }

        public string PlayedAtText
        {
            get
            {
                return "记录时间：" +
                    PlayedAt.ToLocalTime().ToString(
                        "yyyy-MM-dd HH:mm",
                        CultureInfo.InvariantCulture);
            }
        }

        public string AvailabilityText
        {
            get
            {
                return IsMissing
                    ? "对应歌曲已不在当前音乐库中，以上为播放时保存的资料。"
                    : string.Empty;
            }
        }

        public string HistoryIdText
        {
            get { return $"历史 ID：{HistoryId}"; }
        }

        public string DurationText
        {
            get
            {
                return $"有效播放：{PlayedDuration.TotalSeconds:F1} 秒" +
                    $" · 总时长：{MusicDuration.TotalSeconds:F1} 秒" +
                    $" · 最后位置：{LastPosition.TotalSeconds:F1} 秒";
            }
        }

        public string CompletionText
        {
            get
            {
                return IsCompleted
                    ? "已达到完成阈值"
                    : "尚未达到完成阈值";
            }
        }
    }
}

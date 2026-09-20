using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Core.Playlists;
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
    public sealed class PlaylistEntryView
    {
        public PlaylistItem Item { get; set; }

        public Music Music { get; set; }

        public int Number { get; set; }

        public bool IsMissing
        {
            get { return Music == null; }
        }

        public string Title
        {
            get
            {
                return Music == null ? "歌曲已不在音乐库中" : Music.Title;
            }
        }

        public string ArtistName
        {
            get { return Music?.ArtistName ?? string.Empty; }
        }
    }

    public sealed partial class PlaylistControl : UserControl
    {
        private Playlist _currentPlaylist;

        private List<PlaylistEntryView> _rows = new List<PlaylistEntryView>();

        private bool _isLoading;
        private bool _isSaving;

        private int _loadVersion;

        public bool IsSaving
        {
            get { return _isSaving; }
        }

        // 保存成功并重新读取后，通知父控件更新列表中的信息。
        public event Action<Playlist> PlaylistChanged;

        // 写入期间，通知父控件暂时禁止切换和刷新。
        public event EventHandler SavingStateChanged;

        private sealed class PlaylistViewData
        {
            public Playlist Playlist { get; set; }

            public List<Music> MusicItems { get; set; }

            public List<PlaylistEntryView> Rows { get; set; }
        }

        public PlaylistControl()
        {
            this.InitializeComponent();

            Clear();
        }

        private static async Task<PlaylistViewData> ReadDetailsAsync(AppServices services, PlaylistId playlistId)
        {
            if (services?.PlaylistService == null || services.MusicQueryService == null)
            {
                throw new InvalidOperationException("播放列表或音乐查询服务尚未就绪。");
            }

            var playlist = await services.PlaylistService.GetByIdAsync(playlistId);

            if (playlist == null)
            {
                throw new InvalidOperationException("播放列表已不存在，请刷新播放列表列表。");
            }

            // 同时用于解析已有条目和填充歌曲选择框。
            var allMusic = await services.MusicQueryService.GetAllAsync();

            var musicItems = allMusic
                .Where(music => music != null && !music.Id.IsEmpty)
                .OrderBy(
                    music => music.Title,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    music => music.Id.ToString(),
                    StringComparer.Ordinal)
                .ToList();

            var musicById = musicItems.ToDictionary(music => music.Id);

            var items = playlist.Items ?? new List<PlaylistItem>();

            if (items.Any(item => item == null || string.IsNullOrWhiteSpace(item.Id)))
            {
                throw new InvalidOperationException("播放列表包含无效条目，无法读取详情。");
            }

            var rows = new List<PlaylistEntryView>();

            foreach (var item in items.OrderBy(item => item.Order))
            {
                Music music;
                musicById.TryGetValue(item.MusicId, out music);

                rows.Add(new PlaylistEntryView
                {
                    Item = item,
                    Music = music,
                    Number = rows.Count + 1
                });
            }

            return new PlaylistViewData
            {
                Playlist = playlist,
                MusicItems = musicItems,
                Rows = rows
            };
        }

        public async Task ShowPlaylistAsync(PlaylistId playlistId)
        {
            Clear();

            int version = _loadVersion;

            _isLoading = true;
            UpdateOperationState();

            StatusTextBlock.Text = "正在读取播放列表……";

            try
            {
                var data = await ReadDetailsAsync(AppRuntime.Services, playlistId);

                if (version != _loadVersion)
                {
                    return;
                }

                ApplyDetails(data, true);

                StatusTextBlock.Text = data.Rows.Count == 0 ? "播放列表为空，可以选择歌曲后添加。" : $"已读取 {data.Rows.Count} 个条目。";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (version == _loadVersion)
                {
                    ClearContent();
                    StatusTextBlock.Text = $"读取失败：{ex.Message}";
                }
            }
            finally
            {
                if (version == _loadVersion)
                {
                    _isLoading = false;
                    UpdateOperationState();
                }
            }
        }

        private void ApplyDetails(PlaylistViewData data, bool updateInfo)
        {
            _currentPlaylist = data.Playlist;
            _rows = data.Rows;

            if (updateInfo)
            {
                PlaylistNameTextBox.Text = data.Playlist.Name ?? string.Empty;

                PlaylistDescriptionTextBox.Text = data.Playlist.Description ?? string.Empty;
            }

            var totalDuration = TimeSpan.Zero;

            foreach (var row in _rows)
            {
                if (!row.IsMissing)
                {
                    totalDuration += row.Music.Duration;
                }
            }

            int missingCount = _rows.Count(row => row.IsMissing);

            PlaylistSummaryTextBlock.Text =
                $"条目：{_rows.Count} 个\n" +
                $"失效条目：{missingCount} 个\n" +
                $"已有歌曲总时长：{totalDuration:c}";

            PlaylistItemsListView.SelectedItem = null;
            PlaylistItemsListView.ItemsSource = _rows;

            MusicToAddComboBox.SelectedItem = null;
            MusicToAddComboBox.ItemsSource = data.MusicItems;
        }

        public void Clear()
        {
            _loadVersion++;
            _isLoading = false;

            ClearContent();

            StatusTextBlock.Text = "请选择播放列表。";

            UpdateOperationState();
        }

        private void ClearContent()
        {
            _currentPlaylist = null;
            _rows = new List<PlaylistEntryView>();

            PlaylistNameTextBox.Text = string.Empty;
            PlaylistDescriptionTextBox.Text = string.Empty;
            PlaylistSummaryTextBlock.Text = string.Empty;

            PlaylistItemsListView.SelectedItem = null;
            PlaylistItemsListView.ItemsSource = null;

            MusicToAddComboBox.SelectedItem = null;
            MusicToAddComboBox.ItemsSource = null;
        }

        private async Task RunEditAsync(Func<AppServices, PlaylistId, Task> operation, string successMessage, bool updateInfo = false)
        {
            if (_isLoading || _isSaving || _currentPlaylist == null)
            {
                return;
            }

            var services = AppRuntime.Services;

            if (services?.PlaylistService == null || services.MusicQueryService == null)
            {
                StatusTextBlock.Text = "服务尚未就绪。";
                return;
            }

            var playlistId = _currentPlaylist.Id;
            int version = _loadVersion;

            string selectedItemId = (PlaylistItemsListView.SelectedItem as PlaylistEntryView) ?.Item.Id;

            bool saved = false;

            _isSaving = true;
            UpdateOperationState();
            SavingStateChanged?.Invoke(this, EventArgs.Empty);

            StatusTextBlock.Text = "正在执行……";

            try
            {
                await operation(services, playlistId);
                saved = true;

                var data = await ReadDetailsAsync(services, playlistId);

                if (version == _loadVersion)
                {
                    ApplyDetails(data, updateInfo);

                    // 移动后继续选中同一个条目。
                    PlaylistItemsListView.SelectedItem = _rows.FirstOrDefault(row => row.Item.Id == selectedItemId);

                    StatusTextBlock.Text = successMessage;
                }

                PlaylistChanged?.Invoke(data.Playlist);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (version == _loadVersion)
                {
                    StatusTextBlock.Text = saved ? $"更改已保存，但界面更新失败，请刷新列表。原因：{ex.Message}" : $"操作失败：{ex.Message}";
                }
            }
            finally
            {
                _isSaving = false;

                UpdateOperationState();
                SavingStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void SaveInfoButton_Click(object sender, RoutedEventArgs e)
        {
            string name = PlaylistNameTextBox.Text;
            string description = PlaylistDescriptionTextBox.Text;

            await RunEditAsync(
                (services, playlistId) =>
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        throw new InvalidOperationException("播放列表名称不能为空。");
                    }

                    return services.PlaylistService.UpdateInfoAsync(playlistId, name.Trim(), description);
                },
                "播放列表资料已保存。",
                updateInfo: true);
        }

        private async void AddMusicButton_Click(object sender, RoutedEventArgs e)
        {
            var music = MusicToAddComboBox.SelectedItem as Music;

            if (music == null)
            {
                return;
            }

            var musicId = music.Id;

            await RunEditAsync(
                async (services, playlistId) =>
                {
                    await services.PlaylistService.AddMusicAsync(playlistId, musicId);
                },
                $"已添加歌曲“{music.Title}”。");
        }

        private async void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            var row = PlaylistItemsListView.SelectedItem as PlaylistEntryView;

            if (row == null)
            {
                return;
            }

            string itemId = row.Item.Id;

            await RunEditAsync(
                (services, playlistId) =>
                    services.PlaylistService.RemoveItemAsync(playlistId, itemId), "已移除选中条目。");
        }

        private async Task MoveSelectedAsync(int offset)
        {
            var row = PlaylistItemsListView.SelectedItem as PlaylistEntryView;

            if (row == null)
            {
                return;
            }

            int currentIndex = _rows.IndexOf(row);
            int newIndex = currentIndex + offset;

            if (currentIndex < 0 || newIndex < 0 || newIndex >= _rows.Count)
            {
                return;
            }

            string itemId = row.Item.Id;

            await RunEditAsync(
                (services, playlistId) =>
                    services.PlaylistService.MoveItemAsync(playlistId, itemId,newIndex), "条目顺序已更新。");
        }

        private async void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            await MoveSelectedAsync(-1);
        }

        private async void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            await MoveSelectedAsync(1);
        }

        private void UpdateOperationState()
        {
            // XAML 初始化期间，选择事件可能提前触发。
            if (StatusTextBlock == null)
            {
                return;
            }

            bool canEdit = _currentPlaylist != null && !_isLoading && !_isSaving;

            PlaylistNameTextBox.IsEnabled = canEdit;
            PlaylistDescriptionTextBox.IsEnabled = canEdit;
            SaveInfoButton.IsEnabled = canEdit;

            MusicToAddComboBox.IsEnabled = canEdit;
            AddMusicButton.IsEnabled = canEdit && MusicToAddComboBox.SelectedItem is Music;

            PlaylistItemsListView.IsEnabled = canEdit;

            var row = PlaylistItemsListView.SelectedItem as PlaylistEntryView;

            int index = row == null ? -1 : _rows.IndexOf(row);

            RemoveItemButton.IsEnabled = canEdit && index >= 0;
            MoveUpButton.IsEnabled = canEdit && index > 0;
            MoveDownButton.IsEnabled = canEdit && index >= 0 && index < _rows.Count - 1;
        }

        private void MusicToAddComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOperationState();
        }

        private void PlaylistItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOperationState();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Clear();
        }
    }
}

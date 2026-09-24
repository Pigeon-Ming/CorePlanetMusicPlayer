using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Services.Library.Albums;
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
    public sealed partial class AlbumControl : UserControl
    {
        private Album _currentAlbum;

        private bool _isLoading;
        private bool _isSaving;

        // 每次切换或清空详情时递增。
        private int _loadVersion;

        public AlbumControl()
        {
            this.InitializeComponent();

            Clear();
        }

        public void Clear()
        {
            // 已经开始的旧查询即使返回，也不能再更新当前界面。
            _loadVersion++;

            _isLoading = false;

            ClearContent();

            StatusTextBlock.Text = "请选择专辑。";

            UpdateOperationState();
        }

        public async Task ShowAlbumAsync(AlbumId albumId)
        {
            Clear();

            int version = _loadVersion;

            _isLoading = true;
            UpdateOperationState();

            StatusTextBlock.Text = "正在读取专辑……";

            try
            {
                if (albumId.IsEmpty)
                {
                    throw new ArgumentException(
                        "专辑 ID 不能为空。",
                        nameof(albumId));
                }

                var albumService = AppRuntime.Services?.AlbumService;

                if (albumService == null)
                {
                    throw new InvalidOperationException(
                        "专辑服务尚未就绪。");
                }

                var details = await albumService.GetDetailsAsync(albumId);

                if (version != _loadVersion)
                {
                    return;
                }

                if (details == null)
                {
                    StatusTextBlock.Text =
                        "该专辑已不存在，请刷新专辑列表。";
                    return;
                }

                ApplyDetails(details);

                if (details.MissingMusicIds.Count > 0)
                {
                    StatusTextBlock.Text =
                        $"已读取 {details.MusicItems.Count} 首歌曲，" +
                        $"另有 {details.MissingMusicIds.Count} 个歌曲关联已失效，" +
                        "可尝试刷新音乐库。";
                }
                else
                {
                    StatusTextBlock.Text = details.MusicItems.Count == 0
                        ? "该专辑暂无歌曲。"
                        : $"已读取 {details.MusicItems.Count} 首歌曲。";
                }

                await LoadAlbumArtworkAsync(details, version);
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

        private void ApplyDetails(AlbumDetails details)
        {
            var album = details.Album;

            _currentAlbum = album;

            AlbumTitleTextBlock.Text =
                string.IsNullOrWhiteSpace(album.Title)
                    ? "未知专辑"
                    : album.Title;

            string artistName =
                string.IsNullOrWhiteSpace(album.AlbumArtistName)
                    ? album.ArtistName
                    : album.AlbumArtistName;

            AlbumArtistNameTextBlock.Text =
                string.IsNullOrWhiteSpace(artistName)
                    ? "未知艺术家"
                    : artistName;

            string year = album.Year.HasValue && album.Year.Value > 0
                ? album.Year.Value.ToString()
                : "未知";

            string genre = string.IsNullOrWhiteSpace(album.Genre)
                ? "未知"
                : album.Genre;

            string discCount = album.DiscCount > 0
                ? $"{album.DiscCount} 张"
                : "未知";

            AlbumTextBlock.Text =
                $"歌曲：{album.MusicCount} 首\n" +
                $"碟片：{discCount}\n" +
                $"年份：{year}\n" +
                $"流派：{genre}\n" +
                $"总时长：{album.TotalDuration:c}";

            AlbumDescriptionTextBox.Text =
                album.Description ?? string.Empty;

            // 使用服务返回的顺序，仅将歌曲组织成显示分组。
            AlbumMusicSource.Source = details.MusicItems
                .GroupBy(music =>
                    GetDiscTitle(music.Metadata?.DiscNumber))
                .ToList();
        }

        private static string GetDiscTitle(int? discNumber)
        {
            return discNumber.HasValue && discNumber.Value > 0
                ? $"Disc {discNumber.Value}"
                : "碟号未知";
        }

        private void ClearContent()
        {
            _currentAlbum = null;

            AlbumTitleTextBlock.Text = string.Empty;
            AlbumArtistNameTextBlock.Text = string.Empty;
            AlbumTextBlock.Text = string.Empty;
            AlbumDescriptionTextBox.Text = string.Empty;

            AlbumMusicSource.Source = null;

            AlbumArtworkControl.Request = null;
        }

        private void UpdateOperationState()
        {
            bool canEdit =
                _currentAlbum != null &&
                !_isLoading &&
                !_isSaving;

            AlbumDescriptionTextBox.IsEnabled = canEdit;
            SaveAlbumButton.IsEnabled = canEdit;
        }

        private async Task<string> SaveDescriptionAsync(
            AppServices services)
        {
            if (services.AlbumService == null)
            {
                throw new InvalidOperationException(
                    "专辑服务尚未就绪。");
            }

            // 在等待之前确定保存对象和文本。
            var album = _currentAlbum;

            if (album == null)
            {
                throw new InvalidOperationException("请先选择专辑。");
            }

            string description = AlbumDescriptionTextBox.Text;

            await services.AlbumService.UpdateDescriptionAsync(
                album.Id,
                description);

            album.Description = description ?? string.Empty;

            return "专辑简介已保存。";
        }

        private async Task LoadAlbumArtworkAsync(
            AlbumDetails details,
            int version)
        {
            try
            {
                var service = AppRuntime.Services?.AlbumArtworkService;

                if (service == null)
                {
                    throw new InvalidOperationException(
                        "专辑封面服务尚未就绪。");
                }

                // 复用已读取的、按专辑顺序排列的歌曲。
                var request = await service.CreateArtworkAsync(
                    details.MusicItems);

                if (version != _loadVersion)
                {
                    return;
                }

                AlbumArtworkControl.Request = request;
            }
            catch (Exception exception)
            {
                // 封面失败不影响专辑文字信息和歌曲列表。
                Debug.WriteLine("读取专辑封面来源失败：" + exception);
            }
        }

        private async Task RunOperationAsync(
            Func<AppServices, Task<string>> operation)
        {
            if (_isLoading || _isSaving || _currentAlbum == null)
            {
                return;
            }

            var services = AppRuntime.Services;

            if (services == null)
            {
                StatusTextBlock.Text = "服务尚未就绪，请检查应用初始化。";
                return;
            }

            int version = _loadVersion;

            _isSaving = true;
            UpdateOperationState();

            StatusTextBlock.Text = "正在保存……";

            try
            {
                string message = await operation(services);

                if (version == _loadVersion)
                {
                    StatusTextBlock.Text = message;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (version == _loadVersion)
                {
                    StatusTextBlock.Text = $"保存失败：{ex.Message}";
                }
            }
            finally
            {
                _isSaving = false;
                UpdateOperationState();
            }
        }

        private async void SaveAlbumButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            await RunOperationAsync(SaveDescriptionAsync);
        }

        private void UserControl_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            Clear();
        }
    }
}

using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Services.Library.Artists;
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
    public sealed partial class ArtistControl : UserControl
    {
        private Artist _currentArtist;

        private bool _isLoading;
        private bool _isSaving;

        private int _loadVersion;

        public ArtistControl()
        {
            this.InitializeComponent();

            Clear();
        }

        public void Clear()
        {
            _loadVersion++;

            _isLoading = false;

            ClearContent();

            StatusTextBlock.Text = "请选择艺术家。";

            UpdateOperationState();
        }

        public async Task ShowArtistAsync(ArtistId artistId)
        {
            Clear();

            int version = _loadVersion;

            _isLoading = true;
            UpdateOperationState();

            StatusTextBlock.Text = "正在读取艺术家……";

            try
            {
                if (artistId.IsEmpty)
                {
                    throw new ArgumentException(
                        "艺术家 ID 不能为空。",
                        nameof(artistId));
                }

                var artistService = AppRuntime.Services?.ArtistService;

                if (artistService == null)
                {
                    throw new InvalidOperationException(
                        "艺术家服务尚未就绪。");
                }

                var details = await artistService.GetDetailsAsync(artistId);

                if (version != _loadVersion)
                {
                    return;
                }

                if (details == null)
                {
                    StatusTextBlock.Text =
                        "该艺术家已不存在，请刷新艺术家列表。";
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
                        ? "该艺术家暂无歌曲。"
                        : $"已读取 {details.MusicItems.Count} 首歌曲。";
                }
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

        private void ApplyDetails(ArtistDetails details)
        {
            var artist = details.Artist;

            _currentArtist = artist;

            ArtistTitleTextBlock.Text =
                string.IsNullOrWhiteSpace(artist.Name)
                    ? "未知艺术家"
                    : artist.Name;

            ArtistTextBlock.Text =
                $"歌曲：{artist.MusicCount} 首\n" +
                $"专辑数：{artist.AlbumCount}\n" +
                $"总时长：{artist.TotalDuration:c}";

            ArtistDescriptionTextBox.Text =
                artist.Description ?? string.Empty;

            ArtistMusicListView.ItemsSource = details.MusicItems;
            ArtistAlbumsListView.ItemsSource = details.Albums;
        }

        private void ClearContent()
        {
            _currentArtist = null;

            ArtistTitleTextBlock.Text = string.Empty;
            ArtistTextBlock.Text = string.Empty;
            ArtistDescriptionTextBox.Text = string.Empty;

            ArtistMusicListView.ItemsSource = null;
            ArtistAlbumsListView.ItemsSource = null;
        }

        private void UpdateOperationState()
        {
            bool canEdit =
                _currentArtist != null &&
                !_isLoading &&
                !_isSaving;

            ArtistDescriptionTextBox.IsEnabled = canEdit;
            SaveArtistButton.IsEnabled = canEdit;
        }

        private async Task<string> SaveDescriptionAsync(
            AppServices services)
        {
            if (services.ArtistService == null)
            {
                throw new InvalidOperationException(
                    "艺术家服务尚未就绪。");
            }

            // 在等待之前确定保存对象和文本。
            var artist = _currentArtist;

            if (artist == null)
            {
                throw new InvalidOperationException("请先选择艺术家。");
            }

            string description = ArtistDescriptionTextBox.Text;

            await services.ArtistService.UpdateDescriptionAsync(
                artist.Id,
                description);

            artist.Description = description ?? string.Empty;

            return "艺术家简介已保存。";
        }

        private async Task RunOperationAsync(
            Func<AppServices, Task<string>> operation)
        {
            if (_isLoading || _isSaving || _currentArtist == null)
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

        private async void SaveArtistButton_Click(
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

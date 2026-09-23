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
    public sealed partial class GenreControl : UserControl
    {
        private int _loadVersion;

        public GenreControl()
        {
            this.InitializeComponent();

            Clear();
        }

        public void Clear()
        {
            // 使已经开始的旧查询失效。
            _loadVersion++;

            ClearContent();

            StatusTextBlock.Text = "请选择流派。";
        }

        public async Task ShowGenreAsync(string genre)
        {
            Clear();

            int version = _loadVersion;

            StatusTextBlock.Text = "正在读取流派歌曲……";

            try
            {
                var queryService =
                    AppRuntime.Services?.GenreQueryService;

                if (queryService == null)
                {
                    throw new InvalidOperationException(
                        "流派查询服务尚未就绪。");
                }

                // null 是有效分类值，表示未知流派。
                var musicItems = await queryService.GetMusicAsync(genre);

                if (version != _loadVersion)
                {
                    return;
                }

                string title = string.IsNullOrWhiteSpace(genre)
                    ? "未知流派"
                    : genre.Trim();

                ApplyDetails(title, musicItems);

                StatusTextBlock.Text = musicItems.Count == 0
                    ? "该流派当前没有歌曲，可刷新流派列表。"
                    : $"已读取 {musicItems.Count} 首歌曲。";
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
        }

        private void ApplyDetails(string title, IReadOnlyList<Music> musicItems)
        {
            var totalDuration = TimeSpan.Zero;

            foreach (var music in musicItems)
            {
                totalDuration += music.Duration;
            }

            GenreTitleTextBlock.Text = title;

            GenreTextBlock.Text =
                $"歌曲：{musicItems.Count} 首\n" +
                $"总时长：{totalDuration:c}";

            // 保留查询服务提供的歌曲顺序。
            GenreMusicListView.ItemsSource = musicItems;
        }

        private void ClearContent()
        {
            GenreTitleTextBlock.Text = string.Empty;
            GenreTextBlock.Text = string.Empty;

            GenreMusicListView.SelectedItem = null;
            GenreMusicListView.ItemsSource = null;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Clear();
        }
    }
}

using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.DevControls
{
    public sealed partial class PlaylistControl : UserControl
    {
        private Playlist playlist;
        public PlaylistControl()
        {
            this.InitializeComponent();
        }

        public void SetPlaylist(Playlist playlist)
        {
            if(playlist != null)
                playlist.Music.CollectionChanged -= Music_CollectionChanged;
            this.playlist = playlist;

            TitleTextBlock.Text = "标题：" + playlist.Title;
            DescriptionTextBox.Text = playlist.Description;
            CountTextBlock.Text = "歌曲数：" + playlist.Music.Count;
            
            SetListView();
            SetCoverImage();
            playlist.Music.CollectionChanged += Music_CollectionChanged;
        }

        private async void Music_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetListView();
            });
        }

        void SetListView()
        {
            MusicListView.ItemsSource = null;
            MusicListView.ItemsSource = playlist.Music;
        }

        private async void Menu_RemoveSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            if(MusicListView.SelectedItem != null)
                await RemoveItemAsync((IMusic)MusicListView.SelectedItem);
        }

        private async Task RemoveItemAsync(IMusic item)
        {
            await PlaylistManager.RemoveMusicFromPlaylistAsync(playlist, item);
        }

        private void Menu_SavePlaylistInfo_Click(object sender, RoutedEventArgs e)
        {
            _ = SavePlaylistInfoAsync();
        }

        async Task SavePlaylistInfoAsync()
        {
            playlist.Description = DescriptionTextBox.Text;
            await PlaylistManager.SavePlaylistAsync(playlist);
        }

        private void Cover_SetStreamCoverButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Cover_SetStreamCoverAsync();
        }

        async Task Cover_SetStreamCoverAsync()
        {
            await playlist.SetSteamCoverAsync(Cover_StreamCoverUrlTextBlock.Text);
            SetCoverImage();
        }

        private void Cover_SetLocalCoverButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Cover_SetLocalCover();
        }

        async Task Cover_SetLocalCover()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null)
                return;
            await playlist.SetLocalCoverAsync(file);
            SetCoverImage();
        }

        private void Cover_ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            _ = playlist.SetToDefaultCoverAsync();
            SetCoverImage();
        }

        void SetCoverImage()
        {
            switch (playlist.CoverType)
            {
                case PlaylistCoverType.Stream:
                    CoverImage.Source = new BitmapImage(new Uri(playlist.CoverUrl));
                    break;
                case PlaylistCoverType.Local:
                    CoverImage.Source = new BitmapImage(new Uri(playlist.CoverPath));
                    break;
                default:
                    // 读取Playlist最后一首歌曲的Cover
                    break;
            }
        }
    }
}

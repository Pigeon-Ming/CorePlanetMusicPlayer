using CorePlanetMusicPlayer.App;
using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.DevControls
{
    public sealed partial class ArtistControl : UserControl
    {
        private Artist artist;

        public ArtistControl()
        {
            this.InitializeComponent();
        }

        public void SetArtist(Artist artist)
        {
            this.artist = artist;
            UpdateView();
        }

        void UpdateView()
        {
            List<Album> albums = artist.GetAlbums();

            NameTextBlock.Text = $"名称：{artist.Name}";
            DescriptionTextBox.Text = artist.Description;

            ProfilePathTextBox.Text = artist.ProfilePath;

            AlbumsCountTextBlock.Text = $"专辑数：{albums.Count.ToString()}";
            MusicCountTextBlock.Text = $"音乐数：{artist.Music.Count.ToString()}";

            MusicListView.ItemsSource = null;
            MusicListView.ItemsSource = artist.Music;

            AlbumsListView.ItemsSource = null;
            AlbumsListView.ItemsSource = albums;
        }

        private async void SaveDescriptionButton_Click(object sender, RoutedEventArgs e)
        {
            artist.Description = DescriptionTextBox.Text;
            await DataBaseManager.UpdateArtistsDataAsync(new List<Artist>() { artist });
        }

        private async void SetArtistProfileURLButton_Click(object sender, RoutedEventArgs e)
        {
            artist.ProfilePath = ProfilePathTextBox.Text;
            await DataBaseManager.UpdateArtistsDataAsync(new List<Artist>() { artist });
        }

        private async void OpenArtistProfileFileButton_Click(object sender, RoutedEventArgs e)
        {
            await OpenProfileFileAsync(artist);
        }

        async Task OpenProfileFileAsync(Artist artist)
        {
            StorageFolder folder = await StorageHelper.GetStorageFolderFromStorageFolderAsync(await StorageHelper.GetApplicationDataFolderAsync("Data"), "ArtistsProfile");
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            artist.ProfilePath = (await file.CopyAsync(folder, artist.Name + file.FileType)).Path;
            ProfilePathTextBox.Text = artist.ProfilePath;
            await DataBaseManager.UpdateArtistsDataAsync(new List<Artist>() { artist });
        }
    }
}

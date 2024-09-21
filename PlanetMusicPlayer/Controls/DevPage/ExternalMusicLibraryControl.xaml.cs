using CorePlanetMusicPlayer.Models;
using PlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace PlanetMusicPlayer.Controls.DevPage
{
    public sealed partial class ExternalMusicLibraryControl : UserControl
    {
        public ExternalMusicLibraryControl()
        {
            this.InitializeComponent();
            Loaded += ExternalMusicLibraryControl_Loaded;
        }

        private void ExternalMusicLibraryControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        void RefreshData()
        {
            musicListControl.MainListView.ItemsSource = null;
            musicListControl.MainListView.ItemsSource = Library.Music.ExternalMusic;
            musicListControl.MainListView.ItemTemplate = (DataTemplate)Resources["NormalMusicListItem"];
            Debug.WriteLine(Library.Music.LocalMusic.Count);
        }

        private void RefreshListDataButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void PlaySelectedItem_Click(object sender, RoutedEventArgs e)
        {
            if (musicListControl.MainListView.SelectedItem == null)
                return;
            PlayCore.PlayMusic(musicListControl.MainListView.SelectedItem as Music, Library.Music.ExternalMusic.ToList<Music>(), musicListControl.MainListView.SelectedIndex);
        }

        private void AddToPlayQueue_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".flac");
            picker.FileTypeFilter.Add(".wma");
            picker.FileTypeFilter.Add(".m4a");
            picker.FileTypeFilter.Add(".ac3");
            picker.FileTypeFilter.Add(".aac");

           newFile = await picker.PickSingleFileAsync();
        }

        public StorageFile newFile;

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (newFile == null) return;
            ExternalMusic externalMusic = new ExternalMusic();
            externalMusic.Key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(newFile);
            externalMusic.MusicType = MusicType.External;
            externalMusic.Title = TitleTextBox.Text;
            externalMusic.Artist = ArtistTextBox.Text;
            externalMusic.Album = AlbumTextBox.Text;
            LibraryManager.AddExternalMusic(externalMusic);
        }

        private async void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (musicListControl.MainListView.SelectedItem == null)
                return;
            await ContentDialogHelper.ShowContentDialogAsync(new SaveMusicToPlaylistControl(musicListControl.MainListView.SelectedItem as Music));
        }
    }
}

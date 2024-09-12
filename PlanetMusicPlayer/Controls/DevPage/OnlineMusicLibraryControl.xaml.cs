using CorePlanetMusicPlayer.Models;
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
    public sealed partial class OnlineMusicLibraryControl : UserControl
    {
        public OnlineMusicLibraryControl()
        {
            this.InitializeComponent();
            Loaded += OnlineMusicLibraryControl_Loaded;
        }

        private void OnlineMusicLibraryControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        void RefreshData()
        {
            musicListControl.MainListView.ItemsSource = null;
            musicListControl.MainListView.ItemsSource = Library.Music.OnlineMusic;
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
            PlayCore.PlayMusic(musicListControl.MainListView.SelectedItem as Music, Library.Music.OnlineMusic.ToList<Music>(), musicListControl.MainListView.SelectedIndex);
        }

        private void AddToPlayQueue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(URLTextBox.Text))
                return;
            OnlineMusic onlineMusic = new OnlineMusic();
            onlineMusic.Title = TitleTextBox.Text;
            onlineMusic.Artist = ArtistTextBox.Text;
            onlineMusic.Album = AlbumTextBox.Text;
            onlineMusic.URL = URLTextBox.Text;
            LibraryManager.AddOnlineMusic(onlineMusic);
        }


    }
}

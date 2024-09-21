using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace PlanetMusicPlayer.Controls.DevPage
{
    public sealed partial class PlaylistLibraryControl : UserControl
    {
        public PlaylistLibraryControl()
        {
            this.InitializeComponent();
        }

        public void RefreshData()
        {
            PlaylistLibraryListView.ItemsSource = null;
            PlaylistLibraryListView.ItemsSource = Library.PlayLists;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void PlaylistLibraryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistLibraryListView.SelectedItem == null) return;
            Playlist playlist = PlaylistLibraryListView.SelectedItem as Playlist;
            PlaylistMusicListControl.MainListView.ItemsSource = playlist.Music;
            PlaylistMusicListControl.MainListView.ItemTemplate = (DataTemplate)Resources["NormalMusicListItem"];
            
        }

        private void PlaySelectedItem_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistLibraryListView.SelectedItem == null) return;
            if (PlaylistMusicListControl.MainListView.SelectedItem == null) return;
            Playlist playlist = PlaylistLibraryListView.SelectedItem as Playlist;
            PlayCore.PlayMusic(PlaylistMusicListControl.MainListView.SelectedItem as Music,playlist.Music, PlaylistMusicListControl.MainListView.SelectedIndex);
        }

        private void AddToPlayQueue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

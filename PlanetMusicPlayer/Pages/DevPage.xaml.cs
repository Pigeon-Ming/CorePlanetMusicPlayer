using PlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace PlanetMusicPlayer.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DevPage : Page
    {
        public DevPage()
        {
            this.InitializeComponent();
        }

        private void libraryListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            
        }

        private void MenuBar_File_RefreshLibrary(object sender, RoutedEventArgs e)
        {
            libraryListView.ItemsSource = null;
            libraryListView.ItemsSource = Library.LocalLibraryMusic;
            albumsListView.ItemsSource = null;
            albumsListView.ItemsSource = AlbumManager.Albums;
            artistsListView.ItemsSource = null;
            artistsListView.ItemsSource = ArtistManager.Artists;
            playQueueListView.ItemsSource = null;
            if(PlayCore.ShufflePlayMode == PlayCore.ShufflePlayModeEnum.None)
                playQueueListView.ItemsSource = PlayQueue.normalList;
            else
                playQueueListView.ItemsSource = PlayQueue.shuffleList;
            Debug.WriteLine(Library.LocalLibraryMusic.Count);
        }

        private void Library_CommandBar_Play(object sender, RoutedEventArgs e)
        {
            PlayCore.PlayMusic((Music)libraryListView.SelectedItem,Library.LocalLibraryMusic,libraryListView.SelectedIndex);
        }

        private void Main_CommandBar_Pause(object sender, RoutedEventArgs e)
        {
            PlayCore.PauseMusic();
        }

        private void Main_CommandBar_Play(object sender, RoutedEventArgs e)
        {
            PlayCore.PlayMusic();
        }
    }
}

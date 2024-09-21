using CorePlanetMusicPlayer.Models;
using PlanetMusicPlayer.Models;
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

namespace PlanetMusicPlayer.Controls.DevPage
{
    public sealed partial class LocalMusicLibraryControl : UserControl
    {
        public LocalMusicLibraryControl()
        {
            this.InitializeComponent();
            Loaded += LocalMusicLibraryControl_Loaded;
        }

        private void LocalMusicLibraryControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        void RefreshData()
        {
            musicListControl.MainListView.ItemsSource = null;
            musicListControl.MainListView.ItemsSource = Library.Music.LocalMusic;
            musicListControl.MainListView.ItemTemplate = (DataTemplate)Resources["NormalMusicListItem"];
            Debug.WriteLine(Library.Music.LocalMusic.Count);
        }

        private void RefreshListDataButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void PlaySelectedItem_Click(object sender, RoutedEventArgs e)
        {
            if(musicListControl.MainListView.SelectedItem==null)
                return;
            PlayCore.PlayMusic(musicListControl.MainListView.SelectedItem as Music,Library.Music.LocalMusic.ToList<Music>(),musicListControl.MainListView.SelectedIndex);
        }

        private void AddToPlayQueue_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if(musicListControl.MainListView.SelectedItem == null)
                return;
            await ContentDialogHelper.ShowContentDialogAsync(new SaveMusicToPlaylistControl(musicListControl.MainListView.SelectedItem as Music));
        }
    }
}

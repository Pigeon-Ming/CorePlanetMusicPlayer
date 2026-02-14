using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.App;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CorePlanetMusicPlayer6.Controls.DevControls
{
    public sealed partial class SaveToPlaylistControl : UserControl
    {
        private List<IMusic> musicList = new List<IMusic>();

        public SaveToPlaylistControl(IMusic music)
        {
            this.InitializeComponent();
            musicList.Add(music);
            PlaylistsListView.ItemsSource = PlaylistManager.Playlists;
        }

        public SaveToPlaylistControl(List<IMusic> musicList)
        {
            this.InitializeComponent();
            this.musicList = musicList;
            PlaylistsListView.ItemsSource = PlaylistManager.Playlists;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ProgramData.ContentDialogManager.HideContentDialog();
        }

        private async void PlaylistsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Playlist playlist = (Playlist)e.ClickedItem;
            playlist.Music = new ObservableCollection<IMusic>(playlist.Music.Concat(musicList));
            await PlaylistManager.SavePlaylistAsync(playlist);
            ProgramData.ContentDialogManager.HideContentDialog();
        }
    }
}

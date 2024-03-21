using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
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

namespace PlanetMusicPlayer.Controls
{
    public sealed partial class SaveToPlaylistControl : UserControl
    {
        public Music music {  get; set; }

        public SaveToPlaylistControl(Music music)
        {
            this.InitializeComponent();

            init();
            this.music = music;

        }

        async Task init()
        {
            MainListView.ItemsSource = await PlaylistManager.GetPlaylistsListAsync();
        }

        private void MainListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Playlist playlist = ((Playlist)e.ClickedItem);
            //playlist.includeMusic.Insert(0,PlayCore.CurrentMusic);
            playlist.includeMusic.Add(this.music);
            PlaylistManager.SetPlayList(playlist);
            MainPage.dialog.Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainPage.dialog.Hide();
        }
    }
}

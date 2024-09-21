using CorePlanetMusicPlayer.Models;
using PlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Color = Windows.UI.Color;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace PlanetMusicPlayer.Controls.DevPage
{
    public sealed partial class SaveMusicToPlaylistControl : UserControl
    {
        Music music { get; set; } = new Music();
        public SaveMusicToPlaylistControl(Music music)
        {
            this.InitializeComponent();
            this.music = music;
            PlaylistsListView.ItemsSource = Library.PlayLists;
        }

        private void CloseDialogButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogHelper.HideDialog();
        }


        private async void CreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(NewPlaylistName.Text))
            {
                NewPlaylistName.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                return;
            }
            ContentDialogHelper.HideDialog();
            await PlaylistManager.SavePlaylistAsync(new Playlist { Name = NewPlaylistName.Text, Music = new List<Music> { music } });
        }
    }
}

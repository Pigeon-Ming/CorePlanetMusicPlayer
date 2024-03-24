using CorePlanetMusicPlayer.Models;
using PlanetMusicPlayer.Controls;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace PlanetMusicPlayer.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlaylistPage : Page
    {
        public String playlistName { get; set; }

        public PlaylistPage(String playlistName)
        {
            this.InitializeComponent();
            PlaylistName_Block.Text = playlistName;
            this.playlistName = playlistName;
            init();
        }

        private async Task init()
        {
            Debug.WriteLine("PlaylistInit:"+playlistName);
            Playlist playlist = await PlaylistManager.ReadPlaylistAsync(playlistName);
            MusicListGrid.Children.Clear();
            MusicListGrid.Children.Add(new BasicMusicListControl(playlist.includeMusic));
        }
    }
}

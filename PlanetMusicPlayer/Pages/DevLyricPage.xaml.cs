using PlanetMusicPlayer.Controls.DevControls.LyricControls;
using PlanetMusicPlayer.Models;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace PlanetMusicPlayer.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DevLyricPage : Page
    {
        public DevLyricPage()
        {
            this.InitializeComponent();
        }

        private async void ScrollingLyric_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            List<Lyric> lyrics = await LyricManager.LoadFromLRCFileAndProcessAsync();
            ScrollingLyricControlGrid.Children.Clear();
            ScrollingLyricControlGrid.Children.Add(new ScrollingLyricControl(lyrics));
        }

        private void ScrollingLyric_ReadEmbeddedLyrics_Click(object sender, RoutedEventArgs e)
        {
            List<Lyric> lyrics = LyricManager.LoadFromMusicFile(PlayCore.CurrentMusic);
            ScrollingLyricControlGrid.Children.Clear();
            ScrollingLyricControlGrid.Children.Add(new ScrollingLyricControl(lyrics));
        }
    }
}

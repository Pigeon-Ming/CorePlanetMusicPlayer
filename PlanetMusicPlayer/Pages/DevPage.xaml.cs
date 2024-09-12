using CorePlanetMusicPlayer.Models;
using PlanetMusicPlayer.Controls.DevPage;
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
            Loaded += DevPage_Loaded;

        }

        private void DevPage_Loaded(object sender, RoutedEventArgs e)
        {
            LibraryManager.RefreshAllLibraryData();
            ContentGrid.Children.Add(new DevPageControlsShell("正在播放", new PlayingControl()));
            ContentGrid.Children.Add(new DevPageControlsShell("播放队列", new PlayQueueControl()));
            ContentGrid.Children.Add(new DevPageControlsShell("系统音乐库", new LocalMusicLibraryControl()));
            ContentGrid.Children.Add(new DevPageControlsShell("外部音乐库", new ExternalMusicLibraryControl()));
            ContentGrid.Children.Add(new DevPageControlsShell("在线音乐库", new OnlineMusicLibraryControl()));
            ContentGrid.Children.Add(new DevPageControlsShell("艺术家", new ArtistsLibraryControl()));
            ContentGrid.Children.Add(new DevPageControlsShell("专辑", new AlbumsLibraryControl()));
        }
    }
}

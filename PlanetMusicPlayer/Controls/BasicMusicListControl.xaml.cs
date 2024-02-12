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

namespace PlanetMusicPlayer.Controls
{
    public sealed partial class BasicMusicListControl : UserControl
    {

        public ListView mainListView { get { return MainListView; }}

        public BasicMusicListControl(List<Music>MusicList)
        {
            this.InitializeComponent();
            MainListView.ItemsSource = MusicList;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainListView.ItemsSource = null;
        }

        private void MainListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}

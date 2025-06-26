using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer6.Controls.LibraryPage;
using CorePlanetMusicPlayer6.Models;
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

namespace CorePlanetMusicPlayer6.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LibraryPage : Page
    {
        public LibraryPage()
        {
            this.InitializeComponent();
        }


        void UpdateData()
        {
            List<IMusic> musicList = new List<IMusic>();
            musicList.AddRange(ProgramData.SystemLibraryMusic);
            musicList.AddRange(ProgramData.OpenedFoldersMusic);
            musicList.AddRange(ProgramData.OpenedMusic);
            musicList.AddRange(ProgramData.StreamMusic);
            MainMusicListControl.PlayEngine = ProgramData.PlayEngine;
            MainMusicListControl.UpdateData(musicList);
            
        }

        private void FilterFlyout_Opened(object sender, object e)
        {
            FilterFlyout.Content = new MusicListFilterControl(MainMusicListControl);
        }

        private void FilterFlyout_Closed(object sender, object e)
        {
            FilterFlyout.Content = null;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainMusicListControl.ShowItemWithCover = true;
            UpdateData();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewHelper.SetPageHeaderMargin(PageHeaderGrid,e.NewSize);
        }
    }
}

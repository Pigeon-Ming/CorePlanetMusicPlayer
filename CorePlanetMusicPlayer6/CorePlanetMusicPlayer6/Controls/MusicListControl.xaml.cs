using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using CorePlanetMusicPlayer6.Models;
using CorePlanetMusicPlayer6.ViewModels;
using CorePlanetMusicPlayer6.ViewModels.DataModels;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls
{
    public sealed partial class MusicListControl : UserControl
    {
        public bool ShowItemWithCover { get; set; } = false;

        public MenuFlyout MusicMenuFlyout { get; set; }

        public List<IMusic> MusicList { get; set; }

        public MusicMenuViewModel MusicMenuViewModel { get; set; } = new MusicMenuViewModel();

        public IPlayEngine PlayEngine { get; set; }

        public MusicListControl()
        {
            this.InitializeComponent();
            MusicMenuFlyout = (MenuFlyout)Resources["DefaultMusicMenu"];
        }

        public void UpdateData(List<IMusic>musicList)
        {
            MusicList = musicList;
            MusicMenuViewModel.MusicList = musicList;
            if(PlayEngine != null)
                MusicMenuViewModel.PlayEngine = PlayEngine;
            MainListView.ContextFlyout = MusicMenuFlyout;
            if (ShowItemWithCover)
            {
                UpdateData_WithCover(musicList);
            }
            else
            {
                UpdateData_WithoutCover(musicList);
                
            }
        }

        void UpdateData_WithoutCover(List<IMusic>musicList)
        {
            MainListView.ItemTemplate = (DataTemplate)Resources["BasicListViewItemTemplate"];
            MainListView.ItemsSource = musicList;
        }

        async void UpdateData_WithCover(List<IMusic> musicList)
        {
            MainListView.ItemTemplate = (DataTemplate)Resources["BasicListViewItemTemplateWithCover"];
            List<ViewMusic> viewMusicList = new List<ViewMusic>();
            viewMusicList = await ViewMusicManager.FindViewMusicListInViewMusicListAsync(ProgramData.ViewMusic,musicList);
            MainListView.ItemsSource = viewMusicList;
        }

        private void MainListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ViewMusic viewMusic = ((e.OriginalSource as FrameworkElement).DataContext as ViewMusic);
            if(viewMusic != null)
                MusicMenuViewModel.SelectedMusic = viewMusic.Music;
            //MusicMenuViewModel.SelectedMusic = (e.OriginalSource as FrameworkElement).DataContext as IMusic;
            
            
        }

        private void MusicMenu_Opened(object sender, object e)
        {
            if (MusicMenuViewModel.SelectedMusic == null)
            {
                ((MenuFlyout)sender).Hide();
            }
        }
    }
}

using CorePlanetMusicPlayer6.Pages.Common;
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

namespace CorePlanetMusicPlayer6.Controls.Root
{
    public sealed partial class RootLibraryControl : UserControl
    {
        class LibraryNavigationItem
        {
            public LibraryNavigationItem(string title, string subtitle, Type targetPageType)
            {
                Title = title;
                Subtitle = subtitle;
                TargetPageType = targetPageType;
            }

            public string Title { get; set; }

            public string Subtitle { get; set; }

            public Type TargetPageType { get; set; }
        }

        Frame rootFrame;

        public RootLibraryControl()
        {
            this.InitializeComponent();

            rootFrame = Window.Current.Content as Frame;
        }

        private void NavigationListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            LibraryNavigationItem item = e.ClickedItem as LibraryNavigationItem;
            Type page = item?.TargetPageType;
            rootFrame.Navigate(page);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshNavigationListViewItems();
        }

        void RefreshNavigationListViewItems()
        {
            NavigationListView.Items.Clear();
            NavigationListView.Items.Add(new LibraryNavigationItem("音乐", "0个项目", typeof(MusicPage)));
            NavigationListView.Items.Add(new LibraryNavigationItem("艺术家", "0个项目", typeof(ArtistsPage)));
            NavigationListView.Items.Add(new LibraryNavigationItem("专辑", "0个项目", typeof(AlbumsPage)));
            NavigationListView.Items.Add(new LibraryNavigationItem("流派", "0个项目", typeof(GenresPage)));
            NavigationListView.Items.Add(new LibraryNavigationItem("年份", "0个项目", typeof(YearsPage)));
            NavigationListView.Items.Add(new LibraryNavigationItem("播放列表", "0个项目", typeof(PlaylistsPage)));
        }
    }
}

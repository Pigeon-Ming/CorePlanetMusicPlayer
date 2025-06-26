using CorePlanetMusicPlayer.PlayCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class RootPage : Page
    {
        public RootPage()
        {
            this.InitializeComponent();
        }

        private void MainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Navigate(((NavigationViewItem)MainNavigationView.SelectedItem).Tag.ToString());
        }

        public void Navigate(string tag)
        {
            switch(tag)
            {
                case "Home":
                    MainFrame.Navigate(typeof(HomePage));
                    break;
                case "Library":
                    MainFrame.Navigate(typeof(LibraryPage));
                    break;
                case "Album":
                    MainFrame.Navigate(typeof(AlbumsPage));
                    break;
                case "Artist":
                    MainFrame.Navigate(typeof(ArtistsPage));
                    break;
                case "Playlist":
                    MainFrame.Navigate(typeof(PlaylistsPage));
                    break;
            }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, arg) =>
            {
                MainFrame.GoBack();
            };
        }
    }
}

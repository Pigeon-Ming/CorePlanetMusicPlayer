using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.DevControls
{
    public sealed partial class AlbumsControl : UserControl
    {
        public AlbumsControl()
        {
            this.InitializeComponent();
            AlbumManager.Albums.CollectionChanged += Albums_CollectionChanged;
            SetListView();
        }

        private async void Albums_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetListView();
            });
        }

        void SetListView()
        {
            AlbumsListView.ItemsSource = null;
            AlbumsListView.ItemsSource = AlbumManager.Albums;
        }

        private void AlbumsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(AlbumsListView.SelectedItem != null)
                AlbumControl.SetAlbum((Album)AlbumsListView.SelectedItem);
            
        }
    }
}

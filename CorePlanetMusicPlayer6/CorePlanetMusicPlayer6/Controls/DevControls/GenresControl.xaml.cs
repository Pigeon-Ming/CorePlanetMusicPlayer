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
    public sealed partial class GenresControl : UserControl
    {
        public GenresControl()
        {
            this.InitializeComponent();
            GenreManager.Genres.CollectionChanged += Genres_CollectionChanged;
            SetListView();
        }

        private async void Genres_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetListView();
            });
        }

        void SetListView()
        {
            GenresListView.ItemsSource = null;
            GenresListView.ItemsSource = GenreManager.Genres;
        }

        private void GenresListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GenresListView.SelectedItem != null)
                GenreControl.SetGenre((Genre)GenresListView.SelectedItem);
        }
    }
}

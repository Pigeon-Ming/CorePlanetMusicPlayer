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
    public sealed partial class ArtistsControl : UserControl
    {
        public ArtistsControl()
        {
            this.InitializeComponent();
            SetListView();
            ArtistManager.Artists.CollectionChanged += Artists_CollectionChanged;
        }

        private async void Artists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetListView();
            });
        }

        void SetListView()
        {
            ArtistsListView.ItemsSource = null;
            ArtistsListView.ItemsSource = ArtistManager.Artists;
        }

        private void ArtistsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ArtistsListView.SelectedItem != null)
            {
                ArtistControl.SetArtist((Artist)ArtistsListView.SelectedItem);
            }
        }
    }
}

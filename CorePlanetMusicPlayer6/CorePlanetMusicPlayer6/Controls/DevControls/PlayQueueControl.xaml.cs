using CorePlanetMusicPlayer.PlayCore;
using CorePlanetMusicPlayer.App;
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
    public sealed partial class PlayQueueControl : UserControl
    {
        PlayQueue playQueue = ProgramData.PlayEngine.GetPlayQueue();

        public PlayQueueControl()
        {
            this.InitializeComponent();
            playQueue.PlayQueueChanged += PlayQueue_PlayQueueChanged;
            playQueue.CurrentIndexChanged += PlayQueue_CurrentIndexChanged;
        }

        private async void PlayQueue_CurrentIndexChanged(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetListView();
            });
        }

        void SetListView()
        {
            MainListView.ItemsSource = null;
            MainListView.ItemsSource = playQueue.GetQueue();
        }

        void SetPlayingItem()
        {
            if (playQueue.CurrentIndex != -1 && playQueue.CurrentIndex < MainListView.Items.Count)
            {
                var listViewItem = MainListView.ContainerFromIndex(playQueue.CurrentIndex);
                if (listViewItem == null)
                    return;
                var Control = ((ListViewItem)listViewItem).ContentTemplateRoot;
                ((Grid)Control).Children[0].Visibility = Visibility.Visible;
            }
        }

        private async void PlayQueue_PlayQueueChanged(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetListView();
            });
        }

        private void LocatePlayingButton_Click(object sender, RoutedEventArgs e)
        {
            locatePlaying();
        }

        void locatePlaying()
        {
            MainListView.ScrollIntoView(MainListView.Items[playQueue.CurrentIndex]);
        }

        private void RemoveSelectedItemButton_Click(object sender, RoutedEventArgs e)
        {
            removeItem(MainListView.SelectedIndex);
        }

        void removeItem(int index)
        {
            if (index == -1)
                return;
            playQueue.RemoveAt(index);
        }

        private void MainListView_LayoutUpdated(object sender, object e)
        {
            SetPlayingItem();
        }
    }
}

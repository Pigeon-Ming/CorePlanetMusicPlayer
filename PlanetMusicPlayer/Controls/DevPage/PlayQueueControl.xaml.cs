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

namespace PlanetMusicPlayer.Controls.DevPage
{
    public sealed partial class PlayQueueControl : UserControl
    {
        public PlayQueueControl()
        {
            this.InitializeComponent();
            PlayQueue.PlayQueueChanged += PlayQueue_PlayQueueChanged;
            
        }

        private void PlayQueue_PlayQueueChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        public void RefreshData()
        {
            musicListControl.MainListView.ItemsSource = null;
            musicListControl.MainListView.ItemTemplate = (DataTemplate)Resources["NormalMusicListItem"];
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                musicListControl.MainListView.ItemsSource = PlayQueue.normalList.ToList<Music>();
            }
            else
            {
                musicListControl.MainListView.ItemsSource = PlayQueue.shuffleList.ToList<Music>();
            }
        }

        public void SelectPlayingMusic()
        {
            musicListControl.MainListView.SelectedIndex = PlayQueue.playingMusicIndex;
        }

        private void LocatedPlayingItemButton_Click(object sender, RoutedEventArgs e)
        {
            SelectPlayingMusic();
        }

        private void PlaySelectedItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (musicListControl.MainListView.SelectedItem == null)
                return;
            List<Music>newPlayQueue = new List<Music>();
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                newPlayQueue = PlayQueue.normalList;
            }
            else
            {
                newPlayQueue = PlayQueue.shuffleList;
            }
            PlayCore.PlayMusic(musicListControl.MainListView.SelectedItem as Music,newPlayQueue,musicListControl.MainListView.SelectedIndex);
        }
    }
}

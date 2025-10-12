using CorePlanetMusicPlayer.Models;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.DevControls
{
    public sealed partial class MusicLibraryControl : UserControl
    {
        private List<IMusic> currentItems = new List<IMusic>();

        public MusicLibraryControl()
        {
            this.InitializeComponent();
            refreshSourceList();
        }

        private void MusicSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MusicListView.ItemsSource = null;
            switch (MusicSourceComboBox.SelectedIndex)
            {
                case 0:
                    currentItems = Library.LocalMusic.ToList<IMusic>();
                    break;
            }
            MusicListView.ItemsSource = currentItems;
        }

        private void ScanMusicLibrary_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshSourceListButton_Click(object sender, RoutedEventArgs e)
        {
            refreshSourceList();
        }

        void refreshSourceList()
        {
            MusicSourceComboBox.Items.Clear();
            MusicSourceComboBox.Items.Add("本地音乐库");
        }

        private void MusicListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Play((LocalMusic)MusicListView.SelectedItem);
        }

        void Play(IMusic music)
        {
            ProgramData.PlayEngine.PlayMusic(music, currentItems, currentItems.IndexOf(music));
        }
    }
}

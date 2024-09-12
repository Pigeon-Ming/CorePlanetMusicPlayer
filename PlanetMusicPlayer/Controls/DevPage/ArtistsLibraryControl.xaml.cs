using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class ArtistsLibraryControl : UserControl
    {
        public ArtistsLibraryControl()
        {
            this.InitializeComponent();
            Loaded += ArtistsLibraryControl_Loaded;
        }

        private void ArtistsLibraryControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        void RefreshData()
        {
            ArtistsListView.ItemsSource = null;
            ArtistsListView.ItemsSource = Library.Artists;
            Debug.WriteLine("艺术家数量："+Library.Artists.Count);
        }

        List<Music> musicList = new List<Music>();

        private void ArtistsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            musicList = (e.ClickedItem as Artist).Music.ToList();
            musicListControl.MainListView.ItemsSource = null;
            musicListControl.MainListView.ItemsSource = musicList;
            musicListControl.MainListView.ItemTemplate = (DataTemplate)Resources["NormalMusicListItem"];
            
        }

        private void RefreshListDataButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void PlaySelectedItem_Click(object sender, RoutedEventArgs e)
        {
            if (musicListControl.MainListView.SelectedItem == null) return;
            PlayCore.PlayMusic(musicListControl.MainListView.SelectedItem as Music,musicList, musicListControl.MainListView.SelectedIndex);
        }

        private void AddToPlayQueue_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

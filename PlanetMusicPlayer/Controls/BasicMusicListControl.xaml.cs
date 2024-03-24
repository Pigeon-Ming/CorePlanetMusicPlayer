using CorePlanetMusicPlayer.Models;
using PlanetMusicPlayer.ViewModels;
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

namespace PlanetMusicPlayer.Controls
{
    public sealed partial class BasicMusicListControl : UserControl
    {
        public MusicListViewModel viewModel { get; set;}
        public ListView mainListView { get { return MainListView; }}
        public List<Music>MusicList { get; set; }

        public BasicMusicListControl(List<Music>musicList)
        {
            this.InitializeComponent();
            MainListView.ItemsSource = musicList;
            this.MusicList = musicList;
            viewModel = new MusicListViewModel(musicList, MainListView);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MainListView.ItemsSource = null;
        }

        private void MainListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void MainListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            
            PlayCore.PlayMusic((Music)MainListView.SelectedItem,EventList<Music>.ListToEventList(MusicList),MainListView.SelectedIndex);
        }

        private void MainListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            viewModel.rightClickItem = (e.OriginalSource as FrameworkElement).DataContext as Music;
            if (viewModel.rightClickItem == null)
            {
                rightClickMenu.Hide();
                return;
            }
            rightClickMenu_MusicName.Text = viewModel.rightClickItem.Title;
        }

        private void MainListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.selectedItem = (Music)MainListView.SelectedItem;
        }
    }
}

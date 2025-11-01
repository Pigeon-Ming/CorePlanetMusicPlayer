using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer6.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class PlaylistsControl : UserControl
    {
        public PlaylistsControl()
        {
            this.InitializeComponent();
            SetListView();
            PlaylistManager.Playlists.CollectionChanged += Playlists_CollectionChanged;
            PlaylistManager.PlaylistChanged += PlaylistManager_PlaylistChanged;
        }

        private async void PlaylistManager_PlaylistChanged(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetListView();
            });
        }

        private async void Playlists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SetListView();
            });
        }

        void SetListView()
        {
            PlaylistsListView.ItemsSource = null;
            PlaylistsListView.ItemsSource = PlaylistManager.Playlists;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            _ = CreatePlaylistAsync();
        }

        async Task CreatePlaylistAsync()
        {
            Debug.WriteLine(NewPlaylistTextBox.BorderBrush.ToString());
            if (String.IsNullOrEmpty(NewPlaylistTextBox.Text) || !StringHelper.IsValidFileName(NewPlaylistTextBox.Text))
            {
                NewPlaylistTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }
            NewPlaylistTextBox.BorderBrush = null;
            await PlaylistManager.SavePlaylistAsync(new Playlist { Title = NewPlaylistTextBox.Text });
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistsListView.SelectedItem == null)
                return;
            _ = DeletePlaylistAsync(PlaylistsListView.SelectedItem as Playlist);
        }

        async Task DeletePlaylistAsync(Playlist playlist)
        {
            await PlaylistManager.DeletePlaylistAsync(playlist);
        }

        private void PlaylistsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistsListView.SelectedItem != null)
            {
                PlaylistControl.SetPlaylist((Playlist)PlaylistsListView.SelectedItem);
            }
        }
    }
}

using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

        private async void MusicSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await SourceChangedAsync();
        }

        async Task SourceChangedAsync()
        {
            MusicListView.ItemsSource = null;
            Menu_Remove.IsEnabled = false;
            if (MusicSourceComboBox.SelectedIndex == 0)
            {
                currentItems = Library.LocalMusic.ToList<IMusic>();
            }
            else if (MusicSourceComboBox.SelectedIndex == 1)
            {
                currentItems = Library.StreamMusic.ToList<IMusic>();
                Menu_Remove.IsEnabled = true;
            }
            else if (MusicSourceComboBox.SelectedIndex != -1)
            {
                RemovableDevice removableDevice = RemovableDeviceManager.RemovableDevices[MusicSourceComboBox.SelectedIndex - 1];
                if (removableDevice.Music == null)
                    await RemovableDeviceManager.GetRemovableDeviceMusicListAsync(removableDevice);
                currentItems = removableDevice.Music.ToList<IMusic>();
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
            MusicSourceComboBox.Items.Add("流式传输音乐库");
            foreach (RemovableDevice removableDevice in RemovableDeviceManager.RemovableDevices)
            {
                MusicSourceComboBox.Items.Add($"[可移动设备] - {removableDevice.Name}");
            }
        }

        private void MusicListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (MusicListView.SelectedItem is LocalMusic)
                Play((LocalMusic)MusicListView.SelectedItem);
            else if(MusicListView.SelectedItem is StreamMusic)
                Play((StreamMusic)MusicListView.SelectedItem);
            else if(MusicListView.SelectedItem is RemovableMusic)
                Play((RemovableMusic)MusicListView.SelectedItem);
        }

        void Play(IMusic music)
        {
            ProgramData.PlayEngine.PlayMusic(music, currentItems, currentItems.IndexOf(music));
        }

        IMusic rightClickedMusic;

        private void Menu_SaveToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (rightClickedMusic != null)
                _ = SaveToPlaylistAsync(rightClickedMusic);
        }

        async Task SaveToPlaylistAsync(IMusic music)
        {
            await ProgramData.ContentDialogManager.ShowContentDialogAsync(new SaveToPlaylistControl(music));
        }

        private void MusicListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement) != null)
                rightClickedMusic = (IMusic)(e.OriginalSource as FrameworkElement).DataContext;
        }

        private void AddStreamMusicButton_Click(object sender, RoutedEventArgs e)
        {
            _ = AddStreamMusicAsync();
        }

        async Task AddStreamMusicAsync()
        {
            await ProgramData.ContentDialogManager.ShowContentDialogAsync(new SaveStreamMusicControl());
        }

        private async void Menu_Remove_Click(object sender, RoutedEventArgs e)
        {
            List<StreamMusic> streamMusic = new List<StreamMusic>();
            streamMusic.Add(rightClickedMusic as StreamMusic);
            await Library.DeleteStreamMusicAsync(streamMusic);
        }
    }
}

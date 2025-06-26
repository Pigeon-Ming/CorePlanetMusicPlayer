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

namespace CorePlanetMusicPlayer6.Controls.LibraryPage
{
    public sealed partial class MusicListFilterControl : UserControl
    {
        MusicListControl MusicListControl { get; set; }
        public MusicListFilterControl(MusicListControl musicListControl)
        {
            this.InitializeComponent();
            MusicListControl = musicListControl;
            if (musicListControl == null || musicListControl.MusicList == null)
            {
                FilterControls.Visibility = Visibility.Collapsed;
                ErrorTipTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void RestoreDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            MusicSource_SystemLibrary_CheckBox.IsChecked = MusicSource_ScanedFolder_CheckBox.IsChecked = MusicSource_RecentOpendFile_CheckBox.IsChecked = MusicSource_StreamAudio_CheckBox.IsChecked = true;
            ApplyFilter();
        }

        private void ApplyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        void ApplyFilter()
        {
            List<IMusic> musicList = new List<IMusic>();
            
            if (MusicSource_SystemLibrary_CheckBox.IsChecked == true)
            {
                musicList.AddRange(ProgramData.SystemLibraryMusic);
            }
            if (MusicSource_ScanedFolder_CheckBox.IsChecked == true)
            {
                musicList.AddRange(ProgramData.OpenedFoldersMusic);
            }
            if (MusicSource_RecentOpendFile_CheckBox.IsChecked == true)
            {
                musicList.AddRange(ProgramData.OpenedMusic);
            }
            if (MusicSource_StreamAudio_CheckBox.IsChecked == true)
            {
                musicList.AddRange(ProgramData.StreamMusic);
            }

            MusicListControl.UpdateData(musicList);
        }
    }
}

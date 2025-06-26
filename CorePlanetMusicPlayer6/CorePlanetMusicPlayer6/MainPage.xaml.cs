using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer6.Models;
using CorePlanetMusicPlayer6.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace CorePlanetMusicPlayer6
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await GetDataAsync();
            Frame.Navigate(typeof(RootPage));
            


        }

        async Task GetDataAsync()
        {
            await ProgramData.RefreshSystemLibraryMusicListAsync();
            await ProgramData.RefreshOpenedFolderAsync();
            await ProgramData.RefreshOpenedFolderMusicListAsync();
            await ProgramData.RefreshViewMusicListAsync();
            GetCoverData();


            Debug.WriteLine("系统音乐库歌曲数量："+ ProgramData.SystemLibraryMusic.Count);
        }

        async void GetCoverData()
        {
            foreach (LocalMusic localMusic in ProgramData.SystemLibraryMusic)
            {
                await LocalMusicManager.GetProperties_MixedAsync(localMusic);
            }
        }
    }
}

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
using Windows.System;
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
    public sealed partial class DevToolControl : UserControl
    {
        public DevToolControl()
        {
            this.InitializeComponent();
            Init();
        }

        void Init()
        {
            OpenLocalFolderButton.Click += (sender, e) =>
            {
                var t = new FolderLauncherOptions();
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                Launcher.LaunchFolderAsync(folder, t);
            };
            LoadLibraryButton.Click += (sender, e) =>
            {
                LibraryManager.InitAllDataAsync();
            };
            LoadDataButton.Click += (sender, e) =>
            {
                List<Music> musicList = SQLiteManager.MusicCacheDataBasesHelper.GetTableData(ApplicationData.Current.LocalFolder.Path + "\\Cache\\MusicCache.db", "LocalMusic");
                Debug.WriteLine(musicList.Count);
            };
            //LoadMusicCacheButton.Click += (sender, e) =>
            //{
            //    LibraryManager.
            //};
        }
    }
}

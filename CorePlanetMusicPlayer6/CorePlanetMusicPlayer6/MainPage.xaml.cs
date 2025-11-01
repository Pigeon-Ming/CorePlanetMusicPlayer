using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using CorePlanetMusicPlayer6.Models;
using CorePlanetMusicPlayer6.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPTools.Models;
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
            

            // 初始化设置管理
            SettingsManager.InitUserSettings(ApplicationData.Current.LocalSettings);
            //Frame.Navigate(typeof(RootPage));
            Frame.Navigate(typeof(DevPage));
            // 初始化数据库
            await DataBaseManager.InitDataBasesAsync();
            // 初始化播放队列恢复文件
            await PlayQueue.Init();
            // 初始化播放记录
            await PlayRecordManager.InitAsync(ProgramData.PlayEngine);
            // 启动播放记录
            PlayRecordManager.StartPlayRecord();
            
            
            await GetDataAsync();

            List<LocalMusic> localMusicList = Library.LocalMusic.ToList();

            // 初始化播放列表管理
            await PlaylistManager.InitAsync(localMusicList);
            

            // To-Do: 添加是否恢复播放列表的判断
            await ProgramData.PlayEngine.GetPlayQueue().ResumePlayQueueAsync(localMusicList);

            // 获取Playlist
            await PlaylistManager.GetPlaylistsAsync();

            
        }

        async Task GetDataAsync()
        {
            // 获取文件夹列表
            await Library.GetFoldersFromFutureAccessListAsync();
            // 获取LocalMusic
            await Library.GetLocalMusicAsync();
            await Library.GetLocalMusicPropertiesAsync();
            // 获取StreamMusic
            await Library.GetStreamMusicAsync();
            
            // 启动可移动设备监听
            RemovableDeviceManager.StartWatcher();
            // 获取当前可移动设备列表
            await RemovableDeviceManager.RefreshDevicesListAsync();
            // 获取分类信息 获取以后可以整个缓存🤔
            List<IMusic> musicList = Library.GetAllMusicList();

            DateTime dateTime = DateTime.Now;
            DateTime dateTime1;
            Debug.WriteLine($"{dateTime.ToString(@"yyyy/MM/dd HH:mm:ss.ff")} - 开始整理分类信息");
            
            ArtistManager.RefreshArtistsList(musicList);
            AlbumManager.RefreshAlbumsList(musicList);
            GenreManager.RefreshGenresList(musicList);
            YearManager.RefreshYearsList(musicList);
            
            dateTime1 = DateTime.Now;
            Debug.WriteLine($"{dateTime1.ToString(@"yyyy/MM/dd HH:mm:ss.ff")} - 分类信息整理完毕");
            Debug.WriteLine($"整理分类信息共用时：{(dateTime1.Subtract(dateTime))}");
            //Debug.WriteLine("系统音乐库歌曲数量："+ ProgramData.SystemLibraryMusic.Count);
        }

        //async void GetCoverData()
        //{
        //    foreach (LocalMusic localMusic in ProgramData.SystemLibraryMusic)
        //    {
        //        await LocalMusicManager.GetProperties_MixedAsync(localMusic);
        //    }
        //}
    }
}

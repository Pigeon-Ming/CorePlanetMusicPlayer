using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using CorePlanetMusicPlayer6.ViewModels.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer6.Models
{
    public class ProgramData
    {
        public static IPlayEngine PlayEngine { get; set; } = new SystemMediaPlayer();




        public static List<LocalMusic> SystemLibraryMusic { get; set; } = new List<LocalMusic>();//系统音乐库中的音乐

        public static List<StorageFolder> OpenedFolders { get; set; } = new List<StorageFolder>();//添加扫描的文件夹
        public static List<LocalMusic> OpenedFoldersMusic { get; set; } = new List<LocalMusic>();//添加扫描文件夹中的音乐
        public static List<LocalMusic> OpenedMusic { get; set; } = new List<LocalMusic>();//最近打开的音乐文件

        public static List<OnlineMusic> StreamMusic { get; set; } = new List<OnlineMusic>();//流式传输的音频

        public static List<ViewMusic> ViewMusic { get; set; } = new List<ViewMusic>();






        public static event EventHandler SystemLibraryMusicListChanged;
        public static async Task RefreshSystemLibraryMusicListAsync()
        {
            List<StorageFolder> libraryFolders = (await KnownFolders.MusicLibrary.GetFoldersAsync()).ToList();
            List<LocalMusic> musicList = new List<LocalMusic>();
            foreach (StorageFolder folder in libraryFolders)
            {
                musicList.AddRange(await LocalMusicManager.GetLocalMusicFromStorageFolderAsync(folder));
            }
            SystemLibraryMusic = musicList;
            SystemLibraryMusicListChanged?.Invoke(SystemLibraryMusic, null);
        }

        public static async Task RefreshOpenedFolderAsync()
        {
            List<StorageFolder> folders = new List<StorageFolder>();
            List<string> tokens = await FutureAccessListManager.ReadFolderTokensAsync();
            if (tokens == null)
                return;
            foreach (string token in tokens)
            {
                folders.Add(await FutureAccessListManager.GetFolderFromTokensAsync(token));
            }
            OpenedFolders = folders;
        }

        public static async Task RefreshOpenedFolderMusicListAsync()
        {
            List<LocalMusic> musicList = new List<LocalMusic>();
            foreach(StorageFolder folder in OpenedFolders)
            {
                musicList = musicList.Concat(await LocalMusicManager.GetLocalMusicFromStorageFolderAsync(folder)).ToList();
            }
            OpenedFoldersMusic = musicList;
        }

        public static async Task RefreshViewMusicListAsync()
        {
            List<ViewMusic> viewMusicList = new List<ViewMusic>();
            foreach (LocalMusic music in SystemLibraryMusic)
            {
                viewMusicList.Add(await ViewMusicManager.CreateViewMusicAsync(music));
            }
            foreach (LocalMusic music in OpenedFoldersMusic)
            {
                viewMusicList.Add(await ViewMusicManager.CreateViewMusicAsync(music));
            }
            foreach (LocalMusic music in OpenedMusic)
            {
                viewMusicList.Add(await ViewMusicManager.CreateViewMusicAsync(music));
            }
        }

    }
}

using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace CorePlanetMusicPlayer.App
{
    public class Library
    {
        public static ObservableDictionary<StorageFolder, string> Folders { get; set; } = new ObservableDictionary<StorageFolder, string>();//音乐库包含的文件夹

        public static ObservableCollection<LocalMusic> LocalMusic { get; set; } = new ObservableCollection<LocalMusic>();//音乐库文件夹扫描出的音乐

        public static ObservableDictionary<LocalMusic, string> OpenedMusic { get; set; } = new ObservableDictionary<LocalMusic, string>();//通过文件管理器打开的本地音乐（如：从文件夹中拖拽进应用或通过在文件夹中选择pmp为打开方式的本地音乐）

        //public static ObservableCollection<LocalMusic> RemoveableLocalMusic { get; set; } = new ObservableCollection<LocalMusic>();//可移动设备中的本地音乐 如：U盘、移动硬盘


        public static ObservableCollection<StreamMusic> StreamMusic { get; set; } = new ObservableCollection<StreamMusic>();//流式传输的歌曲

        //Library
        public static List<IMusic> GetAllMusicList()
        {
            List<IMusic> musicList = new List<IMusic>();
            musicList.AddRange(LocalMusic);
            musicList.AddRange(OpenedMusic.Keys.ToList());
            musicList.AddRange(StreamMusic);
            return musicList;
        }

        //Folders

        /// <summary>
        /// 使用文件夹选择器FolderPicker添加文件夹至音乐库包含的文件夹
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        public static async Task AddFolderFromFolderPickerAsync()
        {
            FolderPicker folderPicker = new FolderPicker();
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null)
                return;
            if(Folders.ContainsKey(folder))
                return;
            await AddFolderAsync(folder);
        }

        /// <summary>
        /// 获取文件夹的FutureAccessListToken并将其添加至Folders字典中，随后保存Tokens
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        public static async Task AddFolderAsync(StorageFolder storageFolder)
        {
            Folders.Add(storageFolder, StorageHelper.GetFutureAccessListToken(storageFolder));
            await FutureAccessListManager.SaveFolderTokensAsync(Folders.Values.ToList());
        }

        /// <summary>
        /// 删除音乐库包含的某一个文件夹
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        public static async Task RemoveFolderAsync(StorageFolder storageFolder)
        {
            Folders.Remove(storageFolder);
            await FutureAccessListManager.SaveFolderTokensAsync(Folders.Values.ToList());
        }

        public static async Task GetFoldersFromFutureAccessListAsync()
        {
            Folders.Clear();
            List<string> tokens = await FutureAccessListManager.ReadFolderTokensAsync();
            if (tokens == null)
                return;
            foreach (string token in tokens)
            {
                Folders.Add(await StorageHelper.GetStorageFolderFromFutureAccessListAsync(token), token);
            }
        }

        //LocalMusic
        public static async Task GetLocalMusicAsync()
        {
            LocalMusic.Clear();
            List<LocalMusic> localMusicList = new List<LocalMusic>();
            foreach (StorageFolder storageFolder in Folders.Keys)
            {
                localMusicList.AddRange(await LocalMusicManager.GetLocalMusicFromStorageFolderAsync(storageFolder));
            }

            LocalMusic = new ObservableCollection<LocalMusic>(LocalMusic.Concat(localMusicList));
        }

        public static async Task GetLocalMusicPropertiesAsync()
        {
            //1.从数据库中读取数据
            List<Music> musicList = await DataBaseManager.GetLocalMusicDataAsync();
            List<LocalMusic> noDataLocalMusic = new List<LocalMusic>();
            foreach(LocalMusic localMusic in LocalMusic)
            {
                Music music = musicList.Find(m => m.Token == localMusic.StorageFile.Path);
                if (music != null)
                    LocalMusicManager.Music2LocalMusic(localMusic, music);
                else
                    noDataLocalMusic.Add(localMusic);
            }
            //2.获取数据库中没有缓存信息的歌曲
            foreach (LocalMusic localMusic in noDataLocalMusic)
            {
                await LocalMusicManager.GetProperties_MixedAsync(localMusic);
            }
            //3.将没有缓存信息的歌曲缓存写入数据库
            await DataBaseManager.UpdateLocalMusicDataAsync(noDataLocalMusic);
        }

        //StreamMusic
        public static async Task SaveStreamMusicAsync(StreamMusic streamMusic)
        {
            var a = StreamMusic.Where(x => x.Url == streamMusic.Url).ToList();
            StreamMusic oldItem = null;
            if (a != null && a.Count != 0)
                oldItem = a.First();
            if (oldItem != null)
            {
                oldItem.Title = streamMusic.Title;
                oldItem.Artist = streamMusic.Artist;
                oldItem.Album = streamMusic.Album;
                oldItem.Duration = streamMusic.Duration;
                oldItem.Bitrate = streamMusic.Bitrate;
                oldItem.DiscNumber = streamMusic.DiscNumber;
                oldItem.TrackNumber = streamMusic.TrackNumber;
                oldItem.Genre = streamMusic.Genre;
                oldItem.Year = streamMusic.Year;
                oldItem.CoverUrl = streamMusic.CoverUrl;
            }
            else
            {
                StreamMusic.Add(streamMusic);
                AlbumManager.AddMusicToAlbum(streamMusic);
                ArtistManager.AddMusicToArtist(streamMusic);
                GenreManager.AddMusicToGenre(streamMusic);
                YearManager.AddMusicToYears(streamMusic);
            }
            await DataBaseManager.UpdateStreamMusicDataAsync(StreamMusic.ToList());

            
            
        }

        public static async Task DeleteStreamMusicAsync(List<StreamMusic>streamMusicList)
        {
            await DataBaseManager.DeleteStreamMusicDataAsync(streamMusicList);

            List<IMusic> musicList = streamMusicList.ToList<IMusic>();

            ArtistManager.RemoveMusicFromArtist(musicList);
            AlbumManager.RemoveMusicFromAlbum(musicList);
            YearManager.RemoveMusicFromYear(musicList);
            GenreManager.RemoveMusicFromGenre(musicList);
            await GetStreamMusicAsync();
        }
        

        public static async Task GetStreamMusicAsync()
        {
            StreamMusic.Clear();
            List<StreamMusic> streamMusicList = await DataBaseManager.GetStreamMusicDataAsync();
            StreamMusic = new ObservableCollection<StreamMusic>(StreamMusic.Concat(streamMusicList));
        }


        //OpenedMusic
        public static async Task GetOpenedMusicAsync()
        {
            OpenedMusic.Clear();
            Dictionary<LocalMusic,string> musicList = new Dictionary<LocalMusic, string>();
            List<string> tokens = await FutureAccessListManager.ReadFileTokensAsync();
            foreach (string token in tokens)
            {
                musicList.Add(LocalMusicManager.CreateLocalMusicFromStorageFile(await StorageHelper.GetStorageFileFromFutureAccessListAsync(token)) ,token);
            }
            //OpenedMusic = new ObservableDictionary<LocalMusic, string>(OpenedMusic.Concat(musicList));
        }


        public static async Task AddOpenedMusicAsync(StorageFile storageFile)
        {
            LocalMusic localMusic = LocalMusicManager.CreateLocalMusicFromStorageFile(storageFile);
            await LocalMusicManager.GetProperties_MixedAsync(localMusic);
            OpenedMusic.Add(localMusic, StorageHelper.GetFutureAccessListToken(storageFile));
            await FutureAccessListManager.SaveFileTokensAsync(OpenedMusic.Values.ToList());
        }

        public static async Task<bool> RemoveOpenedMusicAsync(LocalMusic openedMusic)
        {
            string token;
            if(OpenedMusic.TryGetValue(openedMusic,out token))
            {
                OpenedMusic.Remove(openedMusic);
                StorageHelper.RemoveFutureAccessListToken(token);
                await FutureAccessListManager.SaveFileTokensAsync(OpenedMusic.Values.ToList());
                return true;
            }
            else
                return false;
        }

        

        

        
    }
}

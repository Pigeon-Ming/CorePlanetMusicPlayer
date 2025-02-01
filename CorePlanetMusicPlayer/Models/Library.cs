using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Id3v2;
using Windows.Data.Json;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public class Library
    {   
        public static List <StorageFile> MusicFiles { get; set; } = new List<StorageFile>();
        public static List <Music> Music { get; set; } = new List<Music>();
        public static List<String> ExternalMusicKeys { get; set; } = new List<String>();
        public static List<StorageFile> TempMusicFiles { get; set; } = new List<StorageFile>();
        public static List<Music> MusicCache { get; set; } = new List<Music>();
        public static List <Artist> Artists { get; set; } = new List<Artist>();
        public static List <Album> Albums { get; set; } = new List<Album>();

        public static List<RemovableDevice> RemovableDevices { get; set; } = new List<RemovableDevice>();

        public static List <DataCodeImage> MusicCovers { get; set; } = new List<DataCodeImage> { };
        public static List<Playlist> Playlists { get; set; } = new List<Playlist>();
    }

    public class LibraryManager
    {
        public static string InitWhatDataStr { get; set; } = "整理音乐库……";
        public static async Task InitAllDataAsync()
        {
            InitWhatDataStr = "查询缓存信息……";
            await GetMusicCacheData();
            InitWhatDataStr = "查找文件……";
            await GetMusicFilesDataAsync();
            InitWhatDataStr = "获取外部文件……";
            await GetExternalMusicStorageFiles();
            InitWhatDataStr = "获取在线音乐信息……";
            GetOnlineMusicData();
            InitWhatDataStr = "处理临时文件……";
            await AddTempMusicListToExternalMusic();
            InitWhatDataStr = "获取播放列表信息……";
            await RefreshPlaylistsData();
            if (CorePMPSettings.Library_GetMusicCoverWhenLoad)
            {
                InitWhatDataStr = "获取歌曲封面……";
                RefreshImagesDataAsync();
            }
            InitWhatDataStr = "整理艺术家……";
            await RefreshArtistsData();
            InitWhatDataStr = "整理专辑……";
            await RefreshAlbumsData();
        }

        public static async Task AddTempMusicListToExternalMusic()
        {
            for(int i=0;i<Library.TempMusicFiles.Count;i++)
            {
                if(Library.MusicFiles.Find(x=>x.Path == Library.TempMusicFiles[i].Path)==null)
                {
                    Library.MusicFiles.Add(Library.TempMusicFiles[i]);
                    await MusicManager.AddExternalMusicAsync(Library.TempMusicFiles[i]);
                }
            }
        }

        public static async Task GetMusicCacheData()
        {
            Library.MusicCache.Clear();
            StorageFolder folder = await StorageManager.GetApplicationDataFolder("Cache");
            await SQLiteManager.MusicDataBasesHelper.CreateTableAsync(folder,"MusicCache","LocalMusic");
            await SQLiteManager.MusicDataBasesHelper.CreateTableAsync(folder,"MusicCache","ExternalLocalMusic");
            Library.MusicCache = SQLiteManager.MusicDataBasesHelper.GetTableData(ApplicationData.Current.LocalFolder.Path + "\\Cache\\MusicCache.db","LocalMusic");
            Library.MusicCache.AddRange(SQLiteManager.MusicDataBasesHelper.GetTableData(ApplicationData.Current.LocalFolder.Path + "\\Cache\\MusicCache.db","ExternalLocalMusic"));
            Debug.WriteLine("MusicCache数量："+Library.MusicCache.Count);
        }

        public static async Task GetMusicFilesDataAsync()
        {
            Library.MusicFiles.Clear();
            Queue<StorageFolder> folderQueue = new Queue<StorageFolder>();
            folderQueue.Enqueue(KnownFolders.MusicLibrary);
            do
            {
                IReadOnlyList<IStorageItem> folderList = await folderQueue.Dequeue().GetItemsAsync();
                foreach (var item in folderList)
                {
                    if (item is StorageFolder)
                    {
                        folderQueue.Enqueue((StorageFolder)item);
                    }
                    else if(item is StorageFile)
                    {
                        //MusicFileCount ++;
                        string fileName = item.Name;
                        //Debug.WriteLine(fileName+"|||"+fileName.Substring(fileName.LastIndexOf(".")));
                        StorageFile storageFile = item as StorageFile;
                        string fileSuffix = storageFile.FileType;
                        if (fileSuffix == ".mp3" || fileSuffix == ".flac" || fileSuffix == ".wma" || fileSuffix == ".m4a" || fileSuffix == ".ac3" || fileSuffix == ".aac")
                        {
                            //StorageFile storageFile = item as StorageFile;
                            Library.MusicFiles.Add(storageFile);
                        }
                    }
                }
            } while (folderQueue.Count > 0);
            for (int i = 0; i < Library.MusicFiles.Count; i++)
            {
                Music music = Library.MusicCache.Find(x => x.DataCode == Library.MusicFiles[i].Path);
                if (music != null)
                    Library.Music.Add(music);
                else
                {
                    music = new Music();
                    music.DataCode = Library.MusicFiles[i].Path;
                    music.MusicType = MusicType.Local;
                    music = await MusicManager.GetLocalMusicPropertiesAsync(music);
                    MusicManager.SetMusicCache(music);
                    Library.Music.Add(music);
                }
            }
        }

        public static async Task GetExternalMusicStorageFiles()
        {
            Library.ExternalMusicKeys = await MusicManager.GetExternalLocalMusicKeys();
            for(int i=0;i< Library.ExternalMusicKeys.Count;i++)
            {
                StorageFile storageFile = await MusicManager.GetExternalMusicByExternalMusicKeyAsync(Library.ExternalMusicKeys[i]);
                Library.MusicFiles.Add(storageFile);
                Music music = new Music { MusicType = MusicType.ExternalLocal,DataCode = storageFile.Path ,Key = Library.ExternalMusicKeys[i] };
                MusicManager.SetMusicCache(music);
                Library.Music.Add(await MusicManager.GetLocalMusicPropertiesAsync(music));
            }
        }

        public static void GetOnlineMusicData()
        {
            Library.Music.AddRange(SQLiteManager.MusicDataBasesHelper.GetTableData(StorageManager.LocalFolderPath + "\\DataBases\\MusicLibrary.db","OnlineMusic"));
        }

        public static StorageFile GetLocalMusicFile(Music music)
        {
            if(music.MusicType!=MusicType.Local && music.MusicType!=MusicType.ExternalLocal)
                return null;
            return Library.MusicFiles.Find(x=>x.Path == music.DataCode);
        }

        public static async Task<List<StorageFolder>> GetLocalMusicLibrayFolders()
        {
            var myMusics = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            return myMusics.Folders.ToList();
        }

        public static async Task ClearAllLibraryCache()
        {
            StorageFolder folder = await StorageManager.GetApplicationDataFolder("Cache");
            SQLiteManager.MusicDataBasesHelper.ClearTableData(folder.Path + "\\MusicCache.db", "LocalMusic");
            SQLiteManager.MusicDataBasesHelper.ClearTableData(folder.Path + "\\MusicCache.db", "ExternalLocalMusic");
            Library.Artists.Clear();
            Library.Albums.Clear();
            Library.MusicFiles.Clear();
            Library.TempMusicFiles.Clear();
            Library.MusicCache.Clear();
            Library.Music.Clear();
            Library.RemovableDevices.Clear();
            
        }

        


        //Artist
        public static async Task RefreshArtistsData()
        {
            await Task.Run(() =>
            {
                Library.Artists.Clear();
                for (int i = 0; i < Library.Music.Count; i++)
                {
                    ArtistManager.AddMusicToArtist(Library.Music[i]);
                }
            });
        }

        //Album
        public static async Task RefreshAlbumsData()
        {
            await Task.Run(() =>
            {
                Library.Albums.Clear();
                for (int i = 0; i < Library.Music.Count; i++)
                {
                    AlbumManager.AddMusicToAlbum(Library.Music[i]);
                }
                Library.Albums = Library.Albums.OrderBy(x => x.Name).ToList();
            });
        }

        //Playlist
        public static async Task RefreshPlaylistsData()
        {
            Library.Playlists.Clear();
            StorageFolder folder = await StorageManager.GetApplicationDataFolder("Playlists");
            var itemsList = await folder.GetItemsAsync();
            foreach (var item in itemsList)
            {
                if (item.IsOfType(StorageItemTypes.File))
                {
                    Library.Playlists.Add(await PlaylistManager.ReadPlaylistFromStorageFileAsync(item as StorageFile));
                }
            }
        }

        //DataCodeImage
        public static async Task RefreshImagesDataAsync()
        {
            for (int i = 0; i < Library.Music.Count; i++)
            {
                DataCodeImage image = new DataCodeImage();
                if (Library.Music[i].MusicType == MusicType.Local || Library.Music[i].MusicType == MusicType.ExternalLocal)
                     await ImageManager.GetLocalMusicCoverForLibrary(Library.Music[i]);
            }
        }

        //RemovableDevices
        public static async Task RefreshRemovableDevices()
        {
            Library.RemovableDevices.Clear();
            List<StorageFolder> folders = await StorageManager.GetRemovableDevicesAsync();
            foreach (StorageFolder folder in folders)
            {
                Library.RemovableDevices.Add(RemovableDeviceManager.GetRemovableDevice(folder));
            }
        }
    }
}

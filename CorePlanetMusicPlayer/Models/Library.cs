using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static List <DataCodeImage> MusicCovers { get; set; } = new List<DataCodeImage> { };
        public static List<Playlist> Playlists { get; set; } = new List<Playlist>();
    }

    public class LibraryManager
    {
        public static async Task InitAllDataAsync()
        {
            await GetMusicCacheData();
            await GetMusicFilesDataAsync();

            RefreshImagesData();
            RefreshArtistsData();
            RefreshAlbumsData();
        }

        public static async Task GetMusicCacheData()
        {
            Library.MusicCache.Clear();
            StorageFolder folder = await StorageManager.GetApplicationDataFolder("Cache");
            await SQLiteManager.MusicDataBasesHelper.CreateTableAsync(folder,"MusicCache","LocalMusic");
            Library.MusicCache = SQLiteManager.MusicDataBasesHelper.GetTableData(ApplicationData.Current.LocalFolder.Path + "\\Cache\\MusicCache.db","LocalMusic");
            Debug.WriteLine(Library.MusicCache.Count);
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
                    else
                    {
                        //MusicFileCount ++;
                        string fileName = item.Name;
                        //Debug.WriteLine(fileName+"|||"+fileName.Substring(fileName.LastIndexOf(".")));
                        string fileSuffix = fileName.Substring(fileName.LastIndexOf("."));
                        if (fileSuffix == ".mp3" || fileSuffix == ".flac" || fileSuffix == ".wma" || fileSuffix == ".m4a" || fileSuffix == ".ac3" || fileSuffix == ".aac")
                        {
                            StorageFile storageFile = item as StorageFile;
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
            }
        }

        public static async Task GetPlaylistData()
        {
            Library.MusicCache.Clear();
            StorageFolder folder = await StorageManager.GetApplicationDataFolder("DataBases");
            //Library.MusicCache = SQLiteManager.MusicDataBasesHelper.GetData(folder.Path+"\\Playlists.db");

            Debug.WriteLine(Library.MusicCache.Count);
        }

        public static StorageFile GetLocalMusicFile(Music music)
        {
            if(music.MusicType!=MusicType.Local)
                return null;
            return Library.MusicFiles.Find(x=>x.Path == music.DataCode);
        }

        public static async Task<List<StorageFolder>> GetLocalMusicLibrayFolders()
        {
            var myMusics = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            return myMusics.Folders.ToList();
        }

        


        //Artist
        public static void RefreshArtistsData()
        {
            Library.Artists.Clear();
            for (int i = 0; i < Library.Music.Count; i++)
            {
                ArtistManager.AddMusicToArtist(Library.Music[i]);
            }
        }

        //Album
        public static void RefreshAlbumsData()
        {
            Library.Albums.Clear();
            for (int i = 0; i < Library.Music.Count; i++)
            {
                AlbumManager.AddMusicToAlbum(Library.Music[i]);
            }
        }

        //Playlist
        public static async void RefreshPlaylistsData()
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
        public static void RefreshImagesData()
        {
            for (int i = 0; i < Library.Music.Count; i++)
            {
                DataCodeImage image = new DataCodeImage();
                if (Library.Music[i].MusicType == MusicType.Local)
                    ImageManager.GetLocalMusicCoverForLibrary(Library.Music[i]);
            }
        }
    }
}

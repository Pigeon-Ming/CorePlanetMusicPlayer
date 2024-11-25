using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public class Library
    {
        public class Music
        {
            //public List<Music>AllMusic { get; set; } = new List<Music>();
            public static List<LocalMusic> LocalMusic { get; set; } = new List<LocalMusic>();
            public static List<ExternalMusic> ExternalMusic { get; set; } = new List<ExternalMusic>();
            public static List<OnlineMusic> OnlineMusic { get; set; } = new List<OnlineMusic>();
            public static List<LocalMusic> ClickToPlayMusic { get; set; } = new List<LocalMusic>();
        }

        public static List<Album> Albums { get; set; } = new List<Album> ();
        public static List<Artist> Artists { get; set;} = new List<Artist> ();
        public static List<Playlist> PlayLists { get; set; } = new List<Playlist> ();

        public class Image
        {
            public static List<CorePlanetMusicPlayer.Models.Image> LocalMusicCover { get; set; } = new List<CorePlanetMusicPlayer.Models.Image> ();
            public static List<CorePlanetMusicPlayer.Models.Image> ExternalMusicCover { get; set; } = new List<CorePlanetMusicPlayer.Models.Image>();
            //public static List<CorePlanetMusicPlayer.Models.Image> OnlineMusicCover { get; set; } = new List<CorePlanetMusicPlayer.Models.Image>();
            //public static List<CorePlanetMusicPlayer.Models.Image> Artist { get; set; } = new List<CorePlanetMusicPlayer.Models.Image>();
            //public static List<CorePlanetMusicPlayer.Models.Image> Playlist { get; set; } = new List<CorePlanetMusicPlayer.Models.Image>();

        }
    }

    public class LibraryManager
    {

        public static async Task RefreshAllLibraryData()
        {
            await RefreshLocalMusicData();
            await RefreshExternalMusicData();
            await RefreshOnlineMusicData();

            RefreshArtistsData();
            RefreshAlbumsData();
            RefreshPlaylistsData();

            RefreshImagesData();
        }

        //LocalMusic
        //public static int LocalMusicFileCount { get; set; } = 0;
        public static async Task RefreshLocalMusicData()
        {
            Library.Music.LocalMusic.Clear();
            StorageFolder folder = KnownFolders.MusicLibrary;
            
            Queue<StorageFolder> folderQueue = new Queue<StorageFolder>();
            folderQueue.Enqueue(folder);
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
                        //LocalMusicFileCount ++;
                        string fileName = item.Name;
                        //Debug.WriteLine(fileName+"|||"+fileName.Substring(fileName.LastIndexOf(".")));
                        string fileSuffix = fileName.Substring(fileName.LastIndexOf("."));
                        if (fileSuffix == ".mp3" || fileSuffix == ".flac" || fileSuffix == ".wma" || fileSuffix == ".m4a" || fileSuffix == ".ac3" || fileSuffix == ".aac")
                        {
                            StorageFile storageFile = item as StorageFile;
                            LocalMusic localMusic = new LocalMusic();
                            localMusic.StorageFile = storageFile;
                            localMusic.Title = item.Name;
                            localMusic.Album = "未知专辑";
                            localMusic.Artist = "未知艺术家";
                            localMusic.DataCode = storageFile.Path;
                            Library.Music.LocalMusic.Add(localMusic);
                        }
                    }
                }
            } while (folderQueue.Count > 0);
            //Library.Music.LocalMusic = Library.Music.LocalMusic.Concat(Library.Music.ClickToPlayMusic).ToList();
            GetLocalMusicPropertiesAsync();
            
        }

        public static async Task GetLocalMusicPropertiesAsync()
        {
            if(await MusicManager.ReadLocalMusicPropertiesCacheAsync())
            {
                //Debug.WriteLine("通过json数据读取音乐信息");
                return;
            }
            for (int i = 0; i < Library.Music.LocalMusic.Count; i++)
            {
                await MusicManager.GetLocalMusicPropertiesAsync(Library.Music.LocalMusic[i]);
                //await MusicManager.GetLocalMusicPropertiesAsync_System(Library.Music.LocalMusic[i]);
            }
            await MusicManager.UpdateLocalMusicPropertiesCacheAsync();
        }

        public static async Task<List<StorageFolder>> GetLocalMusicLibrayFolders()
        {
            var myMusics = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            return myMusics.Folders.ToList();
        }

        //OnlineMusic
        static int OnlineMusicMaxDataCode { get; set; } = 0;
        public static async Task RefreshOnlineMusicData()
        {
            string jsonStr = await StorageHelper.ReadFile(ApplicationData.Current.LocalFolder, "OnlineMusic.json");
            JsonArray jsonArray = JsonHelper.StringToJsonArray(jsonStr);
            if (jsonArray == null) return;
            Library.Music.ExternalMusic.Clear();
            for (int i = 0; i < jsonArray.Count; i++)
            {
                JsonObject jsonObject = jsonArray.GetObjectAt((uint)i);
                Music music = JsonHelper.JsonObjectToMusic(jsonObject);
                OnlineMusic onlineMusic = MusicManager.MusicToOnlineMusic(music);
                
                IJsonValue jsonValue;
                if (jsonObject.TryGetValue("title", out jsonValue))
                    onlineMusic.Title = jsonObject.GetNamedString("title");
                if (jsonObject.TryGetValue("url", out jsonValue))
                {
                    onlineMusic.URL = jsonObject.GetNamedString("url");
                    onlineMusic.DataCode = onlineMusic.URL;
                }
                    
                Library.Music.OnlineMusic.Add(onlineMusic);
            }
        }

        public static void AddOnlineMusic(OnlineMusic onlineMusic)
        {
            if (String.IsNullOrEmpty(onlineMusic.Artist))
                onlineMusic.Artist = "未知艺术家";
            if (String.IsNullOrEmpty(onlineMusic.Album))
                onlineMusic.Artist = "未知专辑";
            Library.Music.OnlineMusic.Add(onlineMusic);
            onlineMusic.DataCode = onlineMusic.URL.ToString();
            SaveOnlineMusicData();
        }

        public static async void SaveOnlineMusicData()
        {
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < Library.Music.OnlineMusic.Count; i++) 
            { 
                JsonObject jsonObject = JsonHelper.MusicToJsonObject(Library.Music.OnlineMusic[i]);
                jsonObject.Add("url", JsonValue.CreateStringValue(Library.Music.OnlineMusic[i].URL));
                jsonArray.Add(jsonObject);
            }
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            await StorageHelper.WriteFile(storageFolder, "OnlineMusic.json", jsonArray.Stringify());
        }



        //ExternalMusic
        static int ExternalMusicMaxDataCode { get; set; } = 0;
        public static async Task RefreshExternalMusicData()
        {
            string jsonStr = await StorageHelper.ReadFile(ApplicationData.Current.LocalFolder, "ExternalMusic.json");
            JsonArray jsonArray = JsonHelper.StringToJsonArray(jsonStr);
            if (jsonArray == null) return;
            Library.Music.ExternalMusic.Clear();
            for(int i = 0; i < jsonArray.Count; i++)
            {
                JsonObject jsonObject = jsonArray.GetObjectAt((uint)i);
                Music music = JsonHelper.JsonObjectToMusic(jsonObject);
                ExternalMusic externalMusic = MusicManager.MusicToExternalMusic(music);
                
                IJsonValue jsonValue;
                if(jsonObject.TryGetValue("key",out jsonValue))
                {
                    externalMusic.Key = jsonObject.GetNamedString("key");
                    externalMusic.DataCode = externalMusic.Key;
                }
                //if (jsonObject.TryGetValue("title", out jsonValue))
                //    externalMusic.Title = jsonObject.GetNamedString("title");
                //if (jsonObject.TryGetValue("artist", out jsonValue))
                //    externalMusic.Artist = jsonValue.GetString();
                //if (jsonObject.TryGetValue("album", out jsonValue))
                //    externalMusic.Album = jsonValue.GetString();
                //if (jsonObject.TryGetValue("trackNumber", out jsonValue))
                //    externalMusic.TrackNumber = Convert.ToUInt32(jsonValue.GetNumber());
                //if (jsonObject.TryGetValue("discNumber", out jsonValue))
                //    externalMusic.DiscNumber = Convert.ToUInt32(jsonValue.GetNumber());
                //if (jsonObject.TryGetValue("year", out jsonValue))
                //    externalMusic.Year = Convert.ToUInt32(jsonValue.GetNumber());
                Library.Music.ExternalMusic.Add(externalMusic);
            }
            await GetExternalMusicPropertiesAsync();
        }

        public static async Task<bool> AddExternalMusicAsync()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".flac");
            picker.FileTypeFilter.Add(".wma");
            picker.FileTypeFilter.Add(".m4a");
            picker.FileTypeFilter.Add(".ac3");
            picker.FileTypeFilter.Add(".aac");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return false;
            ExternalMusic externalMusic = new ExternalMusic();
            externalMusic.Key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
            externalMusic.MusicType = MusicType.External;
            externalMusic.Title = file.DisplayName;
            LibraryManager.AddExternalMusicAsync(externalMusic);
            return true;
        }

        public static void AddExternalMusicFromLocalMusic(LocalMusic localMusic)
        {
            ExternalMusic externalMusic = new ExternalMusic();
            externalMusic.Key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(localMusic.StorageFile);
            externalMusic.DataCode = externalMusic.Key;
            externalMusic.MusicType = MusicType.External;
            externalMusic.Title = localMusic.Title;
            externalMusic.Artist = localMusic.Artist;
            externalMusic.Album = localMusic.Album;
            externalMusic.DiscNumber = localMusic.DiscNumber;
            externalMusic.TrackNumber = localMusic.TrackNumber;
            externalMusic.Year = localMusic.Year;
            LibraryManager.AddExternalMusicAsync(externalMusic);
        }
        public static async Task AddExternalMusicAsync(ExternalMusic externalMusic)
        {
            externalMusic.DataCode = externalMusic.Key;
            Library.Music.ExternalMusic.Add(externalMusic);
            await MusicManager.GetExternalMusicPropertiesAsync(externalMusic);
            SaveExternalMusicData();
        }

        public static async void SaveExternalMusicData()
        {
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < Library.Music.ExternalMusic.Count; i++)
            {
                if (String.IsNullOrEmpty(Library.Music.ExternalMusic[i].Key))
                    continue;
                JsonObject jsonObject = JsonHelper.MusicToJsonObject(Library.Music.ExternalMusic[i]);
                jsonObject.Add("key", JsonValue.CreateStringValue(Library.Music.ExternalMusic[i].Key));
                //jsonObject.Add("title", JsonValue.CreateStringValue(Library.Music.ExternalMusic[i].Title));
                //jsonObject.Add("artist", JsonValue.CreateStringValue(Library.Music.ExternalMusic[i].Artist));
                //jsonObject.Add("album", JsonValue.CreateStringValue(Library.Music.ExternalMusic[i].Album));
                //jsonObject.Add("trackNumber", JsonValue.CreateNumberValue(Library.Music.ExternalMusic[i].TrackNumber));
                //jsonObject.Add("discNumber", JsonValue.CreateNumberValue(Library.Music.ExternalMusic[i].DiscNumber));
                //jsonObject.Add("year", JsonValue.CreateNumberValue(Library.Music.ExternalMusic[i].Year));
                jsonArray.Add(jsonObject);
            }
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            await StorageHelper.WriteFile(storageFolder, "ExternalMusic.json", jsonArray.Stringify());
        }

        public static async Task GetExternalMusicPropertiesAsync()
        {
            if (await MusicManager.ReadExternalMusicPropertiesCacheAsync())
            {
                //Debug.WriteLine("通过json数据读取音乐信息");
                return;
            }
            for (int i = 0; i < Library.Music.LocalMusic.Count; i++)
            {
                await MusicManager.GetExternalMusicPropertiesAsync(Library.Music.ExternalMusic[i]);
            }
            await MusicManager.UpdateExternalMusicPropertiesCacheAsync();
        }

        //Artist
        public static void RefreshArtistsData()
        {
            Library.Artists.Clear();
            for (int i = 0; i < Library.Music.LocalMusic.Count; i++)
            {
                ArtistManager.AddMusicToArtist(Library.Music.LocalMusic[i]);
            }
            for (int i = 0; i < Library.Music.ExternalMusic.Count; i++)
            {
                ArtistManager.AddMusicToArtist(Library.Music.ExternalMusic[i]);
            }
            for (int i = 0; i < Library.Music.OnlineMusic.Count; i++)
            {
                ArtistManager.AddMusicToArtist(Library.Music.OnlineMusic[i]);
            }
        }
        //Album
        public static void RefreshAlbumsData()
        {
            Library.Albums.Clear();
            for (int i = 0; i < Library.Music.LocalMusic.Count; i++)
            {
                AlbumManager.AddMusicToAlbum(Library.Music.LocalMusic[i]);
            }
            for (int i = 0; i < Library.Music.ExternalMusic.Count; i++)
            {
                AlbumManager.AddMusicToAlbum(Library.Music.ExternalMusic[i]);
            }
            for (int i = 0; i < Library.Music.OnlineMusic.Count; i++)
            {
                AlbumManager.AddMusicToAlbum(Library.Music.OnlineMusic[i]);
            }
        }

        //Playlist
        public static async void RefreshPlaylistsData()
        {
            Library.PlayLists.Clear();
            StorageFolder folder = await StorageHelper.GetApplicationDataFolder("Playlists");
            var itemsList = await folder.GetItemsAsync();
            foreach (var item in itemsList) 
            { 
                if (item.IsOfType(StorageItemTypes.File))
                {
                    Library.PlayLists.Add(await PlaylistManager.ReadPlaylistFromStorageFileAsync(item as StorageFile));
                }
            }
        }


        //image
        public static void RefreshImagesData()
        {
            RefreshLocalMusicImagesData();
        }

        public static void RefreshLocalMusicImagesData()
        {
            for(int i = 0; i < Library.Music.LocalMusic.Count; i++)
            {
                Image image = new Image();
                LocalMusic music = Library.Music.LocalMusic[i];
                ImageManager.GetLocalMusicCover_Library(music);
            }
        }
    }
}

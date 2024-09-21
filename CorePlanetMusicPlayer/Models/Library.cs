using System;
using System.Collections.Generic;
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
        }

        public static List<Album> Albums { get; set; } = new List<Album> ();
        public static List<Artist> Artists { get; set;} = new List<Artist> ();
        public static List<Playlist> PlayLists { get; set; } = new List<Playlist> ();

        public class Image
        {
            public static List<CorePlanetMusicPlayer.Models.Image> LocalMusicCover { get; set; } = new List<CorePlanetMusicPlayer.Models.Image> ();
            public static List<CorePlanetMusicPlayer.Models.Image> ExternalMusicCover { get; set; } = new List<CorePlanetMusicPlayer.Models.Image>();
            public static List<CorePlanetMusicPlayer.Models.Image> OnlineMusicCover { get; set; } = new List<CorePlanetMusicPlayer.Models.Image>();
            public static List<CorePlanetMusicPlayer.Models.Image> Artist { get; set; } = new List<CorePlanetMusicPlayer.Models.Image>();
            public static List<CorePlanetMusicPlayer.Models.Image> Playlist { get; set; } = new List<CorePlanetMusicPlayer.Models.Image>();

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
        }

        //LocalMusic
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

                        string fileName = item.Name;
                        //Debug.WriteLine(fileName+"|||"+fileName.Substring(fileName.LastIndexOf(".")));
                        string fileSuffix = fileName.Substring(fileName.LastIndexOf("."));
                        if (fileSuffix == ".mp3" || fileSuffix == ".flac" || fileSuffix == ".wma" || fileSuffix == ".m4a" || fileSuffix == ".ac3" || fileSuffix == ".aac")
                        {
                            StorageFile storageFile = item as StorageFile;
                            LocalMusic localMusic = new LocalMusic();
                            localMusic.StorageFile = storageFile;
                            localMusic.Title = item.Name;
                            localMusic.DataCode = storageFile.Path;
                            Library.Music.LocalMusic.Add(localMusic);
                        }
                    }
                }
            } while (folderQueue.Count > 0);
            for(int i = 0; i < Library.Music.LocalMusic.Count; i++)
            {
                await MusicManager.GetLocalMusicPropertiesAsync(Library.Music.LocalMusic[i]);
            }
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
            Library.Music.OnlineMusic.Add(onlineMusic);
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
                    
                
                Library.Music.ExternalMusic.Add(externalMusic);
            }
        }

        public static void AddExternalMusic(ExternalMusic externalMusic)
        {
            externalMusic.DataCode = externalMusic.Key;
            Library.Music.ExternalMusic.Add(externalMusic);
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
                jsonArray.Add(jsonObject);
            }
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            await StorageHelper.WriteFile(storageFolder, "ExternalMusic.json", jsonArray.Stringify());
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
    }
}

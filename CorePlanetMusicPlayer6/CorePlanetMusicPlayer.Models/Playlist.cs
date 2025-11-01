using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public enum PlaylistCoverType {Default, Local, Stream}

    public class Playlist
    {
        public Playlist()
        {
            CreateTime = DateTime.Now;
        }

        public string Title { get; set; }

        public string Description { get; set; } = "";

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public PlaylistCoverType CoverType { get; set; }

        public string CoverUrl { get; set; }

        public string CoverPath { get; set; }

        public string UpdateURL { get; set; }

        public ObservableCollection<IMusic> Music { get; set; } = new ObservableCollection<IMusic>();

        public async Task SetToDefaultCoverAsync()
        {
            CoverType = PlaylistCoverType.Default;
            await PlaylistManager.SavePlaylistAsync(this);
        }

        public async Task SetLocalCoverAsync(StorageFile storageFile)
        {
            CoverType = PlaylistCoverType.Local;
            StorageFile newFile = await StorageHelper.GetStorageFileFromStorageFolderAsync(PlaylistManager.CoverFolder, $"{Title}{storageFile.FileType}");
            await storageFile.CopyAndReplaceAsync(newFile);
            CoverPath = newFile.Path;
            await PlaylistManager.SavePlaylistAsync(this);
        }

        public async Task SetSteamCoverAsync(string newCoverUrl)
        {
            CoverType = PlaylistCoverType.Stream;
            CoverUrl = newCoverUrl;
            await PlaylistManager.SavePlaylistAsync(this);
        }
    }

    public class PlaylistManager
    {
        private static List<LocalMusic> LocalMusicList { get; set; }

        public static StorageFolder PlaylistsFolder { get; private set; }

        public static StorageFolder CoverFolder { get; private set; }

        public static ObservableCollection<Playlist> Playlists { get; set; } = new ObservableCollection<Playlist>();

        public static event EventHandler PlaylistChanged;

        public static async Task InitAsync(List<LocalMusic> localMusicList)
        {
            StorageFolder storageFolder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            PlaylistsFolder = await StorageHelper.GetStorageFolderFromStorageFolderAsync(storageFolder, "Playlists");
            CoverFolder = await StorageHelper.GetStorageFolderFromStorageFolderAsync(storageFolder, "PlaylistCovers");
            SetLocalMusicList(localMusicList);
        }

        public static async Task GetPlaylistsAsync()
        {
            Playlists.Clear();
            List<StorageFile> files = (await PlaylistsFolder.GetFilesAsync()).ToList();
            foreach (StorageFile file in files)
            {
                Playlist playlist = await ReadPlaylistFileAsync(file);
                Playlists.Add(playlist);
            }
        }

        public static void SetLocalMusicList(List<LocalMusic> localMusicList)
        {
            LocalMusicList = localMusicList;
        }

        public static async Task<Playlist> ReadPlaylistFileAsync(StorageFile storageFile)
        {
            Playlist playlist = new Playlist();
            string fileContent = await StorageHelper.ReadFileAsStringAsync(storageFile);
            JObject jObject = JObject.Parse(fileContent);
            playlist.Title = (string)jObject["Title"];

            string description = (string)jObject["Description"];

            playlist.Description = description == null ? "" : description;
            playlist.CreateTime = DateTime.Parse((string)jObject["CreateTime"]);
            playlist.UpdateTime = DateTime.Parse((string)jObject["UpdateTime"]);

            string coverType = ((string)jObject["CoverType"]);
            playlist.CoverType = coverType == "Stream" ? PlaylistCoverType.Stream : coverType == "Local"? PlaylistCoverType.Local : PlaylistCoverType.Default;
            if (playlist.CoverType == PlaylistCoverType.Stream)
            {
                if (jObject.ContainsKey("CoverUrl"))
                {
                    playlist.CoverUrl = ((string)jObject["CoverUrl"]);
                }
            }
            else
            {
                if (jObject.ContainsKey("CoverPath"))
                {
                    playlist.CoverPath = ((string)jObject["CoverPath"]);
                }
            }

            if (jObject.ContainsKey("UpdateURL"))
            {
                playlist.UpdateURL = (string)jObject["UpdateURL"];
            }

            playlist.Music = new ObservableCollection<IMusic>(JMusicHelper.GetMusicFromJArray((JArray)jObject["Music"],LocalMusicList));

            return playlist;
        }

        //private static List<IMusic> GetPlaylistMusicFromJArray(JArray jArray)
        //{
        //    List<LocalMusic> localMusicList = LocalMusicList;

        //    List<IMusic> music = new List<IMusic>();

        //    foreach (JObject jObject in jArray)
        //    {
        //        int type = Convert.ToInt32(jObject["Type"]);
        //        switch (type)
        //        {
        //            case 0:
        //                LocalMusic localMusic = localMusicList.Find(x => x.StorageFile.Path.Equals((string)jObject["Key"]));
        //                if (localMusic != null)
        //                    music.Add(localMusic);
        //                break;
        //            case 1:
        //                StreamMusic streamMusic = new StreamMusic();
        //                streamMusic.Url = (string)jObject["Key"];
        //                streamMusic.Title = (string)jObject["Title"];
        //                streamMusic.Artist = (string)jObject["Artist"];
        //                streamMusic.Album = (string)jObject["Album"];
        //                streamMusic.CoverUrl = (string)jObject["CoverUrl"];
        //                streamMusic.Genre = Convert.ToUInt32((string)jObject["Genre"]);
        //                streamMusic.Year = Convert.ToUInt32((string)jObject["Year"]);
        //                music.Add(streamMusic);
        //                break;
        //            case 2:
        //                RemovableMusic removableMusic = new RemovableMusic();
        //                //To-Do:RemovableMusic
        //                break;
        //            default:
        //                music.Add(new Music { Title = jObject["Key"].ToString() });
        //                break;
        //        }
        //    }
        //    return music;
        //}

        public static async Task SavePlaylistAsync(Playlist playlist)
        {
            JObject jObject = new JObject
            {
                { "Title", playlist.Title },
                { "Description", playlist.Description },
                { "CreateTime", playlist.CreateTime },
                { "UpdateTime", DateTime.Now },
                { "CoverType", playlist.CoverType == PlaylistCoverType.Stream ? "Stream" : playlist.CoverType == PlaylistCoverType.Local? "Local" : "Default" }
            };
            if (playlist.CoverType == PlaylistCoverType.Stream)
            {
                jObject.Add("CoverUrl", playlist.CoverUrl);
            }
            else if(playlist.CoverType == PlaylistCoverType.Local)
            {
                jObject.Add("CoverPath", playlist.CoverPath);
            }
            
            JArray jArray = new JArray();
            foreach(IMusic music in playlist.Music)
            {
                JMusic jMusic = JMusicHelper.GetJMusic(music);
                if(jMusic != null)
                    jArray.Add(JObject.FromObject(jMusic));
            }
            jObject.Add("Music",jArray);
            if(!await StorageHelper.IsFileExistAsync(PlaylistsFolder,playlist.Title + ".pmplist6"))
            {
                Playlists.Add(playlist);
            }
            else
            {
                PlaylistChanged?.Invoke(playlist, null);
            }
            StorageFile storageFile = await StorageHelper.GetStorageFileFromStorageFolderAsync(PlaylistsFolder, playlist.Title + ".pmplist6");
            await StorageHelper.WriteStringToFileAsync(storageFile, jObject.ToString());
        }

        public static async Task RemoveMusicFromPlaylistAsync(Playlist playlist, IMusic music)
        {
            playlist.Music.Remove(music);
            await SavePlaylistAsync(playlist);
        }

        public static async Task DeletePlaylistAsync(Playlist playlist)
        {
            Playlists.Remove(playlist);
            if(await StorageHelper.IsFileExistAsync(PlaylistsFolder, playlist.Title + ".pmplist6"))
            {
                StorageFile file = await PlaylistsFolder.GetFileAsync(playlist.Title + ".pmplist6");
                await file.DeleteAsync();
            }
        }
    }
}

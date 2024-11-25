using CorePlanetMusicPlayer.Models.TagLibHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TagLib;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace CorePlanetMusicPlayer.Models
{
    public enum MusicType
    {
        Local, External, Online,
    }


    public class Music
    {
        public string DataCode { get; set; } = "";//类似于Token
        public MusicType MusicType { get; set; } = MusicType.Local;
        public String Title { get; set; } = "";
        public String Artist { get; set; } = "";
        public String Album { get; set; } = "";//专辑名称
        public uint Bitrate { get; set; }//比特率
        public uint Year { get; set; }//年份
        public uint TrackNumber { get; set; }//专辑内音乐编号
        public uint DiscNumber { get; set; }//专辑内音乐编号
        public String Duration { get; set; } = "";//长度
    }

    public class MusicManager
    {
        public static object FindMusic(Music music)
        {
            switch (music.MusicType)
            {
                case MusicType.Local:
                    if(music.DataCode.IndexOf("pmptemp-")==-1)
                        return Library.Music.LocalMusic.Find(x => x.DataCode == music.DataCode);
                    else
                        return Library.Music.ClickToPlayMusic.Find(x => x.DataCode == music.DataCode);
                case MusicType.External:
                    return Library.Music.ExternalMusic.Find(x => x.DataCode == music.DataCode);
                case MusicType.Online:
                    return Library.Music.OnlineMusic.Find(x => x.DataCode == music.DataCode);
            }

            return null;
        }

        public static object FindMusic(string DataCode,MusicType musicType)
        {
            switch (musicType)
            {
                case MusicType.Local:
                    if (DataCode.IndexOf("pmptemp-") == -1)
                        return Library.Music.LocalMusic.Find(x => x.DataCode == DataCode);
                    else
                        return Library.Music.ClickToPlayMusic.Find(x => x.DataCode == DataCode);
                case MusicType.External:
                    return Library.Music.ExternalMusic.Find(x => x.DataCode == DataCode);
                case MusicType.Online:
                    return Library.Music.OnlineMusic.Find(x => x.DataCode == DataCode);
            }

            return null;
        }

        static List<string> ExternalProperties { get; set; } = new List<string> { "System.Music.DiscNumber" };

        public static async Task<LocalMusic> GetLocalMusicPropertiesAsync_Fast(LocalMusic music)
        {
            if (music == null || music.StorageFile == null)
                return music;
            StorageFile file = music.StorageFile;
            StorageItemContentProperties storageItemContentProperties = file.Properties;
            MusicProperties musicProperties = await storageItemContentProperties.GetMusicPropertiesAsync(); // 音频属性
            if (!string.IsNullOrEmpty(musicProperties.Title))
                music.Title = musicProperties.Title;
            else
                music.Title = music.StorageFile.Name;
            if (!string.IsNullOrEmpty(musicProperties.Album))
                music.Album = musicProperties.Album;
            else
                music.Album = "未知专辑";

            if (!string.IsNullOrEmpty(musicProperties.Artist))
                music.Artist = musicProperties.Artist;
            else
                music.Artist = "未知艺术家";
            music.Year = musicProperties.Year;
            music.Bitrate = musicProperties.Bitrate;
            music.Duration = musicProperties.Duration.ToString().Substring(3, 5);
            music.TrackNumber = musicProperties.TrackNumber;
            //IDictionary<string, object> extraProperties = await storageItemContentProperties.RetrievePropertiesAsync(ExternalProperties);
            //Debug.WriteLine("获取高级信息");
            //if (extraProperties["System.Music.DiscNumber"] != null)
            //{
            //    Debug.WriteLine("-----"+extraProperties["System.Music.DiscNumber"]);
            //}
            return music;
        }

        public static async Task<LocalMusic> GetLocalMusicPropertiesAsync(LocalMusic music)
        {

            //To-Do:从SQLite中读取
            StorageFile file = music.StorageFile;
            StorageItemContentProperties storageItemContentProperties = file.Properties;
            MusicProperties musicProperties = await storageItemContentProperties.GetMusicPropertiesAsync(); // 音频属性
            if (!string.IsNullOrEmpty(musicProperties.Title))
                music.Title = musicProperties.Title;
            else
                music.Title = music.StorageFile.Name;
            if (!string.IsNullOrEmpty(musicProperties.Album))
                music.Album = musicProperties.Album;
            else
                music.Album = "未知专辑";

            if (!string.IsNullOrEmpty(musicProperties.Artist))
                music.Artist = musicProperties.Artist;
            else
                music.Artist = "未知艺术家";
            music.Year = musicProperties.Year;
            music.Bitrate = musicProperties.Bitrate;
            music.Duration = musicProperties.Duration.ToString().Substring(3, 5);
            music.TrackNumber = musicProperties.TrackNumber;

            if (music.StorageFile.FileType == ".ac3" || music.StorageFile.FileType == ".m4a")
            {
                return music;
            }
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(music.StorageFile);
            TagLib.File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
            TagLib.File _file;
            try
            {
                _file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
                music.DiscNumber = _file.Tag.Disc;
            }
            catch
            {

            }

            return music;
        }

        public static async Task<ExternalMusic> GetExternalMusicPropertiesAsync(ExternalMusic music)
        {

            //To-Do:从SQLite中读取
            StorageFile file = await MusicManager.GetExternalMusicByExternalMusicKeyAsync(music.Key);
            if (file == null) return null;
            StorageItemContentProperties storageItemContentProperties = file.Properties;
            MusicProperties musicProperties = await storageItemContentProperties.GetMusicPropertiesAsync(); // 音频属性
            if (!string.IsNullOrEmpty(musicProperties.Title))
                music.Title = musicProperties.Title;
            else
                music.Title = file.Name;
            if (!string.IsNullOrEmpty(musicProperties.Album))
                music.Album = musicProperties.Album;
            else
                music.Album = "未知专辑";

            if (!string.IsNullOrEmpty(musicProperties.Artist))
                music.Artist = musicProperties.Artist;
            else
                music.Artist = "未知艺术家";
            music.Year = musicProperties.Year;
            music.Bitrate = musicProperties.Bitrate;
            music.Duration = musicProperties.Duration.ToString().Substring(3, 5);
            music.TrackNumber = musicProperties.TrackNumber;

            if (file.FileType == ".ac3" || file.FileType == ".m4a")
            {
                return music;
            }
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(file);
            TagLib.File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
            TagLib.File _file;
            try
            {
                _file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
                music.DiscNumber = _file.Tag.Disc;
            }
            catch
            {

            }

            return music;
        }

        public static ExternalMusic MusicToExternalMusic(Music music)
        {
            ExternalMusic externalMusic = new ExternalMusic();
            externalMusic.Title = music.Title;
            externalMusic.Artist = music.Artist;
            externalMusic.Album = music.Album;
            externalMusic.TrackNumber = music.TrackNumber;
            externalMusic.DiscNumber = music.DiscNumber;
            externalMusic.Duration = music.Duration;
            externalMusic.Year = music.Year;
            externalMusic.Bitrate = music.Bitrate;
            externalMusic.DataCode = music.DataCode;
            return externalMusic;
        }

        public static OnlineMusic MusicToOnlineMusic(Music music)
        {
            OnlineMusic OnlineMusic = new OnlineMusic();
            OnlineMusic.Title = music.Title;
            OnlineMusic.Artist = music.Artist;
            OnlineMusic.Album = music.Album;
            OnlineMusic.TrackNumber = music.TrackNumber;
            OnlineMusic.DiscNumber = music.DiscNumber;
            OnlineMusic.Duration = music.Duration;
            OnlineMusic.Year = music.Year;
            OnlineMusic.Bitrate = music.Bitrate;
            OnlineMusic.DataCode = music.DataCode;
            return OnlineMusic;
        }

        public static async Task<StorageFile> GetExternalMusicByExternalMusicKeyAsync(string Key)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(Key);
        }

        public static Music GetMusicFromJsonObject(JsonObject jsonObject)
        {
            Music music = new Music();
            music.Title = jsonObject.GetNamedString("title");
            music.Artist = jsonObject.GetNamedString("artist");
            music.Album = jsonObject.GetNamedString("album");
            music.TrackNumber = (uint)jsonObject.GetNamedNumber("trackNumber");
            music.DiscNumber = (uint)jsonObject.GetNamedNumber("discNumber");
            music.Duration = jsonObject.GetNamedString("duration");
            music.Year = (uint)jsonObject.GetNamedNumber("year");
            music.Bitrate = (uint)jsonObject.GetNamedNumber("bitrate");
            music.DataCode = jsonObject.GetNamedString("dataCode");
            switch (jsonObject.GetNamedString("musicType"))
            {
                case "Local":
                    music.MusicType = MusicType.Local;
                    break;
                case "Online":
                    music.MusicType = MusicType.Online;
                    break;
                case "External":
                    music.MusicType = MusicType.External;
                    break;
            }
            return music;
        }

        public static async Task UpdateLocalMusicPropertiesCacheAsync()
        {
            JsonArray jsonArray = new JsonArray();
            JsonObject jsonObject;
            for(int i=0;i<Library.Music.LocalMusic.Count;i++)
            {
                jsonArray.Add(JsonHelper.MusicToJsonObject(Library.Music.LocalMusic[i]));
            }
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            storageFolder = await StorageHelper.GetFolder(storageFolder,"Cache");
            await StorageHelper.WriteFile(storageFolder, "LocalMusicProperties.json", jsonArray.Stringify());
        }

        public static async Task<bool> ReadLocalMusicPropertiesCacheAsync()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            storageFolder = await StorageHelper.GetFolder(storageFolder, "Cache");
            string fileStr = await StorageHelper.ReadFile(storageFolder, "LocalMusicProperties.json");
            JsonArray jsonArray = new JsonArray();
            if (JsonArray.TryParse(fileStr, out jsonArray) == false)
                return false;
            for(int i=0;i<jsonArray.Count;i++)
            {
                JsonObject jsonObject = jsonArray[i].GetObject();
                LocalMusic localMusic = Library.Music.LocalMusic.Find(x=>x.DataCode == jsonObject.GetNamedString("dataCode"));
                if (localMusic == null)
                    continue;
                Music music = JsonHelper.JsonObjectToMusic(jsonObject);
                localMusic.Title = music.Title;
                localMusic.Artist = music.Artist;
                localMusic.Album = music.Album;
                localMusic.DiscNumber = music.DiscNumber;
                localMusic.TrackNumber = music.TrackNumber;
                localMusic.Year = music.Year;
                localMusic.Bitrate = music.Bitrate;
                localMusic.Duration = music.Duration;
            }
            return true;
        }

        public static async Task UpdateExternalMusicPropertiesCacheAsync()
        {
            JsonArray jsonArray = new JsonArray();
            JsonObject jsonObject;
            for (int i = 0; i < Library.Music.ExternalMusic.Count; i++)
            {
                jsonArray.Add(JsonHelper.MusicToJsonObject(Library.Music.ExternalMusic[i]));
            }
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            storageFolder = await StorageHelper.GetFolder(storageFolder, "Cache");
            await StorageHelper.WriteFile(storageFolder, "ExternalMusicProperties.json", jsonArray.Stringify());
        }

        public static async Task<bool> ReadExternalMusicPropertiesCacheAsync()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            storageFolder = await StorageHelper.GetFolder(storageFolder, "Cache");
            string fileStr = await StorageHelper.ReadFile(storageFolder, "ExternalMusicProperties.json");
            JsonArray jsonArray = new JsonArray();
            if (JsonArray.TryParse(fileStr, out jsonArray) == false)
                return false;
            for (int i = 0; i < jsonArray.Count; i++)
            {
                JsonObject jsonObject = jsonArray[i].GetObject();
                ExternalMusic externalMusic = Library.Music.ExternalMusic.Find(x => x.DataCode == jsonObject.GetNamedString("dataCode"));
                if (externalMusic == null)
                    continue;
                Music music = JsonHelper.JsonObjectToMusic(jsonObject);
                externalMusic.Title = music.Title;
                externalMusic.Artist = music.Artist;
                externalMusic.Album = music.Album;
                externalMusic.DiscNumber = music.DiscNumber;
                externalMusic.TrackNumber = music.TrackNumber;
                externalMusic.Year = music.Year;
                externalMusic.Bitrate = music.Bitrate;
                externalMusic.Duration = music.Duration;
            }
            return true;
        }
    }

    public class LocalMusic:Music
    {
        public LocalMusic() 
        { 
            MusicType = MusicType.Local;
        }
        public StorageFile StorageFile { get; set; }
    }

    

    public class ExternalMusic:Music
    {
        public ExternalMusic()
        {
            MusicType = MusicType.External;
        }
        public bool Temp { get; set; } = false;//不在库中
        public String Key { get; set; }
    }

    public class OnlineMusic:Music
    {
        public OnlineMusic()
        {
            MusicType = MusicType.Online;
        }
        public bool Temp { get; set; } = false;//不在库中
        public String URL { get; set; } = "";
        public String CoverURL { get; set; } = "";
    }

}

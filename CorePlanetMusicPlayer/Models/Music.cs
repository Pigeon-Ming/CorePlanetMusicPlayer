using CorePlanetMusicPlayer.Models.TagLibHelper;
using System;
using System.Collections.Generic;
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
        public MusicType MusicType { get; set; }
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
                    return Library.Music.LocalMusic.Find(x => x.DataCode == music.DataCode);
                case MusicType.External:
                    return Library.Music.ExternalMusic.Find(x => x.DataCode == music.DataCode);
                case MusicType.Online:
                    return Library.Music.OnlineMusic.Find(x => x.DataCode == music.DataCode);
            }

            return null;
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
        public String URL { get; set; }
    }

}

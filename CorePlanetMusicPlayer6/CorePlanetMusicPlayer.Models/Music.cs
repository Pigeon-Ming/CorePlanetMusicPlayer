using CorePlanetMusicPlayer.Models.TagLibModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Models
{
    public enum MusicType { Local, Stream };
    public class MusicHelper
    {
        public static List<string> SupportedMusicFileTypesString = new List<string> { ".mp3", ".flac" };

        public static TimeSpan GetTotalDuration(List<IMusic> musicList)
        {
            TimeSpan totalDuration = TimeSpan.Zero;
            foreach (IMusic music in musicList)
            {
                totalDuration = totalDuration.Add(StringHelper.ConvertMinuteAndSecondTimeToTimeSpan(music.Duration));
            }
            return totalDuration;
        }

        public static TimeSpan GetDurationTimeSpan(IMusic music)
        {
            if(String.IsNullOrEmpty(music.Duration))
            {
               return TimeSpan.Zero;
            }
            else
            {
                TimeSpan timeSpan = StringHelper.ConvertMinuteAndSecondTimeToTimeSpan(music.Duration);
                return timeSpan;
            }
        }
    }
    public interface IMusic
    {
        string Title { get; set; }

        string Artist { get; set; }

        string Album { get; set; }

        string Duration { get; set; }

        uint Bitrate { get; set; }

        uint TrackNumber { get; set; }

        uint DiscNumber { get; set; }

        uint Year { get; set; }

        uint Genre { get; set; }
    }

    public class Music : IMusic
    {
        public string Title { get; set; } = "未知";
        public string Artist { get; set; } = "未知艺术家";
        public string Album { get; set; } = "未知专辑";
        public string Duration { get; set; } = "--:--";
        public uint Bitrate { get; set; } = 0;
        public uint TrackNumber { get; set; } = 0;
        public uint DiscNumber { get; set; } = 0;
        public uint Year { get; set; } = 0;
        public uint Genre { get; set; } = 0;
        public string Token { get; set; } = "";
        public MusicType Type { get; set; }
    }

    public class LocalMusic : IMusic
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }
        public uint Bitrate { get; set; }
        public uint TrackNumber { get; set; }
        public uint DiscNumber { get; set; }
        public uint Year { get; set; }
        public uint Genre { get; set; }

        public StorageFile StorageFile { get; set; }

    }

    public class LocalMusicManager
    {
        //生成LocalMusic
        public static LocalMusic CreateLocalMusicFromStorageFile(StorageFile storageFile)
        {
            if (storageFile == null)
                return null;
            LocalMusic localMusic = new LocalMusic();
            localMusic.StorageFile = storageFile;
            localMusic.Title = storageFile.Name;
            return localMusic;
        }

        public static async Task<List<LocalMusic>> GetLocalMusicFromStorageFolderAsync(StorageFolder storageFolder)
        {
            if (storageFolder == null)
                return null;
            List<LocalMusic> localMusicList = new List<LocalMusic>();
            List<IStorageItem> storageItems = (await storageFolder.GetItemsAsync()).ToList();
            foreach (IStorageItem storageItem in storageItems)
            {
                if (storageItem is StorageFile)
                {
                    StorageFile storageFile = (StorageFile)storageItem;
                    if (MusicHelper.SupportedMusicFileTypesString.Contains(storageFile.FileType.ToLower()))
                        localMusicList.Add(CreateLocalMusicFromStorageFile(storageFile));
                }
                else if (storageItem is StorageFolder)
                {
                    localMusicList.AddRange(await GetLocalMusicFromStorageFolderAsync(((StorageFolder)storageItem)));
                }
            }
            return localMusicList;
        }
        //音乐信息获取
        public static async Task GetPropertiesAsync(LocalMusic localMusic)//仅使用系统API获取音乐信息（兼容旧平台使用）
        {
            if (localMusic.StorageFile == null)
                return;
            localMusic.Title = localMusic.StorageFile.Name;
            StorageItemContentProperties storageItemContentProperties = localMusic.StorageFile.Properties;
            MusicProperties musicProperties = await storageItemContentProperties.GetMusicPropertiesAsync(); // 音频属性
            if (!string.IsNullOrEmpty(musicProperties.Title))
                localMusic.Title = musicProperties.Title;
            if (!string.IsNullOrEmpty(musicProperties.Album))
                localMusic.Album = musicProperties.Album;
            if (!string.IsNullOrEmpty(musicProperties.Artist))
                localMusic.Artist = musicProperties.Artist;
            localMusic.Year = musicProperties.Year;
            localMusic.Bitrate = musicProperties.Bitrate;

            localMusic.Duration = StringHelper.TimeNumToString(musicProperties.Duration.Minutes) + ":" + StringHelper.TimeNumToString(musicProperties.Duration.Seconds);
            localMusic.TrackNumber = musicProperties.TrackNumber;
        }

        public static void GetProperties_TagLib(LocalMusic localMusic)//仅使用TagLib获取音乐信息（不常用,信息缺失）
        {
            Music2LocalMusic(localMusic,TagLibHelper.GetMusicProperties(TagLibHelper.GetTagLibFile(localMusic.StorageFile)));
        }

        public static async Task GetProperties_MixedAsync(LocalMusic localMusic)//使用系统API+TagLib获取音乐信息 常用，推荐使用
        {
            Music2LocalMusic(localMusic, await TagLibHelper.GetMusicProperties_MixedAsync(localMusic.StorageFile));
        }

        public static BitmapImage GetCover()
        {
            return new BitmapImage();
        }

        public static async Task<BitmapImage> GetCover_TagLibAsync(LocalMusic localMusic)
        {
            if (localMusic == null)
                return null;
            TagLib.File file = TagLibHelper.GetTagLibFile(localMusic.StorageFile);
            if (file == null)
                return new BitmapImage();
            else
                return await TagLibHelper.GetCoverBitmapImageAsync(file);
        }


        public static void Music2LocalMusic(LocalMusic localMusic,Music music)
        {
            if (music == null)
                return;
            localMusic.Title = music.Title;
            localMusic.Artist = music.Artist;
            localMusic.Album = music.Album;
            localMusic.Duration = music.Duration;
            localMusic.Bitrate = music.Bitrate;
            localMusic.TrackNumber = music.TrackNumber;
            localMusic.DiscNumber = music.DiscNumber;
            localMusic.Year = music.Year;
        }
    }

    public class StreamMusic : IMusic
    {
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
        public string Duration { get; set; } = "00:00";
        public uint Bitrate { get; set; }
        public uint TrackNumber { get; set; }
        public uint DiscNumber { get; set; }
        public uint Year { get; set; }
        public uint Genre { get; set; } = 255;

        public string Url { get; set; }

        public string CoverUrl { get; set; } = "";
    }

    public class StreamMusicManager
    {
        public BitmapImage GetCover()
        {
            return new BitmapImage();
        }
    }

    public class RemovableMusic: IMusic
    {
        public string Title { get; set; }
        public string Artist { get; set; } = "未知艺术家";
        public string Album { get; set; } = "未知专辑";
        public string Duration { get; set; } = "00:00";
        public uint Bitrate { get; set; }
        public uint TrackNumber { get; set; }
        public uint DiscNumber { get; set; }
        public uint Year { get; set; }
        public uint Genre { get; set; }
        public RemovableDevice From { get; set; }
        public StorageFile StorageFile { get; set; }

        public bool IsAvailable 
        {
            get 
            {
                if (StorageFile == null)
                    return false;
                else if(StorageFile.IsAvailable)
                    return true;
                else
                    return false;
            }
        }
    }

    public class RemovableMusicManager
    {
        

        public static async Task<List<RemovableMusic>> GetRemovableMusicFromStorageFolderAsync(StorageFolder storageFolder, RemovableDevice removableDevice)
        {
            if (storageFolder == null)
                return null;
            List<RemovableMusic> removableMusicList = new List<RemovableMusic>();
            List<IStorageItem> storageItems = (await storageFolder.GetItemsAsync()).ToList();
            foreach (IStorageItem storageItem in storageItems)
            {
                if (storageItem is StorageFile)
                {
                    StorageFile storageFile = (StorageFile)storageItem;
                    if (MusicHelper.SupportedMusicFileTypesString.Contains(storageFile.FileType.ToLower()))
                        removableMusicList.Add(CreateRemovableMusicFromStorageFile(storageFile, removableDevice));
                }
                else if (storageItem is StorageFolder)
                {
                    removableMusicList.AddRange(await GetRemovableMusicFromStorageFolderAsync(((StorageFolder)storageItem),removableDevice));
                }
            }
            foreach (RemovableMusic removableMusic in removableMusicList)
            {
                await GetProperties_MixedAsync(removableMusic);
            }
            return removableMusicList;
        }

        public static RemovableMusic CreateRemovableMusicFromStorageFile(StorageFile storageFile,RemovableDevice removableDevice)
        {
            if (storageFile == null)
                return null;
            RemovableMusic removableMusic = new RemovableMusic();
            removableMusic.StorageFile = storageFile;
            removableMusic.Title = storageFile.Name;
            removableMusic.From = removableDevice;
            return removableMusic;
        }

        public static async Task GetProperties_MixedAsync(RemovableMusic removableMusic)//使用系统API+TagLib获取音乐信息 常用，推荐使用
        {
            Music2RemovableMusic(removableMusic, await TagLibHelper.GetMusicProperties_MixedAsync(removableMusic.StorageFile));
        }

        public static void Music2RemovableMusic(RemovableMusic removableMusic, Music music)
        {
            if (music == null)
                return;
            removableMusic.Title = music.Title;
            removableMusic.Artist = music.Artist;
            removableMusic.Album = music.Album;
            removableMusic.Duration = music.Duration;
            removableMusic.Bitrate = music.Bitrate;
            removableMusic.TrackNumber = music.TrackNumber;
            removableMusic.DiscNumber = music.DiscNumber;
            removableMusic.Year = music.Year;
        }
    }

    public class JMusic : IMusic
    {
        public MusicType Type { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }
        public uint Bitrate { get; set; }
        public uint TrackNumber { get; set; }
        public uint DiscNumber { get; set; }
        public uint Year { get; set; }
        public uint Genre { get; set; }
    }

    public class JMusicHelper
    {
        public static JMusic GetJMusic(IMusic music)
        {
            JMusic jMusic = new JMusic();
            if (music is LocalMusic)
            {
                jMusic.Type = MusicType.Local;
                jMusic.Key = ((LocalMusic)music).StorageFile.Path;
            }
            else if(music is StreamMusic)
            {
                jMusic.Type = MusicType.Stream;
                jMusic.Key = ((StreamMusic)music).Url;
                jMusic.Title = music.Title;
                jMusic.Artist = music.Artist;
                jMusic.Album = music.Album;
                jMusic.Duration = music.Duration;
                jMusic.Bitrate = music.Bitrate;
                jMusic.TrackNumber = music.TrackNumber;
                jMusic.DiscNumber = music.DiscNumber;
                jMusic.Year = music.Year;
                jMusic.Genre = music.Genre;
            }
            else if (music is RemovableMusic)
            {
                return null;
                // To-Do:RemovableMusic的保存
                // 需要解决的问题：
                // 当可移动磁盘的盘符改变时，该如何找到对应的RemovableMusic?
            }
            else 
            {
                return null;
            }
            return jMusic;
        }

        public static List<JMusic> GetJMusicList(List<IMusic> musicList)
        {
            List<JMusic> jMusicList = new List<JMusic>();
            foreach (IMusic music in musicList)
            {
                jMusicList.Add(GetJMusic(music));
            }
            return jMusicList;
        }

        public static string GetJMusicListJsonString(List<JMusic> jMusicList)
        {
            JArray jArray = new JArray();
            foreach (JMusic jMusic in jMusicList)
            {
                jArray.Add(JObject.FromObject(jMusic));
            }
            return jArray.ToString();
        }


        public static List<IMusic> GetMusicFromJArray(JArray jArray, List<LocalMusic> localMusicList)
        {

            List<IMusic> music = new List<IMusic>();

            foreach (JObject jObject in jArray)
            {
                int type = Convert.ToInt32(jObject["Type"]);
                switch (type)
                {
                    case 0:
                        LocalMusic localMusic = localMusicList.Find(x => x.StorageFile.Path.Equals((string)jObject["Key"]));
                        if (localMusic != null)
                            music.Add(localMusic);
                        break;
                    case 1:
                        StreamMusic streamMusic = new StreamMusic();
                        streamMusic.Url = (string)jObject["Key"];
                        streamMusic.Title = (string)jObject["Title"];
                        streamMusic.Artist = (string)jObject["Artist"];
                        streamMusic.Album = (string)jObject["Album"];
                        streamMusic.CoverUrl = (string)jObject["CoverUrl"];
                        streamMusic.Genre = Convert.ToUInt32((string)jObject["Genre"]);
                        streamMusic.Year = Convert.ToUInt32((string)jObject["Year"]);
                        music.Add(streamMusic);
                        break;
                    case 2:
                        RemovableMusic removableMusic = new RemovableMusic();
                        //To-Do:RemovableMusic
                        break;
                    default:
                        music.Add(new Music { Title = jObject["Key"].ToString() });
                        break;
                }
            }
            return music;
        }
    }
}

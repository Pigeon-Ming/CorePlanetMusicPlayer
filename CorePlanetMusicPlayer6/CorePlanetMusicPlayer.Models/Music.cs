using CorePlanetMusicPlayer.Models.Helpers;
using CorePlanetMusicPlayer.Models.TagLibModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Models
{
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

        Dictionary<string,string> ExternalInfo { get; set; }
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
        public Dictionary<string, string> ExternalInfo { get; set; }
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

        public Dictionary<string, string> ExternalInfo { get; set; }

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
                    if (StorageHelper.SupportedMusicFileTypesString.Contains(storageFile.FileType.ToLower()))
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

    public class OnlineMusic : IMusic
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }
        public uint Bitrate { get; set; }
        public uint TrackNumber { get; set; }
        public uint DiscNumber { get; set; }
        public uint Year { get; set; }

        public Dictionary<string, string> ExternalInfo { get; set; }

        public string Url { get; set; }

        public string CoverUrl { get; set; }
    }

    public class OnlineMusicManaer
    {
        public BitmapImage GetCover()
        {
            return new BitmapImage();
        }
    }
}

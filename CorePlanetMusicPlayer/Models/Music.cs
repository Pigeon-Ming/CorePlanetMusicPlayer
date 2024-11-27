using CorePlanetMusicPlayer.Models.TagLibHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace CorePlanetMusicPlayer.Models
{
    public enum MusicType
    {
        Local, Online,
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
        //public static async Task<Music> GetMusicPropertiesAsync(Music music)
        //{
            
        //    return music;
        //}

        public static async Task<Music> GetLocalMusicPropertiesAsync(Music music)
        {
            if (music.MusicType != MusicType.Local)
            {
                return music;
            }
            StorageFile storageFile = Library.MusicFiles.Find(x => x.Path == music.DataCode);
            music.Title = storageFile.Name;
            music.Album = "未知专辑";
            music.Artist = "未知艺术家";
            music.Duration = "--:--";
            if (storageFile == null)
                return music;
            StorageFile file = storageFile;
            StorageItemContentProperties storageItemContentProperties = file.Properties;
            MusicProperties musicProperties = await storageItemContentProperties.GetMusicPropertiesAsync(); // 音频属性
            if (!string.IsNullOrEmpty(musicProperties.Title))
                music.Title = musicProperties.Title;
            if (!string.IsNullOrEmpty(musicProperties.Album))
                music.Album = musicProperties.Album;
            if (!string.IsNullOrEmpty(musicProperties.Artist))
                music.Artist = musicProperties.Artist;
            music.Year = musicProperties.Year;
            music.Bitrate = musicProperties.Bitrate;
            music.Duration = musicProperties.Duration.ToString().Substring(3, 5);
            music.TrackNumber = musicProperties.TrackNumber;

            if (storageFile.FileType == ".ac3" || storageFile.FileType == ".m4a")
            {
                return music;
            }
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(storageFile);
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
        public static void SetMusicCache(Music music)
        {
            if(music.MusicType == MusicType.Local)
            {
                SQLiteManager.MusicDataBasesHelper.SetTableData(StorageManager.LocalFolderPath + "\\Cache\\MusicCache.db", "LocalMusic",music);
            }
        }
        public static Music GetMusicFromMusicTypeAndDataCode(MusicType musicType,String dataCode)
        {
            List<Music>musicList = Library.Music.Where(x => x.MusicType == musicType && x.DataCode == dataCode).ToList();
            if(musicList.Count>0)
                return musicList[0];
            return null;
        }
        public static Music GetMusic(Music music)
        {
            List<Music> musicList = Library.Music.Where(x => x.MusicType == music.MusicType && x.DataCode == music.DataCode).ToList();
            if (musicList.Count > 0)
                return musicList[0];
            return null;
        }

        //OnlineMusic
        public static void AddOnlineMusic(Music music)
        {
            SQLiteManager.MusicDataBasesHelper.SetTableData(StorageManager.LocalFolderPath + "\\DataBases\\MusicLibrary.db", "OnlineMusic", music);
        }

        public static void DeleteOnlineMusic(Music music)
        {
            SQLiteManager.MusicDataBasesHelper.DeleteTableData(StorageManager.LocalFolderPath + "\\DataBases\\Library.db", "OnlineMusic", music.DataCode);
        }

        //ExternalLocalMusic
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

            StorageFile storageFile = await picker.PickSingleFileAsync();
            if (storageFile == null) return false;

            String key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile);
            Library.ExternalMusicKeys.Add(key);

            SaveExternalData();
            return true;
        }

        public static void SaveExternalData()
        {
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < Library.ExternalMusicKeys.Count; i++)
            {
                jsonArray.Add(JsonValue.CreateStringValue(Library.ExternalMusicKeys[i]));
            }
        }

        public static void DeleteExternalLocalMusic(Music music)
        {
            //SQLiteManager.MusicListDataBasesHelper.DeleteTableData(StorageManager.LocalFolderPath + "\\DataBases\\MusicLibrary.db", "OnlineMusic", music.DataCode);
        }

        public static async Task<StorageFile> GetExternalMusicByExternalMusicKeyAsync(string Key)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(Key);
        }

        public static async Task<List<String>> GetExternalLocalMusicKeys()
        {
            if (await StorageManager.IsItemExsitAsync(ApplicationData.Current.LocalFolder, "ExternalLocalMusic.json"))
            {
                String fileStr = "";
                fileStr = await StorageManager.ReadFile(ApplicationData.Current.LocalFolder, "ExternalLocalMusic.json");
                JsonArray jsonArray = new JsonArray();
                if (JsonArray.TryParse(fileStr, out jsonArray) == false)
                    return new List<String>();
                List<String> Keys = new List<String>();
                for (uint i = 0; i < jsonArray.Count; i++)
                    Keys.Add(jsonArray.GetStringAt(i));
                return Keys;
            }
            else
            {
                await ApplicationData.Current.LocalFolder.CreateFileAsync("ExternalLocalMusic.json");
                return new List<String>();
            }

        }
    }
}

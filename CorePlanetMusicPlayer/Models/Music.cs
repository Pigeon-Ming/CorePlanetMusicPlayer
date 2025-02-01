using CorePlanetMusicPlayer.Models.TagLibHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Local,ExternalLocal, Online,Removable
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
        public String Key { get; set; } = "";

        public bool Available { get; set; } = true;
    }

    public class MusicManager
    {
        //public static async Task<Music> GetMusicPropertiesAsync(Music music)
        //{
            
        //    return music;
        //}

        public static async Task<Music> GetRemovableMusicPropertiesAsync(StorageFile storageFile,Music music)
        {
            return await GetMusicPropertiesByStorageFile(storageFile, music);
        }

        public static async Task<Music> GetLocalMusicPropertiesAsync(Music music)
        {
            if (music.MusicType != MusicType.Local && music.MusicType != MusicType.ExternalLocal)
            {
                return music;
            }
            StorageFile storageFile = Library.MusicFiles.Find(x => x.Path == music.DataCode);
            return await GetMusicPropertiesByStorageFile(storageFile,music);
        }

        public static async Task<Music> GetMusicPropertiesByStorageFile(StorageFile storageFile,Music music)
        {
            if (storageFile == null) return music;
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
            
            music.Duration = TimeToString(musicProperties.Duration.Minutes) +":"+ TimeToString(musicProperties.Duration.Seconds);
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
                Debug.WriteLine("正在获取文件信息：" + file.Path);
                _file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
                music.DiscNumber = _file.Tag.Disc;
            }
            catch
            {

            }

            return music;
        }

        private static string TimeToString(int timeValue)
        {
            if (timeValue < 10)
                return "0" + timeValue;
            else
                return timeValue.ToString();
        }

        public static void SetMusicCache(Music music)
        {
            if(music.MusicType == MusicType.Local)
            {
                SQLiteManager.MusicDataBasesHelper.SetTableData(StorageManager.LocalFolderPath + "\\Cache\\MusicCache.db", "LocalMusic",music);
            }else if(music.MusicType == MusicType.ExternalLocal)
                SQLiteManager.MusicDataBasesHelper.SetTableData(StorageManager.LocalFolderPath + "\\Cache\\MusicCache.db", "ExternalLocalMusic", music);
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
            if (music.MusicType == MusicType.Removable)
            {
                return RemovableDeviceManager.GetMusic(music);
            }
            else
            {
                List<Music> musicList = Library.Music.Where(x => x.MusicType == music.MusicType && x.DataCode == music.DataCode).ToList();
                if (musicList.Count > 0)
                    return musicList[0];
            }
            return null;
        }

        //OnlineMusic
        public static void AddOnlineMusic(Music music)
        {
            Library.Music.Add(music);
            SQLiteManager.MusicDataBasesHelper.SetTableData(StorageManager.LocalFolderPath + "\\DataBases\\MusicLibrary.db", "OnlineMusic", music);
        }

        public static void DeleteOnlineMusic(Music music)
        {
            Library.Music.Remove(music);
            SQLiteManager.MusicDataBasesHelper.DeleteTableData(StorageManager.LocalFolderPath + "\\DataBases\\MusicLibrary.db", "OnlineMusic", music.DataCode);
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

            List<StorageFile> storageFiles = (await picker.PickMultipleFilesAsync()).ToList();
            if (storageFiles == null || storageFiles.Count == 0) return false;

            for(int i = 0; i < storageFiles.Count; i++)
            {
                String key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFiles[i]);
                Library.ExternalMusicKeys.Add(key);
                Library.MusicFiles.Add(storageFiles[i]);
                Music music = new Music { MusicType = MusicType.ExternalLocal, DataCode = storageFiles[i].Path ,Key = Library.ExternalMusicKeys[i] };
                Library.Music.Add(await GetLocalMusicPropertiesAsync(music));
            }
            await SaveExternalDataAsync();
            return true;
        }

        public static async Task<bool> AddExternalMusicAsync(StorageFile storageFile)
        {
            if (storageFile == null) return false;

            String key = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(storageFile);
            Library.ExternalMusicKeys.Add(key);
            Library.MusicFiles.Add(storageFile);
            Music music = new Music { MusicType = MusicType.ExternalLocal, DataCode = storageFile.Path ,Key = key };
            Library.Music.Add(await GetLocalMusicPropertiesAsync(music));
            await SaveExternalDataAsync();
            return true;
        }

        public static async Task SaveExternalDataAsync()
        {
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < Library.ExternalMusicKeys.Count; i++)
            {
                jsonArray.Add(JsonValue.CreateStringValue(Library.ExternalMusicKeys[i]));
            }
            await StorageManager.WriteFile(ApplicationData.Current.LocalFolder, "ExternalLocalMusic.json", jsonArray.ToString());
        }

        public static async Task<bool> DeleteExternalLocalMusicAsync(Music music)
        {
            //SQLiteManager.MusicListDataBasesHelper.DeleteTableData(StorageManager.LocalFolderPath + "\\DataBases\\MusicLibrary.db", "OnlineMusic", music.DataCode);
            Library.ExternalMusicKeys.Remove(music.Key);
            bool isSuccess = Library.Music.Remove(music);
            await SaveExternalDataAsync();
            return isSuccess;
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

        //RemovableMusic
        //public static async Task GetRemovableMusic()
        //{

        //}

        public static async Task AddTempMusicToLibraryAsync(Music music)
        {
            StorageFile storageFile = Library.TempMusicFiles.Find(x => x.Path == music.DataCode.Replace("pmptemp-",""));
            if(storageFile == null) return;
            if (Library.MusicFiles.Find(x => x.Path == storageFile.Path) != null) return;
            await AddExternalMusicAsync(storageFile);
            //LibraryManager.AddExternalMusicFromLocalMusic(localMusic);
        }

        public static TimeSpan CountMusicDuration(List<Music>musicList)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(0);
            for (int i = 0; i < musicList.Count; i++)
            {
                if (!String.IsNullOrEmpty(musicList[i].Duration))
                    timeSpan += TimeSpan.Parse("00:" + musicList[i].Duration);
            }
            return timeSpan;
        }
    }
}

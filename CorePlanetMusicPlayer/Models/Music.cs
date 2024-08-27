using CorePlanetMusicPlayer.Models.Taglib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TagLib;
using Windows.Data.Json;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Models
{
    public class Music
    {
        public String Title { get; set; }
        public String Artist { get; set; }
        public String Album { get; set; }//专辑名称
        public uint Bitrate { get; set; }//比特率
        public uint Year { get; set; }//年份
        public uint TrackNumber { get; set; }//专辑内音乐编号
        public uint DiscNumber { get; set; }//专辑内音乐编号
        public String Duration { get; set; }//长度

        public StorageFile file { get; set; }

        public String Token {  get; set; }
        //public MediaSource source { get; set; }
        public BitmapImage cover { get; set; }
    }

    public class JsonMusic
    {

        public String Title { get; set; }
        public String Artist { get; set; }
        public String Album { get; set; }//专辑名称
        public String Duration { get; set; }//长度

        public String FilePath { get; set; }
    }

    public class JsonMusicList
    {
        public List<JsonMusic> list { get; set; } = new List<JsonMusic>();
    }

    public class MusicManager
    {

        public static async Task SetMusicPropertiesAsync(Music music)
        {
            Windows.Storage.StorageFile file = music.file;
            StorageItemContentProperties storageItemContentProperties = file.Properties;
            MusicProperties musicProperties = await storageItemContentProperties.GetMusicPropertiesAsync(); // 音频属性
            if (!string.IsNullOrEmpty(music.Title))
            {
                musicProperties.Title = music.Title;
            }

            if (!string.IsNullOrEmpty(music.Album))
            {
                musicProperties.Album = music.Album;
            }
            else
            {
                musicProperties.Album = "未知专辑";
            }
            if (!string.IsNullOrEmpty(music.Artist))
            {
                musicProperties.Artist = music.Artist;
            }
            else
            {
                musicProperties.Artist = "未知艺术家";
            }
            musicProperties.Year = music.Year;
            await musicProperties.SavePropertiesAsync();
        }

        public static async Task<Music> GetMusicPropertiesAsync_Single(Music music)
        {
            StorageFile file = music.file;
            StorageItemContentProperties storageItemContentProperties = file.Properties;
            MusicProperties musicProperties = await storageItemContentProperties.GetMusicPropertiesAsync(); // 音频属性
            if (!string.IsNullOrEmpty(musicProperties.Title))
                music.Title = musicProperties.Title;
            else
                music.Title = music.file.Name;
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

            if (music.file.FileType == ".ac3" || music.file.FileType == ".m4a")
            {
                return music;
            }
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(music.file);
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

                //    IDictionary<string, object> extraProperties =
                //await file.Properties.RetrievePropertiesAsync(properties);

                //    if (extraProperties.ContainsKey("System.Music.DiscNumber"))
                //    {

                //        music.DiscNumber = Convert.ToUInt32(extraProperties["System.Music.DiscNumber"]);
                //    }
                //    else
                //    {
                //        music.DiscNumber = 1;
                //    }
                //if (LibraryManager.gotPropertyCount == Library.LocalLibraryMusic.Count)
                //{
                //    AlbumManager.ClassifyAlbum();
                //    ArtistManager.ClassifyArtist();
                //    SetMusicPropertiesToJson();
                //}

                return music;
        }

        //static List<String> properties = new List<String> { "System.Music.DiscNumber" };
        public static async Task<Music> GetMusicPropertiesAsync(Music music)
        {
            
            //Debug.WriteLine(LibraryManager.gotPropertyCount+"=>"+ Library.LocalLibraryMusic.Count);
            if (music.file == null)
            {
                ++LibraryManager.gotPropertyCount;
                if (LibraryManager.gotPropertyCount == Library.LocalLibraryMusic.Count)
                {
                    AlbumManager.ClassifyAlbum();
                    ArtistManager.ClassifyArtist();
                    SetMusicPropertiesToJson();
                }
                return music;
            }
            if (music.Album!="未知专辑"|| music.Artist != "未知艺术家")
            {
                ++LibraryManager.gotPropertyCount;
                if (LibraryManager.gotPropertyCount == Library.LocalLibraryMusic.Count)
                {
                    AlbumManager.ClassifyAlbum();
                    ArtistManager.ClassifyArtist();
                    SetMusicPropertiesToJson();
                }
                return music;
            }
                
            
            StorageFile file = music.file;
            StorageItemContentProperties storageItemContentProperties = file.Properties;
            MusicProperties musicProperties = await storageItemContentProperties.GetMusicPropertiesAsync(); // 音频属性
            if (!string.IsNullOrEmpty(musicProperties.Title))
                music.Title = musicProperties.Title;

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



            //// 死活读不出来，鬼知道是什么问题
            //IDictionary<string, object> extraProperties =
            //    await file.Properties.RetrievePropertiesAsync(properties);

            //Debug.WriteLine("DiscNumber:" + extraProperties["System.Music.DiscNumber"]);
            //if (extraProperties.ContainsKey("System.Music.DiscNumber"))
            //{
               
            //    music.DiscNumber = Convert.ToUInt32(extraProperties["System.Music.DiscNumber"]);
            //}
            //else
            //{
            //    music.DiscNumber = 1;
            //}


            ++LibraryManager.gotPropertyCount;
            if (LibraryManager.gotPropertyCount == Library.LocalLibraryMusic.Count)
            {
                AlbumManager.ClassifyAlbum();
                ArtistManager.ClassifyArtist();
                SetMusicPropertiesToJson();
            }

            return music;
        }

        public static async Task<Music> GetMusicCoverAsync(Music music)
        {
            StorageFile file = music.file;
            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView);
            music.cover = new BitmapImage();
            if (thumbnail != null)
            {
                
                music.cover.SetSource(thumbnail);
                
            }
            return music;
        }

        public static async Task<Music> GetMusicCoverAsync_Taglib(Music music)
        {
            if (music.file.FileType == ".ac3" || music.file.FileType == ".m4a")
            {
                return await GetMusicHDCoverAsync(music);
            }
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(music.file);
            TagLib.File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
            }
            catch (Exception ex)
            {
                return await GetMusicHDCoverAsync(music);
            }
            BitmapImage bitmapImage = new BitmapImage();
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            if (file.Tag.Pictures.Length <= 0) return await GetMusicHDCoverAsync(music);
            await stream.WriteAsync(file.Tag.Pictures[0].Data.Data.AsBuffer());
            stream.Seek(0);
            Debug.WriteLine(stream==null);
            try
            {
                await bitmapImage.SetSourceAsync(stream);
            }
            catch
            {
                return await GetMusicHDCoverAsync(music);
            }
            Debug.WriteLine(stream == null);
            bitmapImage.DecodePixelHeight = 200;
            bitmapImage.DecodePixelWidth = 200;
            music.cover = bitmapImage;
            return music;
        }
        public static async Task<Music> GetMusicHDCoverAsync_Taglib(Music music)
        {
            if (music == null) return null;
            if(music.file.FileType == ".ac3" || music.file.FileType == ".m4a")
            {
                return await GetMusicHDCoverAsync(music);
            }
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(music.file);
            TagLib.File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
            }catch (Exception ex)
            {
                return await GetMusicHDCoverAsync(music);
            }
            BitmapImage bitmapImage = new BitmapImage();
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            if (file.Tag.Pictures.Length <= 0) return await GetMusicHDCoverAsync(music);
            await stream.WriteAsync(file.Tag.Pictures[0].Data.Data.AsBuffer());
            stream.Seek(0);
            try
            {
                await bitmapImage.SetSourceAsync(stream);
            }
            catch
            {
                return await GetMusicHDCoverAsync(music);
            }
            music.cover = bitmapImage;
            return music;
        }
        public static Music FindMusicByFileName(String searchName)
        {
            Music music = Library.LocalLibraryMusic.Find(x => x.file.Name == searchName);
            return music;
        }

        public static async Task<Music> GetMusicHDCoverAsync(Music music)
        {
            if (music == null) return music;
            StorageFile file = music.file;
            if (music.file == null) return music;
            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);

            try
            {
                music.cover = new BitmapImage();
                if (thumbnail != null)
                {

                    music.cover.SetSource(thumbnail);

                }
            }catch (Exception ex)
            {

            }
            
            return music;
        }

        public static async Task SetMusicPropertiesToJson()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = (StorageFile)await folder.TryGetItemAsync("MusicProperties.json");
            if (file == null)
                file = await folder.CreateFileAsync("MusicProperties.json");
            List<JsonMusic> jsonMusicList = new List<JsonMusic>();
            for(int i = 0; i < Library.LocalLibraryMusic.Count; i++)
            {
                jsonMusicList.Add(MusicToJsonMusic(Library.LocalLibraryMusic[i]));
            }
            //Debug.WriteLine();
            await Windows.Storage.FileIO.WriteTextAsync(file, JsonSerializer.Serialize<List<JsonMusic>>(jsonMusicList));
        }

        public static async Task ReadMusicPropertiesFromJson(bool GetCover)
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = (StorageFile)await folder.TryGetItemAsync("MusicProperties.json");
            if (file == null)
            {
                LibraryManager.ReloadLibraryAsync(GetCover);
                return;
            }
            string filecontent = await Windows.Storage.FileIO.ReadTextAsync(file);
            List<JsonMusic> jsonMusicList = new List<JsonMusic>();
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.IncludeFields = false;
            try
            {
                jsonMusicList = JsonSerializer.Deserialize<List<JsonMusic>>(filecontent, jsonSerializerOptions);
            }catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                LibraryManager.GetAllMusicInfo(GetCover);
                return;
            }
            //for(int i=0;i<jsonMusicList.Count;i++)
            //    Debug.WriteLine(jsonMusicList[i].FilePath);


            for (int i = 0; i < Library.LocalLibraryMusic.Count; i++)
            {
                //Debug.WriteLine("正在从Json数据中获取："+ Library.LocalLibraryMusic[i].file.Path+"的信息……");
                JsonMusic jsonMusic = jsonMusicList.Find(x => x.FilePath.Contains(Library.LocalLibraryMusic[i].file.Path));
                if (jsonMusic != null)
                    SetPropertiesFromJsonMusic(Library.LocalLibraryMusic[i], jsonMusic);
                else
                    await GetMusicPropertiesAsync(Library.LocalLibraryMusic[i]);
                GetMusicHDCoverAsync(Library.LocalLibraryMusic[i]);
            }
            AlbumManager.ClassifyAlbum();
            ArtistManager.ClassifyArtist();
    
        }

        public static JsonMusic MusicToJsonMusic(Music music)
        {
            JsonMusic jsonMusic = new JsonMusic();
            jsonMusic.Title = music.Title;
            jsonMusic.Artist = music.Artist;
            jsonMusic.Album = music.Album;
            jsonMusic.Duration = music.Duration;
            if (music.file != null)
                jsonMusic.FilePath = music.file.Path;
            return jsonMusic;
        }

        public static void SetPropertiesFromJsonMusic(Music music,JsonMusic jsonMusic)
        {
            music.Title = jsonMusic.Title;
            music.Artist = jsonMusic.Artist;
            music.Album = jsonMusic.Album;
            music.Duration = jsonMusic.Duration;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage.FileProperties;
using Windows.Storage;
using CorePlanetMusicPlayer.Models.Helpers;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace CorePlanetMusicPlayer.Models.TagLibModels
{
    public class TagLibHelper
    {
        public static async Task<Music> GetMusicProperties_MixedAsync(StorageFile storageFile)//使用系统API+TagLib获取信息
        {
            Music music = new Music();
            music.Title = storageFile.Name;
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

            music.Duration = StringHelper.TimeNumToString(musicProperties.Duration.Minutes) + ":" + StringHelper.TimeNumToString(musicProperties.Duration.Seconds);
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

        public static Music GetMusicProperties(TagLib.File file)//仅使用TagLib获取信息（不常用,信息缺失）
        {
            if (file == null)
                return null;
            Music music = new Music();
            music.Title = file.Tag.Title;
            //music. file.Tag.Subtitle;
            //StringHelper.StringArrayToString(file.Tag.Performers, "; ")));
            //StringHelper.StringArrayToString(file.Tag.Composers, "; ")));
            //file.Tag.Conductor;
            music.Album = file.Tag.Album;
            // file.Tag.Comment;
            music.Year = file.Tag.Year;
            music.DiscNumber = file.Tag.Disc;
            music.TrackNumber = file.Tag.Track;
            //music.Duration = file.Tag.;
            //music.Bitrate

            //file.Tag.Copyright;
            return music;
        }

        public static async Task<WriteableBitmap> GetCoverWrtieableBitmapAsync(TagLib.File file, int width = 300, int height=300)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(300,300);
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            if (file.Tag.Pictures.Length <= 0) return null;
            await stream.WriteAsync(file.Tag.Pictures[0].Data.Data.AsBuffer());
            stream.Seek(0);
            //Debug.WriteLine(stream == null);
            try
            {
                await writeableBitmap.SetSourceAsync(stream);
            }
            catch
            {
                return null;
            }
            return writeableBitmap;
        }

        public static async Task<BitmapImage> GetCoverBitmapImageAsync(TagLib.File file)
        {
            BitmapImage bitmapImage = new BitmapImage();
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            if (file.Tag.Pictures.Length <= 0) return null;
            await stream.WriteAsync(file.Tag.Pictures[0].Data.Data.AsBuffer());
            stream.Seek(0);
            //Debug.WriteLine(stream == null);
            try
            {
                await bitmapImage.SetSourceAsync(stream);
            }
            catch
            {
                return null;
            }
            return bitmapImage;
        }

        public static TagLib.File GetTagLibFile(StorageFile storageFile)//获取TagLib.File对象，以获取更多信息
        {
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(storageFile);
            TagLib.File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
            }
            catch (Exception ex)
            {
                return null;
            }
            return file;
        }
    }
}

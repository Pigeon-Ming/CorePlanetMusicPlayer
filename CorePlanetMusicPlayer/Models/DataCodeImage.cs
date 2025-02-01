using CorePlanetMusicPlayer.Models.TagLibHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Models
{
    public class DataCodeImage
    {
        public BitmapImage BitmapImage { get; set; }

        public string DataCode { get; set; } = "";
    }

    public class ImageManager
    {
        public static async Task GetLocalMusicCoverForLibrary(Music music)
        {
            DataCodeImage image = new DataCodeImage();
            image.DataCode = music.DataCode;
            image.BitmapImage = await GetLocalMusicCoverAsync_Taglib(music,200,200);
            Library.MusicCovers.Add(image);
        }

        public static async Task<DataCodeImage> GetRemovableMusicCoverForLibraryAsync(Music music)
        {
            if(music==null||music.MusicType != MusicType.Removable)
                return new DataCodeImage();
            StorageFile file = RemovableDeviceManager.GetMusicFile(music);
            if (file == null)
                return new DataCodeImage();
            DataCodeImage image = new DataCodeImage();
            image.DataCode = music.DataCode;
            image.BitmapImage = await ImageManager.GetLocalMusicCoverAsync_Taglib(file, 400, 400);
            return image;
        }

        public static async Task<BitmapImage> GetLocalMusicCoverAsync_Taglib(Music music,int width,int height)
        {
            StorageFile storageFile = LibraryManager.GetLocalMusicFile(music);
            if (storageFile == null) return null;
            if (storageFile.FileType == ".ac3" || storageFile.FileType == ".m4a")
            {
                return await GetLocalMusicHDCoverAsync(music);
            }
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(storageFile);
            TagLib.File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
            }
            catch (Exception ex)
            {
                return await GetLocalMusicHDCoverAsync(music);
            }
            BitmapImage bitmapImage = new BitmapImage();
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            if (file.Tag.Pictures.Length <= 0) return await GetLocalMusicHDCoverAsync(music);
            await stream.WriteAsync(file.Tag.Pictures[0].Data.Data.AsBuffer());
            stream.Seek(0);
            //Debug.WriteLine(stream == null);
            try
            {
                await bitmapImage.SetSourceAsync(stream);
            }
            catch
            {
                return await GetLocalMusicHDCoverAsync(music);
            }
            //Debug.WriteLine(stream == null);
            bitmapImage.DecodePixelHeight = width;
            bitmapImage.DecodePixelWidth = height;
            return bitmapImage;
        }

        public static async Task<BitmapImage> GetLocalMusicCoverAsync_Taglib(StorageFile storageFile, int width, int height)
        {
            if (storageFile == null) return null;
            if (storageFile.FileType == ".ac3" || storageFile.FileType == ".m4a")
            {
                
            }
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(storageFile);
            TagLib.File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
            TagLib.File file;
            try
            {
                file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
            }
            catch (Exception ex)
            {
                return await GetLocalMusicHDCoverAsync(storageFile);
            }
            BitmapImage bitmapImage = new BitmapImage();
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            if (file.Tag.Pictures.Length <= 0) return await GetLocalMusicHDCoverAsync(storageFile);
            await stream.WriteAsync(file.Tag.Pictures[0].Data.Data.AsBuffer());
            stream.Seek(0);
            //Debug.WriteLine(stream == null);
            try
            {
                await bitmapImage.SetSourceAsync(stream);
            }
            catch
            {
                return await GetLocalMusicHDCoverAsync(storageFile);
            }
            //Debug.WriteLine(stream == null);
            bitmapImage.DecodePixelHeight = width;
            bitmapImage.DecodePixelWidth = height;
            return bitmapImage;
        }

        //public static async Task<InMemoryRandomAccessStream> GetMusicCoverStreamAsync(Music music)
        //{
        //    StorageFile storageFile = LibraryManager.GetLocalMusicFile(music);
        //    if (storageFile == null) return null;
        //    if (storageFile.FileType == ".ac3" || storageFile.FileType == ".m4a")
        //    {
        //        return null;
        //    }
        //    UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(storageFile);
        //    TagLib.File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
        //    TagLib.File file;
        //    try
        //    {
        //        file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //    BitmapImage bitmapImage = new BitmapImage();
        //    InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
        //    if (file.Tag.Pictures.Length <= 0)
        //    {
        //        return null;
        //    }
        //    await stream.WriteAsync(file.Tag.Pictures[0].Data.Data.AsBuffer());
        //    stream.Seek(0);
        //    return stream;
        //}
        public static async Task<BitmapImage> GetLocalMusicHDCoverAsync(Music music)
        {
            if (music == null) return new BitmapImage();
            StorageFile storageFile = LibraryManager.GetLocalMusicFile(music);
            if (storageFile == null) return new BitmapImage();
            StorageItemThumbnail thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
            BitmapImage bitmapImage = new BitmapImage();
            try
            {

                bitmapImage = new BitmapImage();
                if (thumbnail != null)
                {

                    bitmapImage.SetSource(thumbnail);

                }
            }
            catch (Exception ex)
            {

            }

            return bitmapImage;
        }

        public static async Task<BitmapImage> GetLocalMusicHDCoverAsync(StorageFile storageFile)
        {
            if (storageFile == null) return new BitmapImage();
            StorageItemThumbnail thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
            BitmapImage bitmapImage = new BitmapImage();
            try
            {

                bitmapImage = new BitmapImage();
                if (thumbnail != null)
                {

                    bitmapImage.SetSource(thumbnail);

                }
            }
            catch (Exception ex)
            {

            }

            return bitmapImage;
        }


        public static DataCodeImage GetCoverImageByMusic(Music Music)
        {
            DataCodeImage image = null;
            if (Music.MusicType == MusicType.Local || Music.MusicType == MusicType.ExternalLocal ||Music.MusicType == MusicType.Removable)
            {
                image = Library.MusicCovers.Find(x => x.DataCode == Music.DataCode);
                if (image == null || image.BitmapImage == null)
                    return new DataCodeImage();
                return image;
            }else if(Music.MusicType == MusicType.Online)
            {
                if(String.IsNullOrEmpty(Music.Key))
                    return image;
                image = Library.MusicCovers.Find(x => x.DataCode == Music.DataCode);
                if (image != null && image.BitmapImage != null)
                    return image;
                image = new DataCodeImage();
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.UriSource = new Uri(Music.Key);
                image.DataCode = Music.DataCode;
                image.BitmapImage = bitmapImage;
                return image;
            }
            return new DataCodeImage();
        }
    }
}

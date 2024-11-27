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
            image.BitmapImage = await GetLocalMusicCoverAsync_Taglib(music);
            Library.MusicCovers.Add(image);
        }
        public static async Task<BitmapImage> GetLocalMusicCoverAsync_Taglib(Music music)
        {
            StorageFile storageFile = LibraryManager.GetLocalMusicFile(music);
            if(storageFile==null)return null;
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
            bitmapImage.DecodePixelHeight = 200;
            bitmapImage.DecodePixelWidth = 200;
            return bitmapImage;
        }
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


        public static DataCodeImage GetCoverImageByMusic(Music Music)
        {
            DataCodeImage image = null;
            if (Music.MusicType == MusicType.Local)
            {
                image = Library.MusicCovers.Find(x => x.DataCode == Music.DataCode);
                if (image == null || image.BitmapImage == null)
                    return new DataCodeImage();
                return image;
            }
            return new DataCodeImage();
        }
    }
}

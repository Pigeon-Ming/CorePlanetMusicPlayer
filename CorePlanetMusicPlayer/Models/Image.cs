using CorePlanetMusicPlayer.Models.TagLibHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage.FileProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.IO;

namespace CorePlanetMusicPlayer.Models
{
    public class Image
    {
        public BitmapImage BitmapImage { get; set; }

        public string Token { get; set; } = "";
    }

    public class ImageManager
    {
        public static async Task GetLocalMusicCover_Library(LocalMusic music)
        {
            Image image = new Image();
            image.Token = music.DataCode;
            image.BitmapImage = await GetLocalMusicCoverAsync_Taglib(music);
            Library.Image.LocalMusicCover.Add(image);
        }
        public static async Task<BitmapImage> GetLocalMusicCoverAsync_Taglib(LocalMusic music)
        {
            if (music.StorageFile.FileType == ".ac3" || music.StorageFile.FileType == ".m4a")
            {
                return await GetLocalMusicHDCoverAsync(music);
            }
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(music.StorageFile);
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
        public static async Task<BitmapImage> GetLocalMusicHDCoverAsync(LocalMusic music)
        {
            if (music == null) return new BitmapImage();
            StorageFile file = music.StorageFile;
            if (music.StorageFile == null) return new BitmapImage();
            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
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


        public static Image GetCoverImageByMusic(Music Music)
        {
            Image image = null;
            if (Music.MusicType == MusicType.Local)
            {
                image = Library.Image.LocalMusicCover.Find(x => x.Token == Music.DataCode);
                if (image == null || image.BitmapImage == null)
                    return new Image();
                return image;
            }
            return new Image();
        }
    }
}

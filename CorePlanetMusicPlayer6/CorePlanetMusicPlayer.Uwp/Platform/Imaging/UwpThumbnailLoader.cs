using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Uwp.Platform.Imaging
{
    public sealed class UwpThumbnailLoader
    {
        public async Task<BitmapImage> LoadMusicThumbnailAsync(StorageFile file)
        {
            if(file == null)
            {
                return null;
            }

            StorageItemThumbnail thumbnail = null;

            try
            {
                thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 512, ThumbnailOptions.UseCurrentScale);

                if (thumbnail == null || thumbnail.Size == 0)
                {
                    return null;
                }

                return await LoadFromStreamAsync(thumbnail);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (thumbnail != null)
                {
                    thumbnail.Dispose();
                }
            }
        }

        public async Task<BitmapImage> LoadImageFileAsync(
            StorageFile file)
        {
            if (file == null)
            {
                return null;
            }

            IRandomAccessStream stream = null;

            try
            {
                stream = await file.OpenReadAsync();

                if (stream == null || stream.Size == 0)
                {
                    return null;
                }

                return await LoadFromStreamAsync(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        private async Task<BitmapImage> LoadFromStreamAsync(IRandomAccessStream stream)
        {
            if (stream == null || stream.Size == 0)
            {
                return null;
            }

            try
            {
                stream.Seek(0);

                var image = new BitmapImage();

                await image.SetSourceAsync(stream);

                return image;
            }
            catch
            {
                return null;
            }
        }

        public BitmapImage LoadFromUri(string uriText)
        {
            if (string.IsNullOrWhiteSpace(uriText))
            {
                return null;
            }

            try
            {
                return new BitmapImage(new Uri(uriText));
            }
            catch
            {
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Uwp.Platform.Imaging
{
    public sealed class UwpThumbnailLoader
    {
        public async Task<BitmapImage> LoadMusicThumbnailAsync(StorageFile file, int decodePixelSize = 256)
        {
            ValidateDecodeSize(decodePixelSize);

            if (file == null)
            {
                return null;
            }

            try
            {
                using (var thumbnail = await file.GetThumbnailAsync(
                    ThumbnailMode.MusicView,
                    (uint)decodePixelSize,
                    ThumbnailOptions.None))
                {
                    if (thumbnail == null)
                    {
                        Debug.WriteLine(
                            $"[本地封面] 系统未返回缩略图：{file.Name}");

                        return null;
                    }

                    Debug.WriteLine(
                        $"[本地封面] 文件={file.Name}，" +
                        $"缩略图类型={thumbnail.Type}，" +
                        $"字节数={thumbnail.Size}");

                    if (thumbnail.Size == 0 ||
                        thumbnail.Type != ThumbnailType.Image)
                    {
                        return null;
                    }

                    var image = await LoadArtworkStreamAsync(
                        thumbnail,
                        decodePixelSize);

                    Debug.WriteLine(
                        $"[本地封面] 解码结果：" +
                        $"文件={file.Name}，" +
                        $"宽={image?.PixelWidth}，" +
                        $"高={image?.PixelHeight}");

                    return image;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("读取音乐缩略图失败：" + exception);

                return null;
            }
        }

        public async Task<BitmapImage> LoadImageFileAsync(StorageFile file, int decodePixelSize = 256)
        {
            ValidateDecodeSize(decodePixelSize);

            if (file == null)
            {
                return null;
            }

            try
            {
                using (var stream = await file.OpenReadAsync())
                {
                    return await LoadArtworkStreamAsync(stream, decodePixelSize);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("读取图片文件失败：" + exception);

                return null;
            }
        }

        public async Task<BitmapImage> LoadPackageImageAsync(string uriText, int decodePixelSize = 256)
        {
            ValidateDecodeSize(decodePixelSize);

            Uri uri;

            if (string.IsNullOrWhiteSpace(uriText) ||
                !Uri.TryCreate(uriText, UriKind.Absolute, out uri) ||
                !string.Equals(
                    uri.Scheme,
                    "ms-appx",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(uri);

                return await LoadImageFileAsync(file, decodePixelSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("读取应用图片资源失败：" + exception);

                return null;
            }
        }

        public async Task<BitmapImage> LoadArtworkStreamAsync(IRandomAccessStream stream, int decodePixelSize = 256)
        {
            ValidateDecodeSize(decodePixelSize);

            if (stream == null ||
                stream.Size == 0 ||
                stream.Size > 20UL * 1024 * 1024)
            {
                return null;
            }

            try
            {
                stream.Seek(0);

                var decoder = await BitmapDecoder.CreateAsync(stream);

                // 当前先支持静态图片。
                if (decoder.FrameCount != 1)
                {
                    return null;
                }

                var width = decoder.OrientedPixelWidth;
                var height = decoder.OrientedPixelHeight;

                if (width == 0 || height == 0 ||
                    width > 8192 || height > 8192 ||
                    (ulong)width * height > 16000000)
                {
                    return null;
                }

                var decodeWidth = width >= height
                    ? (int)Math.Min(width, (uint)decodePixelSize)
                    : 0;

                var decodeHeight = height > width
                    ? (int)Math.Min(height, (uint)decodePixelSize)
                    : 0;

                return await LoadFromStreamAsync(stream, decodeWidth, decodeHeight);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("读取封面图片流失败：" + exception);

                return null;
            }
        }

        private async Task<BitmapImage> LoadFromStreamAsync(IRandomAccessStream stream, int decodePixelWidth = 0, int decodePixelHeight = 0)
        {
            if (stream == null || stream.Size == 0)
            {
                return null;
            }

            try
            {
                stream.Seek(0);

                var image = new BitmapImage();

                if (decodePixelWidth > 0)
                {
                    image.DecodePixelType = DecodePixelType.Physical;
                    image.DecodePixelWidth = decodePixelWidth;
                }

                if (decodePixelHeight > 0)
                {
                    image.DecodePixelType = DecodePixelType.Physical;
                    image.DecodePixelHeight = decodePixelHeight;
                }

                await image.SetSourceAsync(stream);

                return image;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("解码图片失败：" + exception);

                return null;
            }
        }

        private static void ValidateDecodeSize(int size)
        {
            if (size <= 0 || size > 2048)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
        }
    }
}

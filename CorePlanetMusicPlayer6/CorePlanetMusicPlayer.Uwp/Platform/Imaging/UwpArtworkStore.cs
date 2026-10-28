using CorePlanetMusicPlayer.Core.Artwork;
using CorePlanetMusicPlayer.Services.Artwork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace CorePlanetMusicPlayer.Uwp.Platform.Imaging
{
    public sealed class UwpArtworkStore : IArtworkStore
    {
        private const string RootFolderName = "Artwork";

        private const long MaxFileBytes = 20L * 1024 * 1024;
        private const uint MaxImageDimension = 8192;
        private const ulong MaxImagePixels = 16000000;

        public async Task<string> ImportAsync(ArtworkOwnerKind ownerKind, Stream source)
        {
            // 提前检查分类是否合法。
            GetFolderName(ownerKind);

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!source.CanRead)
            {
                throw new ArgumentException("图片数据流不可读取。", nameof(source));
            }

            using (var buffer = new MemoryStream())
            {
                await CopyWithLimitAsync(source, buffer);

                buffer.Position = 0;

                using (var randomStream = buffer.AsRandomAccessStream())
                {
                    var decoder = await BitmapDecoder.CreateAsync(randomStream);

                    var extension = GetImageExtension(decoder);

                    ValidateDimensions(decoder);

                    // 实际解码一次，避免只通过文件头检查。
                    using (var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied))
                    {

                    }

                    var folder = await GetFolderAsync(ownerKind, createIfMissing: true);

                    var resourceKey = Guid.NewGuid().ToString("N") + extension;

                    StorageFile savedFile = null;

                    try
                    {
                        savedFile = await folder.CreateFileAsync(resourceKey, CreationCollisionOption.FailIfExists);

                        randomStream.Seek(0);

                        using (var output = await savedFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            await RandomAccessStream.CopyAsync(randomStream, output);

                            if (!await output.FlushAsync())
                            {
                                throw new IOException("保存图片失败。");
                            }
                        }

                        return resourceKey;
                    }
                    catch
                    {
                        // 此时尚未返回资源键，也没有建立数据库关联。
                        // 尽力清理本次未写完的文件。
                        if (savedFile != null)
                        {
                            try
                            {
                                await savedFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                            }
                            catch (Exception cleanupException)
                            {
                                Debug.WriteLine("清理未完成的图片失败：" + cleanupException);
                            }
                        }

                        throw;
                    }
                }
            }
        }

        public async Task DeleteAsync(ArtworkOwnerKind ownerKind, string resourceKey)
        {
            ValidateGeneratedResourceKey(resourceKey);

            var folder = await GetFolderAsync(ownerKind, createIfMissing: false);

            if (folder == null)
            {
                return;
            }

            var item = await folder.TryGetItemAsync(resourceKey);

            if (item == null)
            {
                return;
            }

            var file = item as StorageFile;

            if (file == null)
            {
                throw new IOException("图片资源不是文件。");
            }

            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        private static async Task CopyWithLimitAsync(Stream source, MemoryStream destination)
        {
            var bytes = new byte[64 * 1024];

            while (true)
            {
                var count = await source.ReadAsync(bytes, 0, bytes.Length);

                if (count == 0)
                {
                    break;
                }

                if (destination.Length + count > MaxFileBytes)
                {
                    throw new InvalidDataException("图片文件不能超过 20 MiB。");
                }

                await destination.WriteAsync(bytes, 0, count);
            }

            if (destination.Length == 0)
            {
                throw new InvalidDataException("图片文件为空。");
            }
        }

        private static string GetImageExtension(BitmapDecoder decoder)
        {
            var codecId = decoder.DecoderInformation.CodecId;

            if (codecId == BitmapDecoder.PngDecoderId)
            {
                return ".png";
            }

            if (codecId == BitmapDecoder.JpegDecoderId)
            {
                return ".jpg";
            }

            throw new InvalidDataException("目前仅支持 PNG 和 JPEG 图片。");
        }

        private static void ValidateDimensions(BitmapDecoder decoder)
        {
            var width = decoder.PixelWidth;
            var height = decoder.PixelHeight;

            if (width == 0 || height == 0 ||
                width > MaxImageDimension ||
                height > MaxImageDimension ||
                (ulong)width * height > MaxImagePixels)
            {
                throw new InvalidDataException("图片尺寸过大或无效，请使用单边不超过 8192 像素、总像素不超过 1600 万的图片。");
            }
        }

        private static async Task<StorageFolder> GetFolderAsync(ArtworkOwnerKind ownerKind, bool createIfMissing)
        {
            var folderName = GetFolderName(ownerKind);
            var localFolder = ApplicationData.Current.LocalFolder;

            if (createIfMissing)
            {
                var root = await localFolder.CreateFolderAsync(RootFolderName, CreationCollisionOption.OpenIfExists);

                return await root.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
            }

            var rootItem = await localFolder.TryGetItemAsync(RootFolderName);

            if (rootItem == null)
            {
                return null;
            }

            var existingRoot = rootItem as StorageFolder;

            if (existingRoot == null)
            {
                throw new IOException("图片根目录被同名文件占用。");
            }

            var folderItem = await existingRoot.TryGetItemAsync(
                folderName);

            if (folderItem == null)
            {
                return null;
            }

            var existingFolder = folderItem as StorageFolder;

            if (existingFolder == null)
            {
                throw new IOException("图片分类目录被同名文件占用。");
            }

            return existingFolder;
        }

        private static string GetFolderName(ArtworkOwnerKind ownerKind)
        {
            switch (ownerKind)
            {
                case ArtworkOwnerKind.Music:
                    return "Music";

                case ArtworkOwnerKind.Artist:
                    return "Artists";

                case ArtworkOwnerKind.Playlist:
                    return "Playlists";

                default:
                    throw new ArgumentOutOfRangeException(nameof(ownerKind));
            }
        }

        private static void ValidateGeneratedResourceKey(string resourceKey)
        {
            // 本实现只管理自己生成的“32 位 GUID + .png/.jpg”。
            // 不接受绝对路径或相对目录。
            if (string.IsNullOrWhiteSpace(resourceKey) || resourceKey.Length != 36)
            {
                throw new ArgumentException("图片资源键无效。", nameof(resourceKey));
            }

            Guid id;

            if (!Guid.TryParseExact(resourceKey.Substring(0, 32), "N", out id))
            {
                throw new ArgumentException("图片资源键无效。", nameof(resourceKey));
            }

            var extension = resourceKey.Substring(32);

            if (!string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("图片资源扩展名无效。", nameof(resourceKey));
            }
        }

        public async Task<Stream> OpenReadAsync(ArtworkOwnerKind ownerKind, string resourceKey)
        {
            ValidateGeneratedResourceKey(resourceKey);

            var folder = await GetFolderAsync(ownerKind, createIfMissing: false);

            if (folder == null)
            {
                return null;
            }

            var item = await folder.TryGetItemAsync(resourceKey);

            if (item == null)
            {
                return null;
            }

            var file = item as StorageFile;

            if (file == null)
            {
                throw new IOException("图片资源不是文件。");
            }

            return await file.OpenStreamForReadAsync();
        }
    }
}

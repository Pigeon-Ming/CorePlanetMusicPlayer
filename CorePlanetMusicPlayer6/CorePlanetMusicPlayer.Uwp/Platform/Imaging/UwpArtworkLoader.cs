using CorePlanetMusicPlayer.Core.Artwork;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Artwork;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using CorePlanetMusicPlayer.Uwp.Platform.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Uwp.Platform.Imaging
{
    public sealed partial class UwpArtworkLoader
    {
        private const string DefaultArtworkResourceName = "DefaultAlbumArtwork";
        private const string DefaultArtworkUri = "ms-appx:///Assets/DefaultAlbumArtwork.png";

        private readonly ILibraryFolderRepository _libraryFolderRepository;
        private readonly IArtworkStore _artworkStore;
        private readonly UwpRemoteArtworkLoader _remoteArtworkLoader;
        private readonly UwpStorageAccessService _storageAccessService;
        private readonly UwpThumbnailLoader _thumbnailLoader;

        public UwpArtworkLoader(
            ILibraryFolderRepository libraryFolderRepository, 
            UwpStorageAccessService storageAccessService, 
            UwpThumbnailLoader thumbnailLoader,
            IArtworkStore artworkStore,
            UwpRemoteArtworkLoader remoteArtworkLoader,
            UwpDispatcherService dispatcherService)
        {
            Guard.NotNull(libraryFolderRepository, nameof(libraryFolderRepository));

            Guard.NotNull(storageAccessService, nameof(storageAccessService));

            Guard.NotNull(thumbnailLoader, nameof(thumbnailLoader));

            Guard.NotNull(artworkStore, nameof(artworkStore));

            Guard.NotNull(remoteArtworkLoader, nameof(remoteArtworkLoader));

            Guard.NotNull(dispatcherService, nameof(dispatcherService));

            if (!dispatcherService.HasDispatcher)
            {
                throw new InvalidOperationException("图片加载器需要有效的 UI 调度器。");
            }

            _libraryFolderRepository = libraryFolderRepository;
            _storageAccessService = storageAccessService;
            _thumbnailLoader = thumbnailLoader;
            _artworkStore = artworkStore;
            _remoteArtworkLoader = remoteArtworkLoader;
            _dispatcherService = dispatcherService;
        }

        private async Task<BitmapImage> LoadSourceAsync(ArtworkReference reference, int decodePixelSize, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            BitmapImage image;

            switch (reference.SourceKind)
            {
                case ArtworkSourceKind.Default:
                    image = await LoadDefaultArtworkAsync(reference.DefaultResourceName, decodePixelSize);
                    break;

                case ArtworkSourceKind.MusicFile:
                    image = await LoadMusicFileAsync(reference, decodePixelSize, cancellationToken);
                    break;

                case ArtworkSourceKind.ManagedFile:
                    image = await LoadManagedFileAsync(reference, decodePixelSize, cancellationToken);
                    break;

                case ArtworkSourceKind.RemoteUri:
                    image = await _remoteArtworkLoader.LoadAsync(reference.RemoteUrl, decodePixelSize, cancellationToken);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(reference), "Unknown artwork source kind.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            return image;
        }

        private async Task<BitmapImage> LoadMusicFileAsync(ArtworkReference reference, int decodePixelSize, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var musicFile = await ResolveMusicFileAsync(reference);

                cancellationToken.ThrowIfCancellationRequested();

                if (musicFile == null)
                {
                    Debug.WriteLine(
                        $"[本地封面] 无法取得文件：" +
                        $"实体={reference.Owner?.Id}，" +
                        $"路径={reference.SourcePath}，" +
                        $"相对路径={reference.RelativePath}");

                    return null;
                }

                Debug.WriteLine($"[本地封面] 已取得文件：{musicFile.Name}");

                return await _thumbnailLoader.LoadMusicThumbnailAsync(musicFile, decodePixelSize);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("解析或读取音乐封面失败：" + exception);
                return null;
            }
        }

        private async Task<StorageFile> ResolveMusicFileAsync(ArtworkReference reference)
        {
            if (reference == null)
            {
                return null;
            }

            if (_storageAccessService == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(reference.LibraryFolderId))
            {
                var folderId = new LibraryFolderId(reference.LibraryFolderId);

                if (!folderId.IsEmpty && _libraryFolderRepository != null)
                {
                    var folder = await _libraryFolderRepository.GetByIdAsync(folderId);

                    if (folder != null)
                    {
                        if (folder.AccessKind == LibraryFolderAccessKind.FutureAccessList)
                        {
                            return await _storageAccessService.GetStorageFileByFutureAccessAsync(folder, reference.RelativePath);
                        }

                        if (folder.AccessKind == LibraryFolderAccessKind.DirectPath)
                        {
                            return await _storageAccessService .GetStorageFileByDirectPathAsync(folder, reference.RelativePath, reference.SourcePath);
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(reference.SourcePath))
            {
                return await _storageAccessService.GetFileFromPathAsync(reference.SourcePath);
            }

            return null;
        }

        private async Task<BitmapImage> LoadManagedFileAsync(ArtworkReference reference, int decodePixelSize, CancellationToken cancellationToken)
        {
            if (reference.Owner == null || string.IsNullOrWhiteSpace(reference.ResourceKey))
            {
                return null;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var stream = await _artworkStore.OpenReadAsync(reference.Owner.Kind, reference.ResourceKey))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (stream == null)
                    {
                        return null;
                    }

                    using (var randomStream = stream.AsRandomAccessStream())
                    {
                        return await _thumbnailLoader.LoadArtworkStreamAsync(randomStream, decodePixelSize);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("读取应用封面文件失败：" + exception);
                return null;
            }
        }

        private async Task<BitmapImage> LoadDefaultArtworkAsync(string resourceName, int decodePixelSize)
        {
            var uriText = CreateDefaultArtworkUri(resourceName);

            var image = await _thumbnailLoader.LoadPackageImageAsync(uriText, decodePixelSize);

            if (image != null)
            {
                return image;
            }

            if (!string.Equals(uriText, DefaultArtworkUri, StringComparison.OrdinalIgnoreCase))
            {
                image = await _thumbnailLoader.LoadPackageImageAsync(DefaultArtworkUri, decodePixelSize);

                if (image != null)
                {
                    return image;
                }
            }

            Debug.WriteLine("默认封面资源无法加载。");

            return null;
        }

        private static string CreateDefaultArtworkUri(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                return DefaultArtworkUri;
            }

            var name = resourceName.Trim();

            if (name.StartsWith("ms-appx:///", StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }

            if (!EndsWithImageExtension(name))
            {
                name = name + ".png";
            }

            return "ms-appx:///Assets/" + name;
        }

        private static bool EndsWithImageExtension(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || value.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || value.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase);
        }
    }
}

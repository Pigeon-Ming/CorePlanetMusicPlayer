using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Artwork;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Uwp.Platform.Imaging
{
    public sealed class UwpArtworkLoader
    {
        private const string ArtworkCacheFolderName = "artworks";
        private const string DefaultArtworkResourceName = "DefaultAlbumArtwork";
        private const string DefaultArtworkUri = "ms-appx:///Assets/DefaultAlbumArtwork.png";

        private readonly ILibraryFolderRepository _libraryFolderRepository;
        private readonly UwpStorageAccessService _storageAccessService;
        private readonly UwpThumbnailLoader _thumbnailLoader;

        public UwpArtworkLoader(ILibraryFolderRepository libraryFolderRepository, UwpStorageAccessService storageAccessService, UwpThumbnailLoader thumbnailLoader)
        {
            _libraryFolderRepository = libraryFolderRepository;
            _storageAccessService = storageAccessService;
            _thumbnailLoader = thumbnailLoader;
        }

        public async Task<BitmapImage> LoadAsync(ArtworkReference reference)
        {
            if (reference == null)
            {
                return LoadDefaultArtwork(DefaultArtworkResourceName);
            }

            if (reference.SourceKind == ArtworkSourceKind.Auto)
            {
                return await LoadAutoAsync(reference);
            }

            if (reference.SourceKind == ArtworkSourceKind.Cache)
            {
                return await LoadCacheOrDefaultAsync(reference);
            }

            if (reference.SourceKind == ArtworkSourceKind.Embedded)
            {
                return await LoadEmbeddedOrDefaultAsync(reference);
            }

            if (reference.SourceKind == ArtworkSourceKind.File)
            {
                return await LoadFileOrDefaultAsync(reference);
            }

            if (reference.SourceKind == ArtworkSourceKind.Default)
            {
                return LoadDefaultArtwork(reference.DefaultResourceName);
            }

            return LoadDefaultArtwork(DefaultArtworkResourceName);
        }

        private async Task<BitmapImage> LoadAutoAsync(
            ArtworkReference reference)
        {
            BitmapImage image = null;

            if (reference.CanUseCache)
            {
                image = await LoadCacheAsync(reference);

                if (image != null)
                {
                    return image;
                }
            }

            if (reference.CanUseEmbedded)
            {
                image = await LoadEmbeddedAsync(reference);

                if (image != null)
                {
                    return image;
                }
            }

            return LoadDefaultArtwork(reference.DefaultResourceName);
        }

        private async Task<BitmapImage> LoadCacheOrDefaultAsync(
            ArtworkReference reference)
        {
            var image = await LoadCacheAsync(reference);

            if (image != null)
            {
                return image;
            }

            return LoadDefaultArtwork(reference.DefaultResourceName);
        }

        private async Task<BitmapImage> LoadEmbeddedOrDefaultAsync(
            ArtworkReference reference)
        {
            var image = await LoadEmbeddedAsync(reference);

            if (image != null)
            {
                return image;
            }

            return LoadDefaultArtwork(reference.DefaultResourceName);
        }

        private async Task<BitmapImage> LoadFileOrDefaultAsync(
            ArtworkReference reference)
        {
            var image = await LoadFileAsync(reference);

            if (image != null)
            {
                return image;
            }

            return LoadDefaultArtwork(reference.DefaultResourceName);
        }

        private async Task<BitmapImage> LoadCacheAsync(
            ArtworkReference reference)
        {
            if (reference == null ||
                string.IsNullOrWhiteSpace(reference.CacheKey))
            {
                return null;
            }

            var cacheFile = await GetCacheFileAsync(reference.CacheKey);

            if (cacheFile == null)
            {
                return null;
            }

            return await _thumbnailLoader.LoadImageFileAsync(cacheFile);
        }

        private async Task<BitmapImage> LoadEmbeddedAsync(
            ArtworkReference reference)
        {
            var musicFile = await ResolveMusicFileAsync(reference);

            if (musicFile == null)
            {
                return null;
            }

            return await _thumbnailLoader.LoadMusicThumbnailAsync(musicFile);
        }

        private async Task<BitmapImage> LoadFileAsync(
            ArtworkReference reference)
        {
            if (reference == null ||
                string.IsNullOrWhiteSpace(reference.SourcePath))
            {
                return null;
            }

            if (_storageAccessService == null)
            {
                return null;
            }

            var file = await _storageAccessService.GetFileFromPathAsync(
                reference.SourcePath);

            if (file == null)
            {
                return null;
            }

            return await _thumbnailLoader.LoadImageFileAsync(file);
        }

        private async Task<StorageFile> ResolveMusicFileAsync(
            ArtworkReference reference)
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
                var folderId = new LibraryFolderId(
                    reference.LibraryFolderId);

                if (!folderId.IsEmpty &&
                    _libraryFolderRepository != null)
                {
                    var folder = await _libraryFolderRepository
                        .GetByIdAsync(folderId);

                    if (folder != null)
                    {
                        if (folder.AccessKind ==
                            LibraryFolderAccessKind.FutureAccessList)
                        {
                            return await _storageAccessService
                                .GetStorageFileByFutureAccessAsync(
                                    folder,
                                    reference.RelativePath);
                        }

                        if (folder.AccessKind ==
                            LibraryFolderAccessKind.DirectPath)
                        {
                            return await _storageAccessService
                                .GetStorageFileByDirectPathAsync(
                                    folder,
                                    reference.RelativePath,
                                    reference.SourcePath);
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(reference.SourcePath))
            {
                return await _storageAccessService.GetFileFromPathAsync(
                    reference.SourcePath);
            }

            return null;
        }

        private async Task<StorageFile> GetCacheFileAsync(
            string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return null;
            }

            try
            {
                var cacheFolder = ApplicationData.Current.LocalCacheFolder;
                var artworkFolder = await cacheFolder.GetFolderAsync(
                    ArtworkCacheFolderName);

                var fileName = CreateCacheFileName(cacheKey);

                return await artworkFolder.GetFileAsync(fileName);
            }
            catch
            {
                return null;
            }
        }

        private BitmapImage LoadDefaultArtwork(
            string resourceName)
        {
            var uriText = CreateDefaultArtworkUri(resourceName);
            var image = _thumbnailLoader.LoadFromUri(uriText);

            if (image != null)
            {
                return image;
            }

            image = _thumbnailLoader.LoadFromUri(DefaultArtworkUri);

            if (image != null)
            {
                return image;
            }

            return new BitmapImage();
        }

        private static string CreateDefaultArtworkUri(
            string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                return DefaultArtworkUri;
            }

            var name = resourceName.Trim();

            if (name.StartsWith(
                "ms-appx:///",
                StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }

            if (!EndsWithImageExtension(name))
            {
                name = name + ".png";
            }

            return "ms-appx:///Assets/" + name;
        }

        private static bool EndsWithImageExtension(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || value.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                || value.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase);
        }

        private static string CreateCacheFileName(
            string cacheKey)
        {
            var text = cacheKey ?? string.Empty;
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();

            for (int i = 0; i < invalidChars.Length; i++)
            {
                text = text.Replace(invalidChars[i], '_');
            }

            if (!EndsWithImageExtension(text))
            {
                text = text + ".png";
            }

            return text;
        }
    }
}

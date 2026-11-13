using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.TagLib.Uwp.Platform.Lyrics;
using CorePlanetMusicPlayer.TagLib.Uwp.Platform.Metadata;
using CorePlanetMusicPlayer.TagLib.Uwp.Platform.TagLib;
using CorePlanetMusicPlayer.Uwp.Platform.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;

namespace CorePlanetMusicPlayer.TagLib.Uwp.Composition
{
    public static class TagLibFeatureFactory
    {
        public static TagLibFeatureSet Create(
            ILibraryFolderRepository libraryFolderRepository,
            UwpStorageAccessService storageAccessService,
            string deviceFamily)
        {
            Guard.NotNull(
                libraryFolderRepository,
                nameof(libraryFolderRepository));
            Guard.NotNull(
                storageAccessService,
                nameof(storageAccessService));

            bool isPlatformSupported =
                UwpTagLibPlatformSupport.IsSupported(deviceFamily);

            var capabilities = new MusicTagCapabilities
            {
                IsTagLibExtensionInstalled = true,
                IsTagLibPlatformSupported = isPlatformSupported,
                CanReadTagsWithTagLib = isPlatformSupported,
                CanWriteTagsWithTagLib = isPlatformSupported,
                CanReadEmbeddedLyrics = isPlatformSupported,
                CanWriteEmbeddedLyrics = isPlatformSupported,
                UnavailableReason = isPlatformSupported
                    ? string.Empty
                    : "当前设备类型不支持 Taglib 扩展。"
            };

            IEmbeddedLyricAccessor embeddedLyricAccessor =
                isPlatformSupported
                    ? (IEmbeddedLyricAccessor)
                        new TagLibEmbeddedLyricAccessor(
                            libraryFolderRepository,
                            storageAccessService)
                    : new UnsupportedEmbeddedLyricAccessor(
                        capabilities.UnavailableReason);

            IUwpMusicTagAccessor musicTagAccessor =
                isPlatformSupported
                    ? (IUwpMusicTagAccessor)
                        new TagLibMusicTagAccessor()
                    : null;

            return new TagLibFeatureSet(
                embeddedLyricAccessor,
                musicTagAccessor,
                capabilities);
        }
    }
}

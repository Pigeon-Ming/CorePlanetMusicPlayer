using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Library
{
    public class LibraryFolder
    {
        public string Id { get; set; } = string.Empty;
        
        public string DisplayName { get; set; } = string.Empty;
        
        public string Path { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public LibraryFolderAccessKind AccessKind { get; set; }

        public DateTimeOffset AddedAt { get; set; }

        public bool HasAccessKey
        {
            get { return !string.IsNullOrWhiteSpace(AccessKey); }
        }

        public bool CanRestoreByAccessKey
        {
            get { return AccessKind == LibraryFolderAccessKind.FutureAccessList && HasAccessKey; }
        }

        public bool CanUseDirectPath
        {
            get { return AccessKind == LibraryFolderAccessKind.DirectPath && !string.IsNullOrWhiteSpace(Path); }
        }

        public static LibraryFolder CreateFutureAccessFolder(string displayName, string path, string accessKey)
        {
            Guard.NotNullOrWhiteSpace(accessKey, nameof(accessKey));

            return new LibraryFolder
            {
                Id = EntityId.New(),
                DisplayName = displayName ?? string.Empty,
                Path = path ?? string.Empty,
                AccessKey = accessKey,
                AccessKind = LibraryFolderAccessKind.FutureAccessList,
                AddedAt = DateTimeOffset.Now
            };
        }

        public static LibraryFolder CreateMusicLibraryFolder()
        {
            return new LibraryFolder
            {
                Id = EntityId.New(),
                DisplayName = "Music Library",
                Path = string.Empty,
                AccessKey = string.Empty,
                AccessKind = LibraryFolderAccessKind.MusicLibrary,
                AddedAt = DateTimeOffset.Now
            };
        }

        public static LibraryFolder CreateDirectPathFolder(string displayName, string path)
        {
            Guard.NotNullOrWhiteSpace(path, nameof(path));

            return new LibraryFolder
            {
                Id = EntityId.New(),
                DisplayName = displayName ?? string.Empty,
                Path = path,
                AccessKey = string.Empty,
                AccessKind = LibraryFolderAccessKind.DirectPath,
                AddedAt = DateTimeOffset.Now
            };
        }
    }
}

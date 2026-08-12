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
        public LibraryFolderId Id { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public LibraryFolderAccessKind AccessKind { get; set; }

        public DateTimeOffset AddedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public bool HasDisplayName
        {
            get { return !string.IsNullOrWhiteSpace(DisplayName); }
        }

        public bool HasPath
        {
            get { return !string.IsNullOrWhiteSpace(Path); }
        }

        public bool HasAccessKey
        {
            get { return !string.IsNullOrWhiteSpace(AccessKey); }
        }

        public bool CanRestoreByAccessKey
        {
            get { return AccessKind == LibraryFolderAccessKind.FutureAccessList && HasAccessKey; }
        }

        public bool IsSystemMusicLibrary
        {
            get { return AccessKind == LibraryFolderAccessKind.MusicLibrary; }
        }

        public bool CanUseDirectPath
        {
            get { return AccessKind == LibraryFolderAccessKind.DirectPath && HasPath; }
        }

        public static LibraryFolder CreateFutureAccessFolder(string displayName, string path, string accessKey)
        {
            Guard.NotNullOrWhiteSpace(accessKey, nameof(accessKey));

            return new LibraryFolder
            {
                Id = LibraryFolderId.NewId(),
                DisplayName = displayName ?? string.Empty,
                Path = path ?? string.Empty,
                AccessKey = accessKey,
                AccessKind = LibraryFolderAccessKind.FutureAccessList,
                AddedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        public static LibraryFolder CreateMusicLibraryFolder()
        {
            return new LibraryFolder
            {
                Id = LibraryFolderId.NewId(),
                DisplayName = "Music Library",
                Path = string.Empty,
                AccessKey = string.Empty,
                AccessKind = LibraryFolderAccessKind.MusicLibrary,
                AddedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        public static LibraryFolder CreateDirectPathFolder(string displayName, string path)
        {
            Guard.NotNullOrWhiteSpace(path, nameof(path));

            return new LibraryFolder
            {
                Id = LibraryFolderId.NewId(),
                DisplayName = displayName ?? string.Empty,
                Path = path,
                AccessKey = string.Empty,
                AccessKind = LibraryFolderAccessKind.DirectPath,
                AddedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        public void Rename(string displayName)
        {
            Guard.NotNullOrWhiteSpace(displayName, nameof(displayName));

            DisplayName = displayName;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void UpdateAccessKey(string accessKey)
        {
            Guard.NotNullOrWhiteSpace(accessKey, nameof(accessKey));
            AccessKey = accessKey;
            UpdatedAt = DateTimeOffset.Now;
        }

        public void UpdatePath(string path)
        {
            Path = path ?? string.Empty;
            UpdatedAt = DateTimeOffset.Now;
        }
    }
}

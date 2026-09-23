using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace CorePlanetMusicPlayer.Uwp.Platform.Storage
{
    public sealed class UwpStorageAccessService
    {
        private readonly UwpFolderPickerService _folderPickerService;
        private readonly UwpStorageFileMapper _mapper;

        public UwpStorageAccessService(UwpFolderPickerService folderPickerService, UwpStorageFileMapper mapper)
        {
            _folderPickerService = folderPickerService;
            _mapper = mapper ?? new UwpStorageFileMapper();
        }

        public async Task<LibraryFolder> PickerAndCreateFutureAccessFolderAsync()
        {
            var folder = await _folderPickerService.PickFolderAsync();

            if (folder == null)
            {
                return null;
            }

            var accessKey = StorageApplicationPermissions.FutureAccessList.Add(folder);

            return _mapper.ToFutureAccessLibraryFolder(folder, accessKey);
        }

        public LibraryFolder CreateDirectPathFolder(string displayName, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return LibraryFolder.CreateDirectPathFolder(displayName, path);
        }

        public async Task<StorageFolder> GetStorageFolderAsync(LibraryFolder folder)
        {
            if (folder == null)
            {
                return null;
            }

            if (folder.AccessKind == LibraryFolderAccessKind.FutureAccessList)
            {
                return await GetFolderFromFutureAccessListAsync(folder.AccessKey);
            }

            if (folder.AccessKind == LibraryFolderAccessKind.DirectPath)
            {
                return await GetFolderFromPathAsync(folder.Path);
            }

            return null;
        }

        public async Task<StorageFile> GetStorageFileAsync(Music music, LibraryFolder folder)
        {
            if (music == null || music.FileInfo == null)
            {
                return null;
            }

            if (folder == null)
            {
                return null;
            }

            if (folder.AccessKind == LibraryFolderAccessKind.FutureAccessList)
            {
                return await GetStorageFileByFutureAccessAsync(folder, music.FileInfo.RelativePath);
            }

            if (folder.AccessKind == LibraryFolderAccessKind.DirectPath)
            {
                return await GetStorageFileByDirectPathAsync(folder, music.FileInfo.RelativePath, music.FileInfo.Path);
            }

            return null;
        }

        public async Task<StorageFile> GetStorageFileByFutureAccessAsync(LibraryFolder folder, string relativePath)
        {
            var storageFolder = await GetStorageFolderAsync(folder);

            if (storageFolder == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            return await GetFileByRelativePathAsync(storageFolder, relativePath);
        }

        public async Task<StorageFile> GetStorageFileByDirectPathAsync(LibraryFolder folder, string relativePath, string fallbackPath)
        {
            if (folder == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(fallbackPath))
            {
                var fileByFullPath = await GetFileFromPathAsync(fallbackPath);

                if (fileByFullPath != null)
                {
                    return fileByFullPath;
                }
            }

            if (!string.IsNullOrWhiteSpace(folder.Path) && !string.IsNullOrWhiteSpace(relativePath))
            {
                var fullPath = CombinePath(folder.Path, relativePath);

                return await GetFileFromPathAsync(fullPath);
            }


            return null;
        }

        private async Task<StorageFolder> GetFolderFromFutureAccessListAsync(string accessKey)
        {
            if (string.IsNullOrWhiteSpace(accessKey))
            {
                return null;
            }

            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(accessKey))
            {
                return null;
            }

            try
            {
                return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(accessKey);
            }
            catch
            {
                return null;
            }
        }

        public async Task<StorageFile> GetFileByRelativePathAsync(StorageFolder rootFolder, string relativePath)
        {
            if (rootFolder == null || string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            try
            {
                var parts = relativePath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length == 0)
                {
                    return null;
                }

                StorageFolder currentFolder = rootFolder;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    currentFolder = await currentFolder.GetFolderAsync(parts[i]);
                }

                return await currentFolder.GetFileAsync(parts[parts.Length - 1]);
            }
            catch
            {
                return null;
            }
        }

        public async Task<StorageFolder> GetFolderFromPathAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                return await StorageFolder.GetFolderFromPathAsync(NormalizePath(path));
            }
            catch
            {
                return null;
            }
        }

        public async Task<StorageFile> GetFileFromPathAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                return await StorageFile.GetFileFromPathAsync(NormalizePath(path));
            }
            catch
            {
                return null;
            }
        }

        public bool HasFutureAccess(LibraryFolder folder)
        {
            if (folder == null)
            {
                return false;
            }

            if (folder.AccessKind != LibraryFolderAccessKind.FutureAccessList)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(folder.AccessKey))
            {
                return false;
            }

            return StorageApplicationPermissions
                .FutureAccessList
                .ContainsItem(folder.AccessKey);
        }

        public void RemoveFutureAccess(LibraryFolder folder)
        {
            if (folder == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(folder.AccessKey))
            {
                return;
            }

            if (!StorageApplicationPermissions
                .FutureAccessList
                .ContainsItem(folder.AccessKey))
            {
                return;
            }

            StorageApplicationPermissions
                .FutureAccessList
                .Remove(folder.AccessKey);
        }

        private static string CombinePath(string rootPath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                return relativePath ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return rootPath ?? string.Empty;
            }

            return NormalizePath(rootPath).TrimEnd('\\') + "\\" + NormalizePath(relativePath).TrimStart('\\');
        }

        private static string NormalizePath(string path)
        {
            if (path == null)
            {
                return string.Empty;
            }

            return path.Trim().Replace("/", "\\");
        }
    }
}

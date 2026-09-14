using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Services.Library;
using CorePlanetMusicPlayer.Uwp.Platform.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Uwp.Platform.Library
{
    public sealed class UwpLibraryScanner : ILibraryScanner
    {
        private readonly UwpStorageAccessService _storageAccessService;
        private readonly UwpStorageFileMapper _mapper;
        private readonly UwpMusicFileReader _musicFileReader;

        public UwpLibraryScanner(UwpStorageAccessService storageAccessService, UwpStorageFileMapper mapper, UwpMusicFileReader musicFileReader)
        {
            if (storageAccessService == null)
            {
                throw new ArgumentNullException(
                    nameof(storageAccessService));
            }

            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (musicFileReader == null)
            {
                throw new ArgumentNullException(nameof(musicFileReader));
            }

            _storageAccessService = storageAccessService;
            _mapper = mapper;
            _musicFileReader = musicFileReader;
        }

        public async Task<LibraryScanResult> ScanAsync(
            LibraryFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            if (folder.Id.IsEmpty)
            {
                throw new ArgumentException("Library folder id cannot be empty.",nameof(folder));
            }

            var collectedMusic = new List<Music>();

            try
            {
                var rootFolder = await _storageAccessService.GetStorageFolderAsync(folder);

                if (rootFolder == null)
                {
                    return LibraryScanResult.Incomplete(collectedMusic, $"无法访问音乐来源“{folder.DisplayName}”，" + "目录可能已移动或授权已失效。");
                }

                await ScanFolderAsync(rootFolder, folder, string.Empty, collectedMusic);

                return LibraryScanResult.Completed(collectedMusic);
            }
            catch (Exception ex)
            {
                return LibraryScanResult.Incomplete(collectedMusic, $"扫描“{folder.DisplayName}”失败：{ex.Message}");
            }
        }

        private async Task ScanFolderAsync(StorageFolder storageFolder, LibraryFolder libraryFolder, string relativeFolderPath, List<Music> collectedMusic)
        {
            IReadOnlyList<StorageFile> files;
            IReadOnlyList<StorageFolder> childFolders;

            string folderDescription = string.IsNullOrEmpty(relativeFolderPath) ? "来源根目录" : relativeFolderPath;

            try
            {
                files = await storageFolder.GetFilesAsync();
                childFolders = await storageFolder.GetFoldersAsync();
            }
            catch (Exception ex)
            {
                throw new IOException($"枚举目录“{folderDescription}”失败：{ex.Message}", ex);
            }

            foreach (var file in files)
            {
                if (!_mapper.IsSupportedMusicFile(file))
                {
                    continue;
                }

                string relativeFilePath = CombineRelativePath(relativeFolderPath, file.Name);

                try
                {
                    var music = await _musicFileReader.ReadAsync(file, libraryFolder, relativeFilePath);

                    if (music == null)
                    {
                        throw new InvalidOperationException("单文件读取器没有返回歌曲。");
                    }

                    collectedMusic.Add(music);
                }
                catch (Exception ex)
                {
                    throw new IOException($"读取文件“{relativeFilePath}”失败：{ex.Message}", ex);
                }
            }

            foreach (var childFolder in childFolders)
            {
                string childRelativePath = CombineRelativePath(relativeFolderPath, childFolder.Name);

                await ScanFolderAsync(childFolder, libraryFolder, childRelativePath, collectedMusic);
            }
        }

        private static string CombineRelativePath(string parentPath, string name)
        {
            return string.IsNullOrEmpty(parentPath) ? name : parentPath + "\\" + name;
        }
    }
}

using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace CorePlanetMusicPlayer.Uwp.Platform.Storage
{

    /// <summary>
    /// 负责将UWP文件对象转换成Core里的纯数据
    /// </summary>
    public sealed class UwpStorageFileMapper
    {
        public LibraryFolder ToFutureAccessLibraryFolder(StorageFolder folder, string accessKey)
        {
            if (folder == null)
            {
                return null;
            }

            return LibraryFolder.CreateFutureAccessFolder(folder.DisplayName, folder.Path, accessKey);
        }

        public LibraryFolder ToDirectPathLibraryFolder(string displayName, string path)
        {
            return LibraryFolder.CreateDirectPathFolder(displayName, path);
        }

        public MusicFileInfo ToMusicFileInfo(StorageFile file, LibraryFolder libraryFolder, string relativePath, BasicProperties properties)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (libraryFolder == null)
            {
                throw new ArgumentNullException(nameof(libraryFolder));
            }

            if (libraryFolder.Id.IsEmpty)
            {
                throw new ArgumentException(
                    "Library folder id cannot be empty.",
                    nameof(libraryFolder));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            return new MusicFileInfo
            {
                Path = file.Path ?? string.Empty,
                RelativePath = NormalizeRelativePath(relativePath),
                FileName = file.Name,
                Extension = NormalizeExtension(file.FileType),
                Size = checked((long)properties.Size),
                LastModifiedAt = properties.DateModified,
                LibraryFolderId = libraryFolder.Id.ToString()
            };
        }

        private string GetRelativePath(string rootPath, string filePath)
        {
            if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }

            var normalizedRoot = NormalizePath(rootPath);
            var normalizedFile = NormalizePath(filePath);

            if (!normalizedFile.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var relativePath = normalizedFile.Substring(normalizedRoot.Length);

            while (relativePath.StartsWith("\\") || relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Substring(1);
            }

            return relativePath;
        }

        private static string NormalizeRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException(
                    "Relative path cannot be empty.",
                    nameof(relativePath));
            }

            string normalized = relativePath.Replace('/', '\\');

            if (normalized.StartsWith("\\") || normalized.Contains(":"))
            {
                throw new ArgumentException(
                    "An absolute path is not allowed.",
                    nameof(relativePath));
            }

            var parts = normalized.Split('\\');

            foreach (string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part) ||
                    part == "." ||
                    part == "..")
                {
                    throw new ArgumentException(
                        "Relative path contains an invalid segment.",
                        nameof(relativePath));
                }
            }

            return normalized;
        }

        public string NormalizeExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return string.Empty;
            }

            extension = extension.Trim().ToLowerInvariant();

            if (!extension.StartsWith("."))
            {
                extension = "."  + extension;
            }

            return extension;
        }

        private static string NormalizePath(string path)
        {
            if (path == null)
            {
                return string.Empty;
            }

            return path.Trim().Replace("/", "\\");
        }

        public bool IsSupportedMusicFile(StorageFile file)
        {
            if (file == null)
            {
                return false;
            }

            var extension = NormalizeExtension(file.FileType);

            return extension == ".mp3" || extension == ".flac" || extension == ".wav" || extension == ".m4a" || extension == ".aac" || extension == ".wma" || extension == ".ogg";
        }
    }
}

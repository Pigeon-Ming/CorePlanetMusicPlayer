using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Uwp.Platform.Lyrics
{
    public sealed class UwpExternalLyricReader : IExternalLyricReader
    {
        private const string FileNamePlaceholder = "{FileName}";

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

        private readonly ILibraryFolderRepository _libraryFolderRepository;
        private readonly UwpStorageAccessService _storageAccessService;

        public UwpExternalLyricReader(
            ILibraryFolderRepository libraryFolderRepository,
            UwpStorageAccessService storageAccessService)
        {
            _libraryFolderRepository = libraryFolderRepository ??
                throw new ArgumentNullException(nameof(libraryFolderRepository));
            _storageAccessService = storageAccessService ??
                throw new ArgumentNullException(nameof(storageAccessService));
        }

        public async Task<ExternalLyricReadResult> FindAsync(Music music, ExternalLyricSearchOptions options)
        {
            if (!CanReadExternalLyrics(music))
            {
                return ExternalLyricReadResult.NotFound();
            }

            try
            {
                StorageFile musicFile = await ResolveMusicFileAsync(music);

                if (musicFile == null)
                {
                    return ExternalLyricReadResult.Failure(
                        "无法访问音乐文件，不能查找同目录歌词。");
                }

                StorageFolder parentFolder = await musicFile.GetParentAsync();

                if (parentFolder == null)
                {
                    return ExternalLyricReadResult.Failure(
                        "无法访问音乐文件所在目录。");
                }

                IReadOnlyList<StorageFile> files = await parentFolder.GetFilesAsync();
                List<StorageFile> lyricFiles = GetLyricFiles(files);

                StorageFile lyricFile = FindExactMatch(lyricFiles, musicFile.DisplayName);

                if (lyricFile == null)
                {
                    string fileNameRegex = options == null
                        ? string.Empty
                        : options.FileNameRegex;

                    ExternalLyricReadResult matchResult = FindRegexMatch(
                        lyricFiles,
                        musicFile.DisplayName,
                        fileNameRegex,
                        out lyricFile);

                    if (matchResult != null)
                    {
                        return matchResult;
                    }
                }

                if (lyricFile == null)
                {
                    return ExternalLyricReadResult.NotFound();
                }

                string rawText = await ReadTextAsync(lyricFile);
                var properties = await lyricFile.GetBasicPropertiesAsync();
                string relativePath = CreateRelativePath(music.FileInfo.RelativePath, lyricFile.Name);

                return ExternalLyricReadResult.Success(
                    lyricFile.Name,
                    relativePath,
                    rawText,
                    properties.DateModified);
            }
            catch (DecoderFallbackException)
            {
                return ExternalLyricReadResult.Failure(
                    "外部歌词不是受支持的 Unicode 或 UTF-8 编码。");
            }
            catch (UnauthorizedAccessException)
            {
                return ExternalLyricReadResult.Failure(
                    "没有权限读取音乐文件所在目录。");
            }
            catch (Exception ex)
            {
                return ExternalLyricReadResult.Failure(
                    "读取外部歌词失败：" + ex.Message);
            }
        }

        private async Task<StorageFile> ResolveMusicFileAsync(Music music)
        {
            var folderId = new LibraryFolderId(music.FileInfo.LibraryFolderId);

            if (folderId.IsEmpty)
            {
                return null;
            }

            LibraryFolder folder = await _libraryFolderRepository.GetByIdAsync(folderId);

            if (folder == null)
            {
                return null;
            }

            return await _storageAccessService.GetStorageFileAsync(music, folder);
        }

        private static bool CanReadExternalLyrics(Music music)
        {
            return music != null &&
                   (music.SourceType == MusicSourceType.Local ||
                    music.SourceType == MusicSourceType.Temporary) &&
                   music.FileInfo != null &&
                   music.FileInfo.HasLibraryFolder &&
                   music.FileInfo.HasRelativePath;
        }

        private static List<StorageFile> GetLyricFiles(IReadOnlyList<StorageFile> files)
        {
            var result = new List<StorageFile>();

            if (files == null)
            {
                return result;
            }

            for (int index = 0; index < files.Count; index++)
            {
                StorageFile file = files[index];

                if (file != null &&
                    string.Equals(
                        file.FileType,
                        ".lrc",
                        StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(file);
                }
            }

            result.Sort((left, right) => string.Compare(
                left?.Name,
                right?.Name,
                StringComparison.OrdinalIgnoreCase));

            return result;
        }

        private static StorageFile FindExactMatch(IEnumerable<StorageFile> files, string musicFileName)
        {
            if (files == null)
            {
                return null;
            }

            foreach (StorageFile file in files)
            {
                if (file != null &&
                    string.Equals(
                        file.DisplayName,
                        musicFileName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return file;
                }
            }

            return null;
        }

        private static ExternalLyricReadResult FindRegexMatch(
            IEnumerable<StorageFile> files,
            string musicFileName,
            string fileNameRegex,
            out StorageFile match)
        {
            match = null;

            if (files == null || string.IsNullOrWhiteSpace(fileNameRegex))
            {
                return null;
            }

            try
            {
                string pattern = fileNameRegex.Trim().Replace(
                    FileNamePlaceholder,
                    Regex.Escape(musicFileName ?? string.Empty));

                var regex = new Regex(
                    pattern,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                    RegexTimeout);

                foreach (StorageFile file in files)
                {
                    if (file != null && regex.IsMatch(file.Name))
                    {
                        match = file;
                        return null;
                    }
                }

                return null;
            }
            catch (ArgumentException ex)
            {
                return ExternalLyricReadResult.Failure(
                    "外部歌词文件名正则表达式无效：" + ex.Message);
            }
            catch (RegexMatchTimeoutException)
            {
                return ExternalLyricReadResult.Failure(
                    "外部歌词文件名正则表达式执行超时。");
            }
        }

        private static async Task<string> ReadTextAsync(StorageFile file)
        {
            var utf8 = new UTF8Encoding(false, true);

            using (Stream stream = await file.OpenStreamForReadAsync())
            using (var reader = new StreamReader(
                stream,
                utf8,
                true))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private static string CreateRelativePath(string musicRelativePath, string lyricFileName)
        {
            string normalizedPath = (musicRelativePath ?? string.Empty)
                .Trim()
                .Replace('/', '\\');
            int separatorIndex = normalizedPath.LastIndexOf('\\');

            if (separatorIndex < 0)
            {
                return lyricFileName ?? string.Empty;
            }

            return normalizedPath.Substring(0, separatorIndex + 1) +
                   (lyricFileName ?? string.Empty);
        }
    }
}

using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace CorePlanetMusicPlayer.Uwp.Platform.Lyrics
{
    public sealed class UwpExternalLyricWriter : IExternalLyricWriter
    {
        private readonly ILibraryFolderRepository _libraryFolderRepository;
        private readonly UwpStorageAccessService _storageAccessService;

        public UwpExternalLyricWriter(
            ILibraryFolderRepository libraryFolderRepository,
            UwpStorageAccessService storageAccessService)
        {
            _libraryFolderRepository = libraryFolderRepository ??
                throw new ArgumentNullException(
                    nameof(libraryFolderRepository));
            _storageAccessService = storageAccessService ??
                throw new ArgumentNullException(nameof(storageAccessService));
        }

        public async Task<ExternalLyricWriteResult> WriteAsync(
            Music music,
            string relativePath,
            string rawText)
        {
            ExternalLyricWriteResult validationResult = Validate(music, relativePath, rawText);

            if (validationResult != null)
            {
                return validationResult;
            }

            StorageFile temporaryFile = null;
            bool originalFileReplaced = false;

            try
            {
                StorageFolder rootFolder = await ResolveLibraryFolderAsync(music);

                if (rootFolder == null)
                {
                    return ExternalLyricWriteResult.Failure(
                        ExternalLyricWriteStatus.AccessDenied,
                        "无法访问音乐库目录，目录授权可能已经失效。");
                }

                StorageFile originalFile =
                    await _storageAccessService.GetFileByRelativePathAsync(
                        rootFolder,
                        relativePath);

                if (originalFile == null)
                {
                    return ExternalLyricWriteResult.Failure(
                        ExternalLyricWriteStatus.NotFound,
                        "原外部歌词文件已经不存在。");
                }

                if (!string.Equals(
                    originalFile.FileType,
                    ".lrc",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return ExternalLyricWriteResult.Failure(
                        ExternalLyricWriteStatus.Unsupported,
                        "当前外部歌词文件不是 LRC 文件。");
                }

                StorageFolder parentFolder = await originalFile.GetParentAsync();

                if (parentFolder == null)
                {
                    return ExternalLyricWriteResult.Failure(
                        ExternalLyricWriteStatus.AccessDenied,
                        "无法访问外部歌词所在目录。");
                }

                temporaryFile = await parentFolder.CreateFileAsync(
                    CreateTemporaryFileName(originalFile),
                    CreationCollisionOption.FailIfExists);

                await FileIO.WriteTextAsync(
                    temporaryFile,
                    rawText,
                    UnicodeEncoding.Utf8);

                string temporaryRawText = await FileIO.ReadTextAsync(
                    temporaryFile,
                    UnicodeEncoding.Utf8);

                if (!Matches(rawText, temporaryRawText))
                {
                    return ExternalLyricWriteResult.Failure(
                        ExternalLyricWriteStatus.Failed,
                        "写入临时歌词文件后校验失败，原文件未修改。");
                }

                await temporaryFile.MoveAndReplaceAsync(originalFile);
                originalFileReplaced = true;

                StorageFile replacedFile = await parentFolder.GetFileAsync(
                    originalFile.Name);
                string finalRawText = await FileIO.ReadTextAsync(
                    replacedFile,
                    UnicodeEncoding.Utf8);

                if (!Matches(rawText, finalRawText))
                {
                    return ExternalLyricWriteResult.Failure(
                        ExternalLyricWriteStatus.Failed,
                        "替换外部歌词文件后校验失败。");
                }

                return ExternalLyricWriteResult.Success();
            }
            catch (FileNotFoundException)
            {
                return ExternalLyricWriteResult.Failure(
                    ExternalLyricWriteStatus.NotFound,
                    "原外部歌词文件已经不存在。");
            }
            catch (UnauthorizedAccessException)
            {
                return ExternalLyricWriteResult.Failure(
                    ExternalLyricWriteStatus.AccessDenied,
                    "没有权限写入外部歌词文件。");
            }
            catch (Exception ex)
            {
                return ExternalLyricWriteResult.Failure(
                    GetFailureStatus(ex),
                    "写入外部歌词失败：" + ex.Message);
            }
            finally
            {
                if (temporaryFile != null && !originalFileReplaced)
                {
                    await DeleteTemporaryFileAsync(temporaryFile);
                }
            }
        }

        private async Task<StorageFolder> ResolveLibraryFolderAsync(Music music)
        {
            var folderId = new LibraryFolderId(
                music.FileInfo.LibraryFolderId);

            if (folderId.IsEmpty)
            {
                return null;
            }

            LibraryFolder folder = await _libraryFolderRepository.GetByIdAsync(folderId);

            if (folder == null)
            {
                return null;
            }

            return await _storageAccessService.GetStorageFolderAsync(folder);
        }

        private static ExternalLyricWriteResult Validate(
            Music music,
            string relativePath,
            string rawText)
        {
            if (music == null || music.FileInfo == null)
            {
                return ExternalLyricWriteResult.Failure(
                    ExternalLyricWriteStatus.Unsupported,
                    "音乐文件信息为空。");
            }

            if (music.SourceType != MusicSourceType.Local &&
                music.SourceType != MusicSourceType.Temporary)
            {
                return ExternalLyricWriteResult.Failure(
                    ExternalLyricWriteStatus.Unsupported,
                    "只有本地音乐支持写回外部歌词。");
            }

            if (string.IsNullOrWhiteSpace(
                music.FileInfo.LibraryFolderId))
            {
                return ExternalLyricWriteResult.Failure(
                    ExternalLyricWriteStatus.AccessDenied,
                    "音乐文件没有关联已授权的音乐库目录。");
            }

            if (!IsSafeRelativePath(relativePath))
            {
                return ExternalLyricWriteResult.Failure(
                    ExternalLyricWriteStatus.Unsupported,
                    "外部歌词相对路径无效。");
            }

            if (string.IsNullOrWhiteSpace(rawText))
            {
                return ExternalLyricWriteResult.Failure(
                    ExternalLyricWriteStatus.Failed,
                    "歌词内容不能为空。");
            }

            return null;
        }

        private static bool IsSafeRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) ||
                Path.IsPathRooted(relativePath))
            {
                return false;
            }

            string[] parts = relativePath.Split(
                new[] { '\\', '/' },
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "." || parts[i] == "..")
                {
                    return false;
                }
            }

            return string.Equals(
                Path.GetExtension(parts[parts.Length - 1]),
                ".lrc",
                StringComparison.OrdinalIgnoreCase);
        }

        private static string CreateTemporaryFileName(StorageFile file)
        {
            return file.DisplayName +
                ".lyrics-" +
                Guid.NewGuid().ToString("N") +
                ".tmp";
        }

        private static bool Matches(string expected, string actual)
        {
            return string.Equals(
                NormalizeForComparison(expected),
                NormalizeForComparison(actual),
                StringComparison.Ordinal);
        }

        private static string NormalizeForComparison(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .TrimStart('\uFEFF');
        }

        private static ExternalLyricWriteStatus GetFailureStatus(Exception exception)
        {
            const int SharingViolation = 32;
            const int LockViolation = 33;

            int errorCode = exception.HResult & 0xFFFF;

            if (errorCode == SharingViolation ||
                errorCode == LockViolation)
            {
                return ExternalLyricWriteStatus.Busy;
            }

            return ExternalLyricWriteStatus.Failed;
        }

        private static async Task DeleteTemporaryFileAsync(StorageFile temporaryFile)
        {
            try
            {
                await temporaryFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch
            {
                // 临时文件清理失败不能覆盖原始写入错误。
            }
        }
    }
}

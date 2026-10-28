using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Uwp.Platform.Lyrics
{
    public sealed class UwpEmbeddedLyricAccessor : IEmbeddedLyricAccessor
    {
        private readonly ILibraryFolderRepository _libraryFolderRepository;
        private readonly UwpStorageAccessService _storageAccessService;

        public UwpEmbeddedLyricAccessor(
            ILibraryFolderRepository libraryFolderRepository,
            UwpStorageAccessService storageAccessService)
        {
            _libraryFolderRepository = libraryFolderRepository ??
                throw new ArgumentNullException(
                    nameof(libraryFolderRepository));
            _storageAccessService = storageAccessService ??
                throw new ArgumentNullException(nameof(storageAccessService));
        }

        public async Task<EmbeddedLyricReadResult> ReadAsync(Music music)
        {
            EmbeddedLyricReadResult validationResult =
                ValidateForRead(music);

            if (validationResult != null)
            {
                return validationResult;
            }

            try
            {
                StorageFile file = await ResolveStorageFileAsync(music);

                if (file == null)
                {
                    return EmbeddedLyricReadResult.Failure(
                        EmbeddedLyricAccessStatus.AccessDenied,
                        "无法访问音乐文件，目录授权可能已经失效。");
                }

                return await ReadStorageFileAsync(file);
            }
            catch (UnauthorizedAccessException)
            {
                return EmbeddedLyricReadResult.Failure(
                    EmbeddedLyricAccessStatus.AccessDenied,
                    "没有权限读取音乐文件中的内嵌歌词。");
            }
            catch (TagLib.UnsupportedFormatException)
            {
                return EmbeddedLyricReadResult.Failure(
                    EmbeddedLyricAccessStatus.Unsupported,
                    "当前音频格式不支持读取内嵌歌词。");
            }
            catch (TagLib.CorruptFileException ex)
            {
                return EmbeddedLyricReadResult.Failure(
                    EmbeddedLyricAccessStatus.Failed,
                    "音频文件或标签数据已损坏：" + ex.Message);
            }
            catch (NotSupportedException ex)
            {
                return EmbeddedLyricReadResult.Failure(
                    EmbeddedLyricAccessStatus.Unsupported,
                    "当前音频格式不支持读取内嵌歌词：" + ex.Message);
            }
            catch (Exception ex)
            {
                return EmbeddedLyricReadResult.Failure(
                    GetFailureStatus(ex),
                    "读取内嵌歌词失败：" + ex.Message);
            }
        }

        public async Task<EmbeddedLyricWriteResult> WriteAsync(Music music, string rawText)
        {
            EmbeddedLyricWriteResult validationResult =
                ValidateForWrite(music, rawText);

            if (validationResult != null)
            {
                return validationResult;
            }

            StorageFile temporaryFile = null;
            bool originalFileReplaced = false;

            try
            {
                StorageFile originalFile =
                    await ResolveStorageFileAsync(music);

                if (originalFile == null)
                {
                    return EmbeddedLyricWriteResult.Failure(
                        EmbeddedLyricAccessStatus.AccessDenied,
                        "无法访问音乐文件，目录授权可能已经失效。");
                }

                StorageFolder parentFolder = await originalFile.GetParentAsync();

                if (parentFolder == null)
                {
                    return EmbeddedLyricWriteResult.Failure(
                        EmbeddedLyricAccessStatus.AccessDenied,
                        "无法访问音乐文件所在目录。");
                }

                temporaryFile = await originalFile.CopyAsync(
                    parentFolder,
                    CreateTemporaryFileName(originalFile),
                    NameCollisionOption.GenerateUniqueName);

                await WriteStorageFileAsync(temporaryFile, rawText);

                EmbeddedLyricReadResult temporaryReadResult = await ReadStorageFileAsync(temporaryFile);

                if (!Matches(rawText, temporaryReadResult))
                {
                    return EmbeddedLyricWriteResult.Failure(
                        EmbeddedLyricAccessStatus.Unsupported,
                        "写入临时音频文件后未能读取到相同的歌词，原文件未修改。");
                }

                await temporaryFile.MoveAndReplaceAsync(originalFile);
                originalFileReplaced = true;

                StorageFile replacedFile = await parentFolder.GetFileAsync( originalFile.Name);
                EmbeddedLyricReadResult finalReadResult = await ReadStorageFileAsync(replacedFile);

                if (!Matches(rawText, finalReadResult))
                {
                    return EmbeddedLyricWriteResult.Failure(
                        EmbeddedLyricAccessStatus.Failed,
                        "替换原音频文件后，内嵌歌词校验失败。");
                }

                return EmbeddedLyricWriteResult.Success();
            }
            catch (UnauthorizedAccessException)
            {
                return EmbeddedLyricWriteResult.Failure(
                    EmbeddedLyricAccessStatus.AccessDenied,
                    "没有权限写入音乐文件中的内嵌歌词。");
            }
            catch (TagLib.UnsupportedFormatException)
            {
                return EmbeddedLyricWriteResult.Failure(
                    EmbeddedLyricAccessStatus.Unsupported,
                    "当前音频格式不支持写入内嵌歌词。");
            }
            catch (TagLib.CorruptFileException ex)
            {
                return EmbeddedLyricWriteResult.Failure(
                    EmbeddedLyricAccessStatus.Failed,
                    "音频文件或标签数据已损坏：" + ex.Message);
            }
            catch (NotSupportedException ex)
            {
                return EmbeddedLyricWriteResult.Failure(
                    EmbeddedLyricAccessStatus.Unsupported,
                    "当前音频格式不支持写入内嵌歌词：" + ex.Message);
            }
            catch (Exception ex)
            {
                return EmbeddedLyricWriteResult.Failure(
                    GetFailureStatus(ex),
                    "写入内嵌歌词失败：" + ex.Message);
            }
            finally
            {
                if (temporaryFile != null && !originalFileReplaced)
                {
                    await DeleteTemporaryFileAsync(temporaryFile);
                }
            }
        }

        private async Task<StorageFile> ResolveStorageFileAsync(Music music)
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

        private static async Task<EmbeddedLyricReadResult> ReadStorageFileAsync(StorageFile file)
        {
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                string rawText = await Task.Run(
                    () => ReadLyrics(file.Name, stream));

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    return EmbeddedLyricReadResult.NotFound();
                }

                return EmbeddedLyricReadResult.Success(rawText);
            }
        }

        private static async Task WriteStorageFileAsync(StorageFile file, string rawText)
        {
            using (Stream stream = await file.OpenStreamForWriteAsync())
            {
                await Task.Run(
                    () => WriteLyrics(file.Name, stream, rawText));
                await stream.FlushAsync();
            }
        }

        private static string ReadLyrics(string fileName, Stream stream)
        {
            var abstraction = new UwpTagLibFileAbstraction(fileName, stream);

            using (TagLib.File tagFile = TagLib.File.Create(abstraction, TagLib.ReadStyle.None))
            {
                return tagFile.Tag.Lyrics ?? string.Empty;
            }
        }

        private static void WriteLyrics(string fileName, Stream stream, string rawText)
        {
            var abstraction = new UwpTagLibFileAbstraction(fileName, stream);

            using (TagLib.File tagFile = TagLib.File.Create(abstraction, TagLib.ReadStyle.None))
            {
                tagFile.Tag.Lyrics = rawText;
                tagFile.Save();
            }
        }

        private static EmbeddedLyricReadResult ValidateForRead(Music music)
        {
            string errorMessage = ValidateMusic(music);

            if (errorMessage == null)
            {
                return null;
            }

            return EmbeddedLyricReadResult.Failure(
                EmbeddedLyricAccessStatus.Unsupported,
                errorMessage);
        }

        private static EmbeddedLyricWriteResult ValidateForWrite(Music music, string rawText)
        {
            string errorMessage = ValidateMusic(music);

            if (errorMessage != null)
            {
                return EmbeddedLyricWriteResult.Failure(
                    EmbeddedLyricAccessStatus.Unsupported,
                    errorMessage);
            }

            if (string.IsNullOrWhiteSpace(rawText))
            {
                return EmbeddedLyricWriteResult.Failure(
                    EmbeddedLyricAccessStatus.Failed,
                    "歌词内容不能为空。");
            }

            return null;
        }

        private static string ValidateMusic(Music music)
        {
            if (music == null)
            {
                return "音乐不能为空。";
            }

            if (music.SourceType != MusicSourceType.Local &&
                music.SourceType != MusicSourceType.Temporary)
            {
                return "只有本地音乐文件支持内嵌歌词。";
            }

            if (music.FileInfo == null)
            {
                return "音乐文件信息为空。";
            }

            if (string.IsNullOrWhiteSpace(
                music.FileInfo.LibraryFolderId))
            {
                return "音乐文件没有关联已授权的音乐库目录。";
            }

            return null;
        }

        private static bool Matches(
            string expectedRawText,
            EmbeddedLyricReadResult actualResult)
        {
            if (actualResult == null || !actualResult.Found)
            {
                return false;
            }

            return string.Equals(
                NormalizeForComparison(expectedRawText),
                NormalizeForComparison(actualResult.RawText),
                StringComparison.Ordinal);
        }

        private static string NormalizeForComparison(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .TrimEnd('\0');
        }

        private static string CreateTemporaryFileName(StorageFile file)
        {
            return file.DisplayName +
                ".lyrics-" +
                Guid.NewGuid().ToString("N") +
                file.FileType;
        }

        private static EmbeddedLyricAccessStatus GetFailureStatus(Exception exception)
        {
            const int SharingViolation = 32;
            const int LockViolation = 33;

            int errorCode = exception.HResult & 0xFFFF;

            if (errorCode == SharingViolation ||
                errorCode == LockViolation)
            {
                return EmbeddedLyricAccessStatus.Busy;
            }

            return EmbeddedLyricAccessStatus.Failed;
        }

        private static async Task DeleteTemporaryFileAsync(StorageFile temporaryFile)
        {
            try
            {
                await temporaryFile.DeleteAsync(
                    StorageDeleteOption.PermanentDelete);
            }
            catch
            {
                // 临时文件清理失败不能覆盖原始写入错误。
            }
        }
    }
}

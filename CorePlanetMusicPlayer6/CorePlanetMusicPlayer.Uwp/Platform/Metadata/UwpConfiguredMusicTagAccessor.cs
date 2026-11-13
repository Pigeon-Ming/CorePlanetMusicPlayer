using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Services.Settings;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Uwp.Platform.Metadata
{
    /// <summary>
    /// 根据全局设置和运行能力选择歌曲标签读写引擎。
    /// </summary>
    public sealed class UwpConfiguredMusicTagAccessor
        : IUwpMusicTagAccessor
    {
        private readonly IUwpMusicTagAccessor _windowsAccessor;
        private readonly IUwpMusicTagAccessor _tagLibAccessor;
        private readonly MusicTagCapabilities _capabilities;
        private readonly ISettingsService _settingsService;

        public UwpConfiguredMusicTagAccessor(
            IUwpMusicTagAccessor windowsAccessor,
            IUwpMusicTagAccessor tagLibAccessor,
            MusicTagCapabilities capabilities,
            ISettingsService settingsService)
        {
            Guard.NotNull(windowsAccessor, nameof(windowsAccessor));
            Guard.NotNull(capabilities, nameof(capabilities));
            Guard.NotNull(settingsService, nameof(settingsService));

            _windowsAccessor = windowsAccessor;
            _tagLibAccessor = tagLibAccessor;
            _capabilities = capabilities;
            _settingsService = settingsService;
        }

        public async Task<MusicTagReadResult> ReadAsync(
            StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            MusicTagEngine engine = await GetConfiguredEngineAsync();

            if (!CanUseTagLib())
            {
                return await _windowsAccessor.ReadAsync(file);
            }

            switch (engine)
            {
                case MusicTagEngine.TagLib:
                    return await _tagLibAccessor.ReadAsync(file);

                case MusicTagEngine.Hybrid:
                    return await ReadHybridAsync(file);

                default:
                    return await _windowsAccessor.ReadAsync(file);
            }
        }

        public async Task<Result> WriteAsync(
            StorageFile file,
            MusicMetadataUpdateRequest request)
        {
            if (file == null)
            {
                return Result.Failure("音乐文件不能为空。");
            }

            if (request == null || !request.HasAnyChange)
            {
                return Result.Failure(
                    "没有需要写入的标签。");
            }

            MusicTagEngine engine = await GetConfiguredEngineAsync();

            if ((engine == MusicTagEngine.TagLib ||
                 engine == MusicTagEngine.Hybrid) &&
                !CanUseTagLibForWrite())
            {
                engine = MusicTagEngine.WindowsApi;
            }

            if (engine == MusicTagEngine.WindowsApi &&
                request.HasDiscNumber)
            {
                return Result.Failure(
                    "Windows API 标签引擎不支持写入碟片号。");
            }

            if (engine == MusicTagEngine.WindowsApi &&
                request.HasComment)
            {
                return Result.Failure(
                    "Windows API 标签引擎不支持写入备注。" +
                    "请使用 TagLib 或混合引擎。");
            }

            StorageFile temporaryFile = null;
            bool originalFileReplaced = false;

            try
            {
                StorageFolder parentFolder =
                    await file.GetParentAsync();

                if (parentFolder == null)
                {
                    return Result.Failure(
                        "无法访问音乐文件所在目录。");
                }

                string originalFileName = file.Name;

                temporaryFile = await file.CopyAsync(
                    parentFolder,
                    CreateTemporaryFileName(file),
                    NameCollisionOption.GenerateUniqueName);

                Result writeResult = await WriteTemporaryFileAsync(
                    temporaryFile,
                    request,
                    engine);

                if (writeResult == null || writeResult.IsFailure)
                {
                    return writeResult ?? Result.Failure(
                        "标签访问器没有返回写入结果。");
                }

                StorageFile temporaryVerificationFile =
                    await ReopenFileAsync(temporaryFile);

                MusicTagReadResult temporaryReadResult =
                    await ReadWithEngineAsync(
                        temporaryVerificationFile,
                        engine);

                string temporaryMismatch =
                    MusicMetadataWriteVerifier
                        .GetMismatchDescription(
                            request,
                            temporaryReadResult == null
                                ? null
                                : temporaryReadResult.Metadata);

                if (!string.IsNullOrEmpty(temporaryMismatch))
                {
                    return Result.Failure(
                        "写入临时文件后的标签校验失败，" +
                        "原文件未修改：" +
                        temporaryMismatch);
                }

                await temporaryFile.MoveAndReplaceAsync(file);
                originalFileReplaced = true;

                StorageFile replacedFile =
                    await parentFolder.GetFileAsync(
                        originalFileName);

                MusicTagReadResult finalReadResult =
                    await ReadWithEngineAsync(
                        replacedFile,
                        engine);

                string finalMismatch =
                    MusicMetadataWriteVerifier
                        .GetMismatchDescription(
                            request,
                            finalReadResult == null
                                ? null
                                : finalReadResult.Metadata);

                if (!string.IsNullOrEmpty(finalMismatch))
                {
                    return Result.Failure(
                        "替换原文件后的标签校验失败：" +
                        finalMismatch);
                }

                return Result.Success();
            }
            catch (UnauthorizedAccessException)
            {
                return Result.Failure(
                    "没有权限写入音乐文件标签。");
            }
            catch (Exception ex)
            {
                return Result.Failure(
                    IsFileBusy(ex)
                        ? "音乐文件正在使用，暂时无法写入标签。"
                        : "写入音乐文件标签失败：" + ex.Message);
            }
            finally
            {
                if (temporaryFile != null &&
                    !originalFileReplaced)
                {
                    await DeleteTemporaryFileAsync(
                        temporaryFile);
                }
            }
        }

        private async Task<Result> WriteTemporaryFileAsync(
            StorageFile file,
            MusicMetadataUpdateRequest request,
            MusicTagEngine engine)
        {
            switch (engine)
            {
                case MusicTagEngine.TagLib:
                    return await _tagLibAccessor.WriteAsync(
                        file,
                        request);

                case MusicTagEngine.Hybrid:
                    MusicMetadataUpdateRequest windowsRequest =
                        CreateWindowsRequest(request);

                    if (windowsRequest.HasAnyChange)
                    {
                        Result windowsResult =
                            await _windowsAccessor.WriteAsync(
                                file,
                                windowsRequest);

                        if (windowsResult == null ||
                            windowsResult.IsFailure)
                        {
                            return windowsResult ?? Result.Failure(
                                "Windows API 标签访问器没有返回写入结果。");
                        }
                    }

                    MusicMetadataUpdateRequest tagLibRequest =
                        CreateTagLibRequest(request);

                    if (tagLibRequest.HasAnyChange)
                    {
                        Result tagLibResult =
                            await _tagLibAccessor.WriteAsync(
                                file,
                                tagLibRequest);

                        if (tagLibResult == null ||
                            tagLibResult.IsFailure)
                        {
                            return tagLibResult ?? Result.Failure(
                                "TagLib 标签访问器没有返回写入结果。");
                        }
                    }

                    return Result.Success();

                default:
                    return await _windowsAccessor.WriteAsync(
                        file,
                        request);
            }
        }

        private async Task<MusicTagReadResult> ReadWithEngineAsync(
            StorageFile file,
            MusicTagEngine engine)
        {
            switch (engine)
            {
                case MusicTagEngine.TagLib:
                    return await _tagLibAccessor.ReadAsync(file);

                case MusicTagEngine.Hybrid:
                    return await ReadHybridAsync(file);

                default:
                    return await _windowsAccessor.ReadAsync(file);
            }
        }

        private async Task<MusicTagReadResult> ReadHybridAsync(
            StorageFile file)
        {
            MusicTagReadResult windowsResult =
                await _windowsAccessor.ReadAsync(file);

            try
            {
                MusicTagReadResult tagLibResult =
                    await _tagLibAccessor.ReadAsync(file);

                if (tagLibResult != null &&
                    tagLibResult.Metadata != null)
                {
                    windowsResult.Metadata.DiscNumber =
                        tagLibResult.Metadata.DiscNumber;
                    windowsResult.Metadata.Comment =
                        tagLibResult.Metadata.Comment;
                }
            }
            catch (Exception)
            {
                // 混合引擎中 TagLib 负责增强碟片号和备注，失败时保留 Windows API 结果。
            }

            return windowsResult;
        }

        private async Task<MusicTagEngine> GetConfiguredEngineAsync()
        {
            try
            {
                AppSettings settings =
                    await _settingsService.LoadAsync();

                if (settings == null || settings.Metadata == null)
                {
                    return MusicTagEngine.WindowsApi;
                }

                MusicTagEngine engine = settings.Metadata.TagEngine;

                return Enum.IsDefined(
                    typeof(MusicTagEngine),
                    engine)
                    ? engine
                    : MusicTagEngine.WindowsApi;
            }
            catch (Exception)
            {
                return MusicTagEngine.WindowsApi;
            }
        }

        private static MusicMetadataUpdateRequest CreateWindowsRequest(
            MusicMetadataUpdateRequest source)
        {
            MusicMetadataUpdateRequest result =
                MusicMetadataUpdateRequest.ForMusic(source.MusicId);

            if (source.HasTitle)
            {
                result.WithTitle(source.Title);
            }

            if (source.HasArtistNames)
            {
                result.WithArtistNames(source.ArtistNames);
            }

            if (source.HasAlbumTitle)
            {
                result.WithAlbumTitle(source.AlbumTitle);
            }

            if (source.HasAlbumArtistName)
            {
                result.WithAlbumArtistName(
                    source.AlbumArtistName);
            }

            if (source.HasGenre)
            {
                result.WithGenre(source.Genre);
            }

            if (source.HasYear)
            {
                result.WithYear(source.Year);
            }

            if (source.HasTrackNumber)
            {
                result.WithTrackNumber(source.TrackNumber);
            }

            if (source.HasComposer)
            {
                result.WithComposer(source.Composer);
            }

            return result;
        }

        private static MusicMetadataUpdateRequest CreateTagLibRequest(
            MusicMetadataUpdateRequest source)
        {
            MusicMetadataUpdateRequest result =
                MusicMetadataUpdateRequest.ForMusic(source.MusicId);

            if (source.HasDiscNumber)
            {
                result.WithDiscNumber(source.DiscNumber);
            }

            if (source.HasComment)
            {
                result.WithComment(source.Comment);
            }

            return result;
        }

        private static string CreateTemporaryFileName(
            StorageFile file)
        {
            return file.DisplayName +
                   ".metadata-" +
                   Guid.NewGuid().ToString("N") +
                   file.FileType;
        }

        private static async Task<StorageFile> ReopenFileAsync(
            StorageFile file)
        {
            StorageFolder parentFolder =
                await file.GetParentAsync();

            if (parentFolder == null)
            {
                return file;
            }

            return await parentFolder.GetFileAsync(file.Name);
        }

        private static bool IsFileBusy(Exception exception)
        {
            const int SharingViolation = 32;
            const int LockViolation = 33;

            int errorCode = exception.HResult & 0xFFFF;

            return errorCode == SharingViolation ||
                   errorCode == LockViolation;
        }

        private static async Task DeleteTemporaryFileAsync(
            StorageFile temporaryFile)
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

        private bool CanUseTagLib()
        {
            return _tagLibAccessor != null &&
                   _capabilities.IsTagLibAvailable &&
                   _capabilities.CanReadTagsWithTagLib;
        }

        private bool CanUseTagLibForWrite()
        {
            return _tagLibAccessor != null &&
                   _capabilities.IsTagLibAvailable &&
                   _capabilities.CanWriteTagsWithTagLib;
        }
    }
}

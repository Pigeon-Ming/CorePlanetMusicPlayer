using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Uwp.Platform.Metadata
{
    public sealed class UwpMusicMetadataWriter : IMusicMetadataWriter
    {
        private readonly ILibraryFolderRepository _libraryFolderRepository;
        private readonly UwpStorageAccessService _storageAccessService;
        private readonly IUwpMusicTagAccessor _musicTagAccessor;

        public UwpMusicMetadataWriter(
            ILibraryFolderRepository libraryFolderRepository,
            UwpStorageAccessService storageAccessService,
            IUwpMusicTagAccessor musicTagAccessor)
        {
            Guard.NotNull(libraryFolderRepository, nameof(libraryFolderRepository));
            Guard.NotNull(storageAccessService, nameof(storageAccessService));
            Guard.NotNull(musicTagAccessor, nameof(musicTagAccessor));

            _libraryFolderRepository = libraryFolderRepository;
            _storageAccessService = storageAccessService;
            _musicTagAccessor = musicTagAccessor;
        }

        public async Task<Result> WriteAsync(Music music, MusicMetadataUpdateRequest request)
        {
            var validationResult = Validate(music, request);

            if (validationResult.IsFailure)
            {
                return validationResult;
            }

            try
            {
                var file = await ResolveStorageFileAsync(music);

                if (file == null)
                {
                    return Result.Failure("无法访问音乐文件，可能是文件已移动、目录授权失效，或应用没有文件访问权限。");
                }

                return await _musicTagAccessor.WriteAsync(
                    file,
                    request);
            }
            catch (UnauthorizedAccessException)
            {
                return Result.Failure("没有权限写入该音乐文件。请重新添加音乐目录，或检查应用的文件系统访问权限。");
            }
            catch (Exception ex)
            {
                return Result.Failure("写入或核对音乐文件元数据失败：" + ex.Message);
            }
        }

        private async Task<StorageFile> ResolveStorageFileAsync(Music music)
        {
            if (music == null || music.FileInfo == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(music.FileInfo.LibraryFolderId))
            {
                return null;
            }

            var folderId = new LibraryFolderId(music.FileInfo.LibraryFolderId);

            if (folderId.IsEmpty)
            {
                return null;
            }

            var folder = await _libraryFolderRepository.GetByIdAsync(folderId);

            if (folder == null)
            {
                return null;
            }

            return await _storageAccessService.GetStorageFileAsync(music, folder);
        }

        private static Result Validate(Music music, MusicMetadataUpdateRequest request)
        {
            if (music == null)
            {
                return Result.Failure("音乐不能为空。");
            }

            if (music.Id.IsEmpty)
            {
                return Result.Failure("音乐 ID 不能为空。");
            }

            if (music.SourceType != MusicSourceType.Local &&
                music.SourceType != MusicSourceType.Temporary)
            {
                return Result.Failure("只有本地音乐文件支持写入文件元数据。");
            }

            if (music.FileInfo == null)
            {
                return Result.Failure("音乐文件信息为空。");
            }

            if (request == null)
            {
                return Result.Failure("元数据修改请求不能为空。");
            }

            if (request.MusicId.IsEmpty)
            {
                return Result.Failure("元数据修改请求中的音乐 ID 不能为空。");
            }

            if (request.MusicId != music.Id)
            {
                return Result.Failure("元数据修改请求与当前音乐不匹配。");
            }

            if (!request.HasAnyChange)
            {
                return Result.Failure("没有需要写入的元数据。");
            }

            return Result.Success();
        }

    }
}

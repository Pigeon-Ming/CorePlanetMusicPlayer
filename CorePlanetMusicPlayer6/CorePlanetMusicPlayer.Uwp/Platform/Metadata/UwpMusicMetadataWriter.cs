using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace CorePlanetMusicPlayer.Uwp.Platform.Metadata
{
    public sealed class UwpMusicMetadataWriter : IMusicMetadataWriter
    {
        private readonly ILibraryFolderRepository _libraryFolderRepository;
        private readonly UwpStorageAccessService _storageAccessService;

        public UwpMusicMetadataWriter(ILibraryFolderRepository libraryFolderRepository, UwpStorageAccessService storageAccessService)
        {
            Guard.NotNull(libraryFolderRepository, nameof(libraryFolderRepository));
            Guard.NotNull(storageAccessService, nameof(storageAccessService));

            _libraryFolderRepository = libraryFolderRepository;
            _storageAccessService = storageAccessService;
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

                var propertiesToSave = CreatePropertiesToSave(request);

                if (propertiesToSave.Count > 0)
                {
                    await file.Properties.SavePropertiesAsync(propertiesToSave);
                }

                if (request.HasArtistNames)
                {
                    bool matches = await VerifyArtistNamesAsync(file, request.ArtistNames);

                    if (!matches)
                    {
                        return Result.Failure("文件写入后读取到的艺术家列表与请求不一致，数据库未更新。文件可能已经发生修改，请检查文件标签后重新扫描。");
                    }
                }

                return Result.Success();
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

        private static Dictionary<string, object> CreatePropertiesToSave(MusicMetadataUpdateRequest request)
        {
            var values = new Dictionary<string, object>();

            if (request.HasTitle)
            {
                values["System.Title"] = NormalizeText(request.Title);
            }

            if (request.HasArtistNames)
            {
                values["System.Music.Artist"] = request.ArtistNames.ToArray();
            }

            if (request.HasAlbumTitle)
            {
                values["System.Music.AlbumTitle"] = NormalizeText(request.AlbumTitle);
            }

            if (request.HasAlbumArtistName)
            {
                values["System.Music.AlbumArtist"] = NormalizeText(request.AlbumArtistName);
            }

            if (request.HasYear)
            {
                values["System.Media.Year"] = NormalizeUInt(request.Year);
            }

            if (request.HasTrackNumber)
            {
                values["System.Music.TrackNumber"] = NormalizeUInt(request.TrackNumber);
            }

            if (request.HasGenre)
            {
                values["System.Music.Genre"] = CreateSingleValueArray(request.Genre);
            }

            if (request.HasComposer)
            {
                values["System.Music.Composer"] = CreateSingleValueArray(request.Composer);
            }

            // 沿用当前行为：DiscNumber、Comment 暂时只更新数据库。
            return values;
        }

        private static string[] CreateSingleValueArray(string value)
        {
            string normalized = NormalizeText(value);

            return normalized.Length == 0
                ? new string[0]
                : new[] { normalized };
        }

        private static async Task<bool> VerifyArtistNamesAsync(StorageFile file, IReadOnlyList<string> expectedNames)
        {
            const string propertyName = "System.Music.Artist";

            var values = await file.Properties.RetrievePropertiesAsync(new[] { propertyName });

            object rawValue;
            IEnumerable<string> names;

            if (!values.TryGetValue(propertyName, out rawValue) || rawValue == null)
            {
                names = new string[0];
            }
            else if (rawValue is string)
            {
                names = new[] { (string)rawValue };
            }
            else
            {
                names = rawValue as IEnumerable<string>;

                if (names == null)
                {
                    return false;
                }
            }

            var actualNames = ArtistNameNormalizer.Normalize(names);

            return actualNames.SequenceEqual(expectedNames, StringComparer.Ordinal);
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

        private static string NormalizeText(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Trim();
        }

        private static uint NormalizeUInt(int? value)
        {
            if (!value.HasValue)
            {
                return 0;
            }

            if (value.Value < 0)
            {
                return 0;
            }

            return (uint)value.Value;
        }

        private static void SetSingleValueList(IList<string> values, string value)
        {
            if (values == null)
            {
                return;
            }

            values.Clear();

            var normalizedValue = NormalizeText(value);

            if (!string.IsNullOrWhiteSpace(normalizedValue))
            {
                values.Add(normalizedValue);
            }
        }
    }
}

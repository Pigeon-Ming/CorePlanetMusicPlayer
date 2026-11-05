using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Services.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace CorePlanetMusicPlayer.Uwp.Platform.Metadata
{
    /// <summary>
    /// 使用 Windows API 读取音乐文件标签。
    /// </summary>
    public sealed class UwpWindowsMusicTagAccessor
        : IUwpMusicTagAccessor
    {
        public async Task<MusicTagReadResult> ReadAsync(
            StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            MusicProperties properties =
                await file.Properties.GetMusicPropertiesAsync();

            List<string> artistNames =
                await ReadArtistNamesAsync(properties);

            string comment =
                await ReadCommentAsync(properties);

            var metadata = new MusicMetadata
            {
                Title = NormalizeText(properties.Title),
                ArtistName = string.Join("; ", artistNames),
                ArtistNames = artistNames,
                AlbumTitle = NormalizeText(properties.Album),
                AlbumArtistName =
                    NormalizeText(properties.AlbumArtist),
                Genre = GetFirstValue(properties.Genre),
                Year = ToNullableInt(properties.Year),
                TrackNumber = ToNullableInt(properties.TrackNumber),
                DiscNumber = null,
                Composer = JoinValues(properties.Composers),
                Comment = comment
            };

            return new MusicTagReadResult(
                metadata,
                properties.Duration);
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

            if (request.HasDiscNumber)
            {
                return Result.Failure(
                    "Windows API 标签引擎不支持写入碟片号。");
            }

            try
            {
                Dictionary<string, object> values =
                    CreatePropertiesToSave(request);

                if (values.Count > 0)
                {
                    await file.Properties.SavePropertiesAsync(values);
                }

                MusicTagReadResult readResult =
                    await ReadAsync(file);

                return Matches(request, readResult.Metadata)
                    ? Result.Success()
                    : Result.Failure(
                        "Windows API 写入后的标签校验失败。");
            }
            catch (UnauthorizedAccessException)
            {
                return Result.Failure(
                    "没有权限写入音乐文件标签。");
            }
            catch (Exception ex)
            {
                return Result.Failure(
                    "Windows API 写入音乐文件标签失败：" +
                    ex.Message);
            }
        }

        private static async Task<List<string>> ReadArtistNamesAsync(
            MusicProperties properties)
        {
            const string propertyName = "System.Music.Artist";

            var values = await properties.RetrievePropertiesAsync(
                new[] { propertyName });

            object rawValue;

            if (!values.TryGetValue(propertyName, out rawValue) ||
                rawValue == null)
            {
                return new List<string>();
            }

            var names = rawValue as IEnumerable<string>;

            if (names != null)
            {
                return ArtistNameNormalizer.Normalize(names);
            }

            var singleName = rawValue as string;

            if (singleName != null)
            {
                return ArtistNameNormalizer.Normalize(
                    new[] { singleName });
            }

            throw new InvalidOperationException(
                "艺术家属性的数据类型不受支持。");
        }

        private static async Task<string> ReadCommentAsync(
            MusicProperties properties)
        {
            const string propertyName = "System.Comment";

            var values = await properties.RetrievePropertiesAsync(
                new[] { propertyName });

            object rawValue;

            if (!values.TryGetValue(propertyName, out rawValue) ||
                rawValue == null)
            {
                return string.Empty;
            }

            return NormalizeText(rawValue.ToString());
        }

        private static Dictionary<string, object>
            CreatePropertiesToSave(
                MusicMetadataUpdateRequest request)
        {
            var values = new Dictionary<string, object>();

            if (request.HasTitle)
            {
                values["System.Title"] =
                    NormalizeText(request.Title);
            }

            if (request.HasArtistNames)
            {
                values["System.Music.Artist"] =
                    request.ArtistNames.ToArray();
            }

            if (request.HasAlbumTitle)
            {
                values["System.Music.AlbumTitle"] =
                    NormalizeText(request.AlbumTitle);
            }

            if (request.HasAlbumArtistName)
            {
                values["System.Music.AlbumArtist"] =
                    NormalizeText(request.AlbumArtistName);
            }

            if (request.HasGenre)
            {
                values["System.Music.Genre"] =
                    CreateSingleValueArray(request.Genre);
            }

            if (request.HasYear)
            {
                values["System.Media.Year"] =
                    NormalizeUInt(request.Year);
            }

            if (request.HasTrackNumber)
            {
                values["System.Music.TrackNumber"] =
                    NormalizeUInt(request.TrackNumber);
            }

            if (request.HasComposer)
            {
                values["System.Music.Composer"] =
                    CreateSingleValueArray(request.Composer);
            }

            if (request.HasComment)
            {
                values["System.Comment"] =
                    NormalizeText(request.Comment);
            }

            return values;
        }

        private static bool Matches(
            MusicMetadataUpdateRequest request,
            MusicMetadata actual)
        {
            if (actual == null)
            {
                return false;
            }

            if (request.HasTitle &&
                NormalizeText(actual.Title) !=
                NormalizeText(request.Title))
            {
                return false;
            }

            if (request.HasArtistNames)
            {
                List<string> expectedNames =
                    ArtistNameNormalizer.Normalize(
                        request.ArtistNames);
                List<string> actualNames =
                    ArtistNameNormalizer.Normalize(
                        actual.ArtistNames ??
                        new List<string>());

                if (!actualNames.SequenceEqual(
                    expectedNames,
                    StringComparer.Ordinal))
                {
                    return false;
                }
            }

            return (!request.HasAlbumTitle ||
                    NormalizeText(actual.AlbumTitle) ==
                    NormalizeText(request.AlbumTitle)) &&
                   (!request.HasAlbumArtistName ||
                    NormalizeText(actual.AlbumArtistName) ==
                    NormalizeText(request.AlbumArtistName)) &&
                   (!request.HasGenre ||
                    NormalizeText(actual.Genre) ==
                    NormalizeText(request.Genre)) &&
                   (!request.HasYear ||
                    actual.Year == request.Year) &&
                   (!request.HasTrackNumber ||
                    actual.TrackNumber == request.TrackNumber) &&
                   (!request.HasComposer ||
                    NormalizeText(actual.Composer) ==
                    NormalizeText(request.Composer)) &&
                   (!request.HasComment ||
                    NormalizeText(actual.Comment) ==
                    NormalizeText(request.Comment));
        }

        private static string[] CreateSingleValueArray(
            string value)
        {
            string normalized = NormalizeText(value);

            return normalized.Length == 0
                ? new string[0]
                : new[] { normalized };
        }

        private static uint NormalizeUInt(int? value)
        {
            if (!value.HasValue || value.Value <= 0)
            {
                return 0;
            }

            return (uint)value.Value;
        }

        private static string NormalizeText(string value)
        {
            return value == null
                ? string.Empty
                : value.Trim();
        }

        private static int? ToNullableInt(uint value)
        {
            if (value == 0 || value > int.MaxValue)
            {
                return null;
            }

            return (int)value;
        }

        private static string GetFirstValue(
            IEnumerable<string> values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            return values
                .Select(NormalizeText)
                .FirstOrDefault(value => value.Length > 0) ??
                string.Empty;
        }

        private static string JoinValues(
            IEnumerable<string> values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            return string.Join(
                "; ",
                values
                    .Select(NormalizeText)
                    .Where(value => value.Length > 0));
        }
    }
}

using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.TagLib.Uwp.Platform.TagLib;
using CorePlanetMusicPlayer.Uwp.Platform.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.TagLib.Uwp.Platform.Metadata
{
    /// <summary>
    /// 使用 TagLibSharp 读取音乐文件标签。
    /// </summary>
    public sealed class TagLibMusicTagAccessor
        : IUwpMusicTagAccessor
    {
        public async Task<MusicTagReadResult> ReadAsync(
            StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            try
            {
                using (Stream stream =
                    await file.OpenStreamForReadAsync())
                {
                    return await Task.Run(
                        () => ReadTags(file.Name, stream));
                }
            }
            catch (global::TagLib.UnsupportedFormatException ex)
            {
                throw new NotSupportedException(
                    "当前音频格式不支持 TagLib 标签读取。",
                    ex);
            }
            catch (global::TagLib.CorruptFileException ex)
            {
                throw new InvalidDataException(
                    "音频文件或标签数据已损坏：" +
                    ex.Message,
                    ex);
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

            try
            {
                using (Stream stream =
                    await file.OpenStreamForWriteAsync())
                {
                    await Task.Run(
                        () => WriteTags(
                            file.Name,
                            stream,
                            request));

                    await stream.FlushAsync();
                }

                return Result.Success();
            }
            catch (UnauthorizedAccessException)
            {
                return Result.Failure(
                    "没有权限写入音乐文件标签。");
            }
            catch (global::TagLib.UnsupportedFormatException)
            {
                return Result.Failure(
                    "当前音频格式不支持 TagLib 标签写入。");
            }
            catch (global::TagLib.CorruptFileException ex)
            {
                return Result.Failure(
                    "音频文件或标签数据已损坏：" + ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure(
                    "TagLib 写入音乐文件标签失败：" +
                    ex.Message);
            }
        }

        private static MusicTagReadResult ReadTags(
            string fileName,
            Stream stream)
        {
            var abstraction = new UwpTagLibFileAbstraction(
                fileName,
                stream);

            using (global::TagLib.File tagFile =
                global::TagLib.File.Create(
                    abstraction,
                    global::TagLib.ReadStyle.Average))
            {
                global::TagLib.Tag tag = tagFile.Tag;

                List<string> artistNames =
                    ArtistNameNormalizer.Normalize(
                        tag.Performers ?? new string[0]);

                var metadata = new MusicMetadata
                {
                    Title = NormalizeText(tag.Title),
                    ArtistName = string.Join("; ", artistNames),
                    ArtistNames = artistNames,
                    AlbumTitle = NormalizeText(tag.Album),
                    AlbumArtistName = JoinValues(tag.AlbumArtists),
                    Genre = GetFirstValue(tag.Genres),
                    Year = ToNullableInt(tag.Year),
                    TrackNumber = ToNullableInt(tag.Track),
                    DiscNumber = ToNullableInt(tag.Disc),
                    Composer = JoinValues(tag.Composers),
                    Comment = NormalizeText(tag.Comment)
                };

                TimeSpan duration =
                    tagFile.Properties == null
                        ? TimeSpan.Zero
                        : tagFile.Properties.Duration;

                if (duration < TimeSpan.Zero)
                {
                    duration = TimeSpan.Zero;
                }

                return new MusicTagReadResult(
                    metadata,
                    duration);
            }
        }

        private static void WriteTags(
            string fileName,
            Stream stream,
            MusicMetadataUpdateRequest request)
        {
            var abstraction = new UwpTagLibFileAbstraction(
                fileName,
                stream);

            using (global::TagLib.File tagFile =
                global::TagLib.File.Create(
                    abstraction,
                    global::TagLib.ReadStyle.None))
            {
                global::TagLib.Tag tag = tagFile.Tag;

                if (request.HasTitle)
                {
                    tag.Title = NormalizeText(request.Title);
                }

                if (request.HasArtistNames)
                {
                    tag.Performers = request.ArtistNames.ToArray();
                }

                if (request.HasAlbumTitle)
                {
                    tag.Album = NormalizeText(request.AlbumTitle);
                }

                if (request.HasAlbumArtistName)
                {
                    tag.AlbumArtists = CreateSingleValueArray(
                        request.AlbumArtistName);
                }

                if (request.HasGenre)
                {
                    tag.Genres = CreateSingleValueArray(
                        request.Genre);
                }

                if (request.HasYear)
                {
                    tag.Year = NormalizeUInt(request.Year);
                }

                if (request.HasTrackNumber)
                {
                    tag.Track = NormalizeUInt(
                        request.TrackNumber);
                }

                if (request.HasDiscNumber)
                {
                    tag.Disc = NormalizeUInt(
                        request.DiscNumber);
                }

                if (request.HasComposer)
                {
                    tag.Composers = CreateSingleValueArray(
                        request.Composer);
                }

                if (request.HasComment)
                {
                    tag.Comment = NormalizeText(request.Comment);
                }

                tagFile.Save();
            }
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
    }
}

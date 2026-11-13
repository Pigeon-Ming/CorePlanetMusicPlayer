using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Services.Metadata;
using System;
using System.Collections.Generic;

namespace CorePlanetMusicPlayer.Uwp.Platform.Metadata
{
    /// <summary>
    /// 比较标签写入请求与重新读取的音乐元数据。
    /// </summary>
    internal static class MusicMetadataWriteVerifier
    {
        public static string GetMismatchDescription(
            MusicMetadataUpdateRequest request,
            MusicMetadata actual)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (actual == null)
            {
                return "标签读取结果为空";
            }

            var mismatches = new List<string>();

            if (request.HasTitle)
            {
                AddTextMismatch(
                    mismatches,
                    "标题",
                    request.Title,
                    actual.Title);
            }

            if (request.HasArtistNames)
            {
                string expectedArtists =
                    NormalizeArtistNames(request.ArtistNames);
                string actualArtists =
                    NormalizeArtistNames(GetArtistNames(actual));

                AddNormalizedMismatch(
                    mismatches,
                    "艺术家",
                    expectedArtists,
                    actualArtists);
            }

            if (request.HasAlbumTitle)
            {
                AddTextMismatch(
                    mismatches,
                    "专辑",
                    request.AlbumTitle,
                    actual.AlbumTitle);
            }

            if (request.HasAlbumArtistName)
            {
                AddTextMismatch(
                    mismatches,
                    "专辑艺术家",
                    request.AlbumArtistName,
                    actual.AlbumArtistName);
            }

            if (request.HasGenre)
            {
                AddTextMismatch(
                    mismatches,
                    "流派",
                    request.Genre,
                    actual.Genre);
            }

            if (request.HasYear)
            {
                AddNumberMismatch(
                    mismatches,
                    "年份",
                    request.Year,
                    actual.Year);
            }

            if (request.HasTrackNumber)
            {
                AddNumberMismatch(
                    mismatches,
                    "音轨号",
                    request.TrackNumber,
                    actual.TrackNumber);
            }

            if (request.HasDiscNumber)
            {
                AddNumberMismatch(
                    mismatches,
                    "碟片号",
                    request.DiscNumber,
                    actual.DiscNumber);
            }

            if (request.HasComposer)
            {
                AddTextMismatch(
                    mismatches,
                    "作曲家",
                    request.Composer,
                    actual.Composer);
            }

            if (request.HasComment)
            {
                AddTextMismatch(
                    mismatches,
                    "备注",
                    request.Comment,
                    actual.Comment);
            }

            return string.Join("；", mismatches);
        }

        private static IEnumerable<string> GetArtistNames(
            MusicMetadata metadata)
        {
            if (metadata.ArtistNames != null &&
                metadata.ArtistNames.Count > 0)
            {
                return metadata.ArtistNames;
            }

            return string.IsNullOrWhiteSpace(metadata.ArtistName)
                ? new string[0]
                : new[] { metadata.ArtistName };
        }

        private static string NormalizeArtistNames(
            IEnumerable<string> names)
        {
            return string.Join(
                "; ",
                ArtistNameNormalizer.Normalize(
                    names ?? new string[0]));
        }

        private static void AddTextMismatch(
            List<string> mismatches,
            string fieldName,
            string expected,
            string actual)
        {
            AddNormalizedMismatch(
                mismatches,
                fieldName,
                NormalizeText(expected),
                NormalizeText(actual));
        }

        private static void AddNormalizedMismatch(
            List<string> mismatches,
            string fieldName,
            string expected,
            string actual)
        {
            if (string.Equals(
                expected,
                actual,
                StringComparison.Ordinal))
            {
                return;
            }

            mismatches.Add(
                fieldName +
                "（期望：" + FormatValue(expected) +
                "，实际：" + FormatValue(actual) + "）");
        }

        private static void AddNumberMismatch(
            List<string> mismatches,
            string fieldName,
            int? expected,
            int? actual)
        {
            if (expected == actual)
            {
                return;
            }

            mismatches.Add(
                fieldName +
                "（期望：" + FormatNumber(expected) +
                "，实际：" + FormatNumber(actual) + "）");
        }

        private static string NormalizeText(string value)
        {
            return value == null
                ? string.Empty
                : value.Trim();
        }

        private static string FormatValue(string value)
        {
            string normalized = NormalizeText(value)
                .Replace("\r", " ")
                .Replace("\n", " ");

            if (normalized.Length == 0)
            {
                return "（空）";
            }

            const int MaximumLength = 80;

            return normalized.Length <= MaximumLength
                ? normalized
                : normalized.Substring(0, MaximumLength) + "…";
        }

        private static string FormatNumber(int? value)
        {
            return value.HasValue
                ? value.Value.ToString()
                : "（空）";
        }
    }
}

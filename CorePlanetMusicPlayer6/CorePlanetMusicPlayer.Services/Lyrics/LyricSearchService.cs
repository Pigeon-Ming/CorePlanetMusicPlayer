using CorePlanetMusicPlayer.Core.Lyrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class LyricSearchService
    {
        public LyricDocument SelectPreferred(IEnumerable<LyricDocument> documents)
        {
            IReadOnlyList<LyricDocument> orderedDocuments = OrderByPreference(documents);

            return orderedDocuments.Count == 0
                ? null
                : orderedDocuments[0];
        }

        public IReadOnlyList<LyricDocument> OrderByPreference(IEnumerable<LyricDocument> documents)
        {
            var candidates = new List<LyricDocument>();

            if (documents == null)
            {
                return candidates.AsReadOnly();
            }

            foreach (LyricDocument document in documents)
            {
                if (IsSelectable(document))
                {
                    candidates.Add(document);
                }
            }

            candidates.Sort(CompareDocuments);

            var result = new List<LyricDocument>();

            for (int i = 0; i < candidates.Count; i++)
            {
                LyricDocument candidate = candidates[i];

                if (ContainsSameSource(result, candidate))
                {
                    System.Diagnostics.Debug.WriteLine(
                        "发现重复歌词来源，已使用更新时间较新的记录：" +
                        GetSourceDescription(candidate));
                    continue;
                }

                result.Add(candidate);
            }

            return result.AsReadOnly();
        }

        public LyricLine FindCurrentLine(IEnumerable<LyricLine> lines, TimeSpan position)
        {
            if (lines == null)
            {
                return null;
            }

            if (position < TimeSpan.Zero)
            {
                position = TimeSpan.Zero;
            }

            LyricLine current = null;

            foreach (var line in lines)
            {
                if (line == null)
                {
                    continue;
                }

                if (line.Time <= position)
                {
                    if (current == null || line.Time >= current.Time)
                    {
                        current = line;
                    }
                }
            }

            return current;
        }

        private static bool IsSelectable(LyricDocument document)
        {
            if (document == null ||
                !document.HasRawText ||
                !document.HasLines)
            {
                return false;
            }

            if (document.SourceType != LyricSourceType.ExternalFile &&
                document.SourceType != LyricSourceType.Embedded &&
                document.SourceType != LyricSourceType.Online)
            {
                return false;
            }

            return document.SourceType != LyricSourceType.ExternalFile ||
                !string.IsNullOrWhiteSpace(document.SourcePath);
        }

        private static int CompareDocuments(LyricDocument left, LyricDocument right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            int priorityComparison = GetPriority(left).CompareTo(GetPriority(right));

            if (priorityComparison != 0)
            {
                return priorityComparison;
            }

            int updatedAtComparison = right.UpdatedAt.CompareTo(
                left.UpdatedAt);

            if (updatedAtComparison != 0)
            {
                return updatedAtComparison;
            }

            int sourcePathComparison = string.Compare(
                left.SourcePath ?? string.Empty,
                right.SourcePath ?? string.Empty,
                StringComparison.OrdinalIgnoreCase);

            if (sourcePathComparison != 0)
            {
                return sourcePathComparison;
            }

            return string.Compare(
                left.Id ?? string.Empty,
                right.Id ?? string.Empty,
                StringComparison.Ordinal);
        }

        private static int GetPriority(LyricDocument document)
        {
            if (document.IsUserOverride)
            {
                return 0;
            }

            switch (document.SourceType)
            {
                case LyricSourceType.ExternalFile:
                    return 1;
                case LyricSourceType.Embedded:
                    return 2;
                case LyricSourceType.Online:
                    return 3;
                default:
                    return int.MaxValue;
            }
        }

        private static bool ContainsSameSource(
            List<LyricDocument> documents,
            LyricDocument candidate)
        {
            for (int i = 0; i < documents.Count; i++)
            {
                if (IsSameSource(documents[i], candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSameSource(
            LyricDocument left,
            LyricDocument right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (left.IsUserOverride || right.IsUserOverride)
            {
                return left.IsUserOverride && right.IsUserOverride;
            }

            if (left.SourceType != right.SourceType)
            {
                return false;
            }

            if (left.SourceType != LyricSourceType.ExternalFile)
            {
                return true;
            }

            return string.Equals(
                left.SourcePath ?? string.Empty,
                right.SourcePath ?? string.Empty,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSourceDescription(
            LyricDocument document)
        {
            if (document == null)
            {
                return string.Empty;
            }

            if (document.IsUserOverride)
            {
                return "应用缓存";
            }

            if (document.SourceType == LyricSourceType.ExternalFile)
            {
                return document.SourcePath ?? string.Empty;
            }

            return document.SourceType.ToString();
        }
    }
}

using CorePlanetMusicPlayer.Core.Lyrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class LrcParser : ILyricParser
    {
        /// <summary>
        /// 支持：
        /// [01:23]
        /// [01:23.4]
        /// [01:23.45]
        /// [01:23.456]
        /// </summary>
        private static readonly Regex TimeTagRegex = new Regex(
            @"\[(\d{1,3}):([0-5]?\d)(?:\.(\d{1,3}))?\]",
            RegexOptions.Compiled);

        public bool CanParse(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return false;
            }

            return TimeTagRegex.IsMatch(rawText);
        }

        public IReadOnlyList<LyricLine> Parse(string rawText)
        {
            var fragments = new List<LyricFragment>();

            if (string.IsNullOrWhiteSpace(rawText))
            {
                return new List<LyricLine>();
            }

            string normalizedText = NormalizeRawText(rawText);

            string[] lines = normalizedText.Split(new[] { "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                ParseLine(lines[i], fragments);
            }

            /*
             * List.Sort 本身不是稳定排序。
             * LyricFragment.Order 保存原始出现顺序，
             * 因此时间相同时再按 Order 排列。
             */
            fragments.Sort(CompareLyricFragment);

            return MergeFragments(fragments);
        }

        public string NormalizeRawText(string rawText)
        {
            if (string.IsNullOrEmpty(rawText))
            {
                return string.Empty;
            }

            /*
             * 先处理 Windows 换行，再处理剩余的单独回车，
             * 避免把 \r\n 转换成两个换行。
             */
            string normalizedText = rawText
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");

            // 某些 UTF-8 文本读取后会在开头保留 BOM 字符。
            normalizedText = normalizedText.TrimStart('\uFEFF');

            return normalizedText.Trim();
        }

        private static void ParseLine(
            string lineText,
            List<LyricFragment> fragments)
        {
            if (string.IsNullOrWhiteSpace(lineText))
            {
                return;
            }

            MatchCollection matches = TimeTagRegex.Matches(lineText);

            /*
             * 没有时间标签的行可能是：
             * [ar:艺术家]
             * [ti:标题]
             * [al:专辑]
             * 或普通说明文字。
             *
             * 当前阶段不把它们转换为同步歌词行。
             */
            if (matches.Count == 0)
            {
                return;
            }

            string lyricText = TimeTagRegex.Replace(lineText, string.Empty).Trim();
            string translationText;

            SplitInlineTranslation(
                lyricText,
                out lyricText,
                out translationText);

            for (int i = 0; i < matches.Count; i++)
            {
                TimeSpan time = ParseTime(matches[i]);

                fragments.Add(new LyricFragment
                {
                    Time = time,
                    Text = lyricText,
                    TranslationText = translationText,
                    Order = fragments.Count
                });
            }
        }

        private static IReadOnlyList<LyricLine> MergeFragments(
            List<LyricFragment> fragments)
        {
            var result = new List<LyricLine>();
            int fragmentIndex = 0;

            while (fragmentIndex < fragments.Count)
            {
                TimeSpan time = fragments[fragmentIndex].Time;
                var timeFragments = new List<LyricFragment>();

                while (fragmentIndex < fragments.Count &&
                    fragments[fragmentIndex].Time == time)
                {
                    timeFragments.Add(fragments[fragmentIndex]);
                    fragmentIndex++;
                }

                result.Add(CreateLyricLine(
                    result.Count,
                    time,
                    timeFragments));
            }

            return result;
        }

        private static LyricLine CreateLyricLine(
            int index,
            TimeSpan time,
            List<LyricFragment> fragments)
        {
            LyricFragment originalFragment = fragments[0];

            for (int i = 0; i < fragments.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(fragments[i].Text))
                {
                    originalFragment = fragments[i];
                    break;
                }
            }

            string originalText = originalFragment.Text ?? string.Empty;
            var translations = new List<string>();

            AddTranslation(
                translations,
                originalFragment.TranslationText,
                originalText);

            for (int i = 0; i < fragments.Count; i++)
            {
                LyricFragment fragment = fragments[i];

                if (object.ReferenceEquals(fragment, originalFragment))
                {
                    continue;
                }

                AddTranslation(translations, fragment.Text, originalText);
                AddTranslation(
                    translations,
                    fragment.TranslationText,
                    originalText);
            }

            string translationText = string.Join("\n", translations);

            return LyricLine.Create(
                index,
                time,
                originalText,
                translationText);
        }

        private static void AddTranslation(
            List<string> translations,
            string translationText,
            string originalText)
        {
            if (string.IsNullOrWhiteSpace(translationText))
            {
                return;
            }

            string text = translationText.Trim();

            if (string.Equals(text, originalText, StringComparison.Ordinal) ||
                translations.Contains(text))
            {
                return;
            }

            translations.Add(text);
        }

        private static void SplitInlineTranslation(
            string content,
            out string text,
            out string translationText)
        {
            text = content == null ? string.Empty : content.Trim();
            translationText = string.Empty;

            if (!text.EndsWith("」", StringComparison.Ordinal))
            {
                return;
            }

            int translationStartIndex = text.LastIndexOf('「');

            if (translationStartIndex <= 0)
            {
                return;
            }

            string originalText = text
                .Substring(0, translationStartIndex)
                .TrimEnd();

            string translatedText = text
                .Substring(
                    translationStartIndex + 1,
                    text.Length - translationStartIndex - 2)
                .Trim();

            if (string.IsNullOrWhiteSpace(originalText) ||
                string.IsNullOrWhiteSpace(translatedText))
            {
                return;
            }

            text = originalText;
            translationText = translatedText;
        }

        private static TimeSpan ParseTime(Match match)
        {
            int minutes = ParseInt(match.Groups[1].Value);
            int seconds = ParseInt(match.Groups[2].Value);
            int milliseconds = ParseMilliseconds(match.Groups[3].Value);

            return new TimeSpan(0, 0, minutes, seconds, milliseconds);
        }

        private static int ParseInt(string value)
        {
            int result;

            if (int.TryParse(value, out result))
            {
                return result;
            }

            return 0;
        }

        private static int ParseMilliseconds(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            string text = value.Trim();

            if (text.Length == 1)
            {
                text += "00";
            }
            else if (text.Length == 2)
            {
                text += "0";
            }
            else if (text.Length > 3)
            {
                text = text.Substring(0, 3);
            }

            return ParseInt(text);
        }

        private static int CompareLyricFragment(
            LyricFragment left,
            LyricFragment right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            int timeComparison = left.Time.CompareTo(right.Time);

            if (timeComparison != 0)
            {
                return timeComparison;
            }

            return left.Order.CompareTo(right.Order);
        }

        private sealed class LyricFragment
        {
            public TimeSpan Time { get; set; }

            public string Text { get; set; }

            public string TranslationText { get; set; }

            public int Order { get; set; }
        }
    }
}

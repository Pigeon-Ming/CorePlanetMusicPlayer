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
        private static readonly Regex TimeTagRegex = new Regex(
            @"\[(\d{1,2}):(\d{1,2})(?:\.(\d{1,3}))?\]",
            RegexOptions.Compiled);

        public bool CanPrse(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText))
            {
                return false;
            }

            return TimeTagRegex.IsMatch(rawText);
        }

        public IReadOnlyList<LyricLine> Parse(string rawText)
        {
            var result = new List<LyricLine>();

            if (string.IsNullOrWhiteSpace(rawText))
            {
                return result;
            }

            var normalizedText = NormalizeRawText(rawText);
            var lines = normalizedText.Split(new[] {"\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                ParseLine(lines[i], result);
            }

            result.Sort(CompareLyricLine);

            return result;
        }

        public string NormalizeRawText(string rawText)
        {
            throw new NotImplementedException();
        }

        private static void ParseLine(string lineText, List<LyricLine> result)
        {
            if (string.IsNullOrWhiteSpace(lineText))
            {
                return;
            }

            var matches = TimeTagRegex.Matches(lineText);

            if (matches.Count == 0)
            {
                return;
            }

            var lyricText = TimeTagRegex.Replace(lineText, string.Empty).Trim();

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var time = ParseTime(match);
            }
        }

        private static TimeSpan ParseTime(Match match)
        {
            var minutes = ParseInt(match.Groups[1].Value);
            var seconds = ParseInt(match.Groups[2].Value);
            var milliseconds = ParseMillseconds(match.Groups[3].Value);

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

        private static int ParseMillseconds(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            var text = value.Trim();

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

        private static int CompareLyricLine(LyricLine left, LyricLine right)
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

            return left.Time.CompareTo(right.Time);
        }
    }
}

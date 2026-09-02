using CorePlanetMusicPlayer.Core.Lyrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public class LyricParserCollection : ILyricParser
    {
        private readonly List<ILyricParser> _parsers;

        public LyricParserCollection(IEnumerable<ILyricParser> parsers)
        {
            _parsers = new List<ILyricParser>();

            if (parsers == null)
            {
                return;
            }

            foreach (var parser in parsers)
            {
                if (parser != null)
                {
                    _parsers.Add(parser);
                }
            }
        }

        public bool CanParse(string rawText)
        {
            return FindParser(rawText) != null;
        }

        public IReadOnlyList<LyricLine> Parse(string rawText)
        {
            var parser = FindParser(rawText);

            if (parser == null)
            {
                return new List<LyricLine>();
            }

            return parser.Parse(rawText);
        }

        public string NormalizeRawText(string rawText)
        {
            if (rawText == null)
            {
                return string.Empty;
            }

            var parser = FindParser(rawText);

            if (parser == null)
            {
                return rawText.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            }

            return parser.NormalizeRawText(rawText);
        }

        private ILyricParser FindParser(string rawText)
        {
            for (int i = 0; i < _parsers.Count; i++)
            {
                var parser = _parsers[i];

                if (parser != null && parser.CanParse(rawText))
                {
                    return parser;
                }
            }

            return null;
        }
    }
}

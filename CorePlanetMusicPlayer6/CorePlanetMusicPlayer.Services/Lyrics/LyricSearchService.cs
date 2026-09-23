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
            if (documents == null)
            {
                return null;
            }

            var list = new List<LyricDocument>();

            foreach (var document in documents)
            {
                if (document != null)
                {
                    list.Add(document);
                }
            }

            var manual = FindBySourceType(list, LyricSourceType.Manual);

            if (manual != null)
            {
                return manual;
            }

            var external = FindBySourceType(list, LyricSourceType.ExternalFile);

            if (external != null)
            {
                return external;
            }

            var embedded = FindBySourceType(list, LyricSourceType.Embedded);

            if (embedded != null)
            {
                return embedded;
            }

            var online = FindBySourceType(list, LyricSourceType.Online);

            if (online != null)
            {
                return online;
            }

            if (list.Count > 0)
            {
                return list[0];
            }

            return null;
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

        private static LyricDocument FindBySourceType(IEnumerable<LyricDocument> documents,LyricSourceType sourceType)
        {
            if (documents == null)
            {
                return null;
            }

            foreach (var document in documents)
            {
                if (document != null && document.SourceType == sourceType)
                {
                    return document;
                }
            }

            return null;
        }
    }
}

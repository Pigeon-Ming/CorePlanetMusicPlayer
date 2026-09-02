using CorePlanetMusicPlayer.Core.Lyrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public interface ILyricParser
    {
        bool CanParse(string rawText);

        IReadOnlyList<LyricLine> Parse(string rawText);

        string NormalizeRawText(string rawText);
    }
}

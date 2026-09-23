using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public enum ArtworkSourceKind
    {
        None = 0,

        Auto = 1,

        Embedded = 2,

        Cache = 3,

        File = 4,

        Default = 5
    }
}

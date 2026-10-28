using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Artwork
{
    public enum ArtworkSourceKind
    {
        Default = 0,

        MusicFile = 1,

        ManagedFile = 2,

        RemoteUri = 3
    }
}

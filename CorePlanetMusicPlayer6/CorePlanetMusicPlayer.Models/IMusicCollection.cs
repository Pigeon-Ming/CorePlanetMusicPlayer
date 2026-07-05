using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{

    public interface IMusicCollection
    {
        string Title { get; }

        string CoverPath { get; }

        IEnumerable<IMusic> MusicItems { get; }

        int MusicCount { get; }

        TimeSpan TotalDuration { get; }
    }

}

using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.PlayCore
{
    public interface IPlayMode
    {
        PlayQueue PlayQueue { get; set; }

        int Next(int index);
        int Previous(int index);
        
    }
}

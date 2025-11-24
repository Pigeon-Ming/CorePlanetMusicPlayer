using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.App
{
    public interface IArtistsSelectControl
    {
        event EventHandler<Artist> ArtistSelected;
    }
}

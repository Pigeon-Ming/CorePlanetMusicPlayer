using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class PlayList
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<Music> Music { get; set; } = new List<Music>();
    }
}

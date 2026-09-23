using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Playback
{
    public sealed class PlaybackSessionData
    {
        public int Version { get; set; }

        public int Mode { get; set; }

        public int CurrentIndex { get; set; }

        public List<PlaybackSessionItemData> Items { get; set; }

        public List<string> ShuffleItemIds { get; set; }
    }

    public sealed class PlaybackSessionItemData
    {
        public string Id { get; set; }

        public string MusicId { get; set; }

        public int Order { get; set; }
    }
}

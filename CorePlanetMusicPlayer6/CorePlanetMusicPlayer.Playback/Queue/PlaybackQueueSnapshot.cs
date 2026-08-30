using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Queue
{
    public sealed class PlaybackQueueSnapshot
    {
        public List<PlaybackQueueItem> Items { get; set; } = new List<PlaybackQueueItem>();

        public int CurrentIndex { get; set; }

        public bool HasItems
        {
            get { return Items != null && Items.Count > 0; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Statistics
{
    public sealed class DailyPlaybackStat
    {
        public DateTime Date { get; set; }

        public int PlayCount { get; set; }

        public int CompletedPlayCount { get; set; }

        public int SkippedPlayCount { get; set; }

        public TimeSpan TotalPlayedDuration { get; set; }
    }
}

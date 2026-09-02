using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Statistics
{
    public sealed class PlaybackStatisticsSummary
    {
        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset EndTime { get; set; }

        public int TotalPlayCount { get; set; }

        public int CompletedPlayCount { get; set; }

        public int SkippedPlayCount { get; set; }

        public int DistinctMusicCount { get; set; }

        public TimeSpan TotalPlayedDuration { get; set; }

        public TimeSpan AveragePlayedDuration
        {
            get
            {
                if (TotalPlayCount <= 0)
                {
                    return TimeSpan.Zero;
                }

                return TimeSpan.FromTicks(
                    TotalPlayedDuration.Ticks / TotalPlayCount);
            }
        }
    }
}

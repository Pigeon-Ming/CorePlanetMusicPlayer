using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Statistics
{
    public sealed class MusicPlaybackStat
    {
        public MusicId MusicId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public string AlbumTitle { get; set; } = string.Empty;

        public int PlayCount { get; set; }

        public int CompletedPlayCount { get; set; }

        public int SkippedPlayCount { get; set; }

        public TimeSpan TotalPlayedDuration { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Entities
{
    public sealed class PlaybackHistoryEntity
    {
        public string Id { get; set; } = string.Empty;

        public string MusicId { get; set; } = string.Empty;

        public long PlayedAtUnixTimeMilliseconds { get; set; }

        public long MusicDurationTicks { get; set; }

        public long PlayedDurationTicks { get; set; }

        public long LastPositionTicks { get; set; }

        public bool IsCompleted { get; set; }
    }
}

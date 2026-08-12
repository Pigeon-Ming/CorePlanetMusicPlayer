using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Entities
{
    public sealed class ArtistEntity
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string SortName { get; set; } = string.Empty;

        public string MusicIdsText { get; set; } = string.Empty;

        public string AlbumIdsText { get; set; } = string.Empty;

        public long TotalDurationTicks { get; set; }

        public long? AddedAtUnixTimeMilliseconds { get; set; }

        public long? UpdatedAtUnixTimeMilliseconds { get; set; }
    }
}

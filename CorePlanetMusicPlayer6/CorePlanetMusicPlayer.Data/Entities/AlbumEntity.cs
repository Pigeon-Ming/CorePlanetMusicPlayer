using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Entities
{
    public sealed class AlbumEntity
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public string AlbumArtistName { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public int? Year { get; set; }

        public string MusicIdsText { get; set; } = string.Empty;

        public long TotalDurationTicks { get; set; }

        public long? AddedAtUnixTimeMilliseconds { get; set; }

        public long? UpdatedAtUnixTimeMilliseconds { get; set; }
    }
}

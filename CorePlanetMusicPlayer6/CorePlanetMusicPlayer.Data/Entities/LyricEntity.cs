using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Entities
{
    public sealed class LyricEntity
    {
        public string Id { get; set; } = string.Empty;

        public string MusicId { get; set; } = string.Empty;

        public int SourceType { get; set; }

        public string SourcePath { get; set; } = string.Empty;

        public string RawText { get; set; } = string.Empty;

        public string LinesText { get; set; } = string.Empty;

        public long CreatedAtUnixTimeMilliseconds { get; set; }

        public long UpdatedAtUnixTimeMilliseconds { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Entities
{
    public sealed class PlaylistItemEntity
    {
        public string Id { get; set; } = string.Empty;

        public string PlaylistId { get; set; } = string.Empty;

        public string MusicId { get; set; } = string.Empty;

        public int Order { get; set; }

        public long AddedAtUnixTimeMilliseconds { get; set; }
    }
}

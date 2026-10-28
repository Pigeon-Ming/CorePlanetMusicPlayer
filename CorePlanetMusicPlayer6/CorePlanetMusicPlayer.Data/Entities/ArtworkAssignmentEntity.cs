using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Entities
{
    public sealed class ArtworkAssignmentEntity
    {
        public int OwnerKind { get; set; }

        public string OwnerId { get; set; } = string.Empty;

        public int SourceKind { get; set; }

        public string ResourceKey { get; set; } = string.Empty;

        public string RemoteUrl { get; set; } = string.Empty;

        public long UpdatedAtUnixTimeMilliseconds { get; set; }
    }
}

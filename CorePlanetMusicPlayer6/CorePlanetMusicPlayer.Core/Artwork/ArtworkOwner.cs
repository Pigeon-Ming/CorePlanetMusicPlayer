using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Artwork
{
    public sealed class ArtworkOwner
    {
        public ArtworkOwnerKind Kind { get; private set; }

        public string Id { get; private set; }

        public ArtworkOwner(ArtworkOwnerKind kind, string id)
        {
            if (kind != ArtworkOwnerKind.Music && kind != ArtworkOwnerKind.Artist && kind != ArtworkOwnerKind.Playlist)
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            var normalizedId = EntityId.Normalize(id);

            if (EntityId.IsEmpty(normalizedId))
            {
                throw new ArgumentException("Artwork owner id cannot be empty.", nameof(id));
            }

            Kind = kind;
            Id = normalizedId;
        }
    }
}

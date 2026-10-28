using CorePlanetMusicPlayer.Core.Artwork;
using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public sealed class ArtworkChangedEventArgs : EventArgs
    {
        public ArtworkOwner Owner { get; private set; }

        public ArtworkChangedEventArgs(ArtworkOwner owner)
        {
            Guard.NotNull(owner, nameof(owner));

            Owner = owner;
        }
    }
}

using CorePlanetMusicPlayer.Core.Artwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Artwork
{
    public interface IArtworkEditService
    {
        event EventHandler<ArtworkChangedEventArgs> ArtworkChanged;

        Task<ArtworkAssignment> ImportAsync(ArtworkOwner owner, Stream source);

        Task<ArtworkAssignment> SetRemoteUriAsync(ArtworkOwner owner, string remoteUrl);

        Task ResetAsync(ArtworkOwner owner);
    }
}

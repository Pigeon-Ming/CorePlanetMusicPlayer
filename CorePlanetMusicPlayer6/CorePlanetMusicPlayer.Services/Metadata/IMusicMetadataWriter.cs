using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Metadata
{
    public interface IMusicMetadataWriter
    {
        Task<Result> WriteAsync(Music music, MusicMetadataUpdateRequest request);
    }
}

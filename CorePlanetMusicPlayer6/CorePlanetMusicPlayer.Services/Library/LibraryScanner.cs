using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library
{
    /// <summary>
    /// 先做占位，后续在实现平台功能时再实现
    /// </summary>
    public sealed class LibraryScanner : ILibraryScanner
    {

        public Task<IReadOnlyCollection<Music>> ScanAsync(LibraryFolder folder)
        {
            IReadOnlyCollection<Music> result = new List<Music>();

            return Task.FromResult(result);
        }
    }
}

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
    /// 音乐库扫描接口
    /// </summary>
    public interface ILibraryScanner
    {
        Task<IReadOnlyList<Music>> ScanAsync(LibraryFolder folder);
    }
}

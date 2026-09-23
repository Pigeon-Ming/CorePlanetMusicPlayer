using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    /// <summary>
    /// 为播放流程提供歌曲资料。
    /// </summary>
    public interface IPlaybackMusicResolver
    {
        Task<Music> GetByIdAsync(MusicId musicId);
    }
}

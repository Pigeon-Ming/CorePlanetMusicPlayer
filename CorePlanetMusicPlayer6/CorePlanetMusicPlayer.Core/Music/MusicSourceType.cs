using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Music
{
    /// <summary>
    /// 音乐来源类型
    /// </summary>
    public enum MusicSourceType
    {
        Local = 0,// 本地音乐
        Stream = 1,// 流式传输音乐
        Temporary = 2,// 临时添加的音乐（暂定）
    }
}

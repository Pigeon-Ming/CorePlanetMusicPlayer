using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Modes
{
    public enum PlaybackMode
    {
        /// <summary>
        /// 顺序播放
        /// </summary>
        Sequential = 0,

        /// <summary>
        /// 列表循环
        /// </summary>
        RepeatAll = 1,

        /// <summary>
        /// 单曲循环
        /// </summary>
        RepeatOne = 2,

        /// <summary>
        /// 随机播放
        /// </summary>
        Shuffle = 3,

        /// <summary>
        /// 倒序播放
        /// </summary>
        Reverse = 4
    }
}

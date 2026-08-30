using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    public enum PlaybackStatus
    {
        /// <summary>
        /// 表示没有播放任务或播放已停止
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// 表示正在加载
        /// </summary>
        Loading = 1,

        /// <summary>
        /// 表示正在播放
        /// </summary>
        Playing = 2,

        /// <summary>
        /// 表示已暂停
        /// </summary>
        Paused = 3,

        /// <summary>
        /// 表示当前音乐播放结束
        /// </summary>
        Ended = 4,

        /// <summary>
        /// 表示发生错误
        /// </summary>
        Error = 5
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Lyrics
{
    public enum LyricSourceType
    {
        /// <summary>
        /// 未知来源
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 内嵌歌词
        /// </summary>
        Embedded = 1,

        /// <summary>
        /// 外部.lrc等歌词文件
        /// </summary>
        ExternalFile = 2,

        /// <summary>
        /// 用户手动编辑或粘贴的歌词（待验证必要性）
        /// </summary>
        Manual = 3,

        /// <summary>
        /// 网络来源
        /// </summary>
        Online = 4
    }
}

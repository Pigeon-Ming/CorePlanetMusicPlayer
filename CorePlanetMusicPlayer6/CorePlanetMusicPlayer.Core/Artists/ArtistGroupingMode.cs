using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Artists
{
    /// <summary>
    /// 多艺术家歌曲的分类方式。
    /// </summary>
    public enum ArtistGroupingMode
    {
        /// <summary>
        /// 分别归入每位参与艺术家。
        /// </summary>
        Separate = 0,

        /// <summary>
        /// 将完整署名作为一个分类项。
        /// </summary>
        Combined = 1
    }
}

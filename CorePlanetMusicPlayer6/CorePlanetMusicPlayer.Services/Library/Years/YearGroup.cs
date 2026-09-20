using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Years
{
    /// <summary>
    /// 根据歌曲元数据汇总得到的年份分类。
    /// </summary>
    public sealed class YearGroup
    {
        /// <summary>
        /// 年份。null 表示未知年份。
        /// </summary>
        public int? Year { get; set; }

        public int MusicCount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Genres
{
    /// <summary>
    /// 根据歌曲元数据汇总得到的流派分类。
    /// </summary>
    public sealed class GenreGroup
    {
        /// <summary>
        /// 流派名称。null 表示未知流派。
        /// </summary>
        public string Genre { get; set; }

        public int MusicCount { get; set; }
    }
}

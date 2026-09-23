using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Music
{
    /// <summary>
    /// 音乐元数据
    /// </summary>
    public sealed class MusicMetadata
    {
        public static MusicMetadata Empty
        {
            get { return new MusicMetadata(); }
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 艺术家显示文本
        /// </summary>
        public string ArtistName { get; set; } = string.Empty;

        /// <summary>
        /// 歌曲的参与艺术家名称集合。
        /// 每一项表示一个完整名称，不根据标点符号自动拆分。
        /// </summary>
        public List<string> ArtistNames { get; set; } = new List<string>();

        /// <summary>
        /// 专辑标题
        /// </summary>
        public string AlbumTitle { get; set; } = string.Empty;

        /// <summary>
        /// 专辑艺术家名称
        /// </summary>
        public string AlbumArtistName { get; set; } = string.Empty;

        /// <summary>
        /// 流派
        /// </summary>
        public string Genre { get; set; } = string.Empty;

        /// <summary>
        /// 年份
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// 音轨号
        /// </summary>
        public int? TrackNumber { get; set; }

        /// <summary>
        /// 碟号
        /// </summary>
        public int? DiscNumber { get; set; }

        /// <summary>
        /// 作曲家
        /// </summary>
        public string Composer { get; set; } = string.Empty;

        /// <summary>
        /// 注解
        /// </summary>
        public string Comment { get; set; } = string.Empty;
    }
}

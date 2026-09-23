using CorePlanetMusicPlayer.Core.Artists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Index
{
    public interface IMusicIndexService
    {
        /// <summary>
        /// 根据当前歌曲重建专辑和艺术家索引。
        /// 失败时向调用方报告异常。
        /// </summary>
        Task RebuildAsync();

        /// <summary>
        /// 保存艺术家分类规则，并根据已有歌曲重建索引。
        /// 只有设置保存和索引重建都成功，操作才正常完成。
        /// </summary>
        Task SetArtistGroupingAsync(ArtistGroupingMode groupingMode);
    }
}

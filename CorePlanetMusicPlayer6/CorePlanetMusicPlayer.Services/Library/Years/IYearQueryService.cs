using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Years
{
    public interface IYearQueryService
    {
        /// <summary>
        /// 获取年份分类及歌曲数量。
        /// 年份从新到旧排列，未知年份位于最后。
        /// </summary>
        Task<IReadOnlyList<YearGroup>> GetAllAsync();

        /// <summary>
        /// 获取指定年份的歌曲。
        /// null 或非正数表示未知年份。
        /// 按歌曲标题排序。
        /// </summary>
        Task<IReadOnlyList<Music>> GetMusicAsync(int? year);
    }
}

using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Genres
{
    public interface IGenreQueryService
    {
        /// <summary>
        /// 获取流派分类及歌曲数量。
        /// 按名称排序，未知流派位于最后。
        /// </summary>
        Task<IReadOnlyList<GenreGroup>> GetAllAsync();

        /// <summary>
        /// 获取指定流派的歌曲，忽略名称大小写及首尾空白。
        /// null、空字符串或纯空白表示未知流派。
        /// 按歌曲标题排序。
        /// </summary>
        Task<IReadOnlyList<Music>> GetMusicAsync(string genre);
    }
}

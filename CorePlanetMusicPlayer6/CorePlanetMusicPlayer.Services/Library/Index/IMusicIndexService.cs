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
    }
}

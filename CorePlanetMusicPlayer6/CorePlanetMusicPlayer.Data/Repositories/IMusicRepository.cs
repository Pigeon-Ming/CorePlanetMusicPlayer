using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories
{
    public interface IMusicRepository
    {
        /// <summary>
        /// 获取全部音乐缓存。
        /// </summary>
        /// <returns>全部音乐缓存列表</returns>
        Task<IReadOnlyList<Music>> GetAllAsync();

        /// <summary>
        /// 根据MusicId获取一首音乐，未找到时返回null。
        /// </summary>
        /// <param name="id">要查找的目标音乐Id</param>
        /// <returns>获取结果</returns>
        Task<Music> GetByIdAsync(MusicId id);

        /// <summary>
        /// 按关键词搜索音乐。
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns></returns>
        Task<IReadOnlyList<Music>> SearchAsync(string keyword);

        /// <summary>
        /// 获取某个音乐库文件夹下的所有音乐。
        /// </summary>
        /// <param name="libraryFolderId">音乐库Id</param>
        /// <returns>该文件夹下的所有音乐。</returns>
        Task<IReadOnlyList<Music>> GetByLibraryFolderIdAsync(LibraryFolderId libraryFolderId);

        /// <summary>
        /// 新增或更新一首音乐。
        /// </summary>
        /// <param name="music">要新增的音乐</param>
        /// <returns></returns>
        Task UpsertAsync(Music music);

        /// <summary>
        /// 批量新增或更新音乐。
        /// </summary>
        /// <param name="musicList">要批量新增的音乐列表</param>
        /// <returns></returns>
        Task UpsertRangeAsync(IEnumerable<Music> musicList);

        /// <summary>
        /// 删除单首音乐。
        /// </summary>
        /// <param name="id">要删除的音乐Id</param>
        /// <returns></returns>
        Task DeleteAsync(MusicId id);

        /// <summary>
        /// 删除某个音乐库文件夹下的所有音乐缓存。
        /// </summary>
        /// <param name="libraryFolderId">音乐库Id</param>
        /// <returns></returns>
        Task DeleteByLibraryFolderIdAsync(LibraryFolderId libraryFolderId);

        /// <summary>
        /// 清空LocalMusic缓存。
        /// </summary>
        /// <returns></returns>
        Task ClearLocalMusicAsync();

        /// <summary>
        /// 清空StreamMusic缓存。
        /// </summary>
        /// <returns></returns>
        Task ClearStreamMusicAsync();
    }
}

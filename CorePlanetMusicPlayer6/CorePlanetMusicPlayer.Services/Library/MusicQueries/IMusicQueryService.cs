using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.MusicQueries
{
    public interface IMusicQueryService
    {

        Task<IReadOnlyList<Music>> GetAllAsync();

        Task<Music> GetByIdAsync(MusicId musicId);

        Task<IReadOnlyList<Music>> SearchAsync(string keyword);

        Task<IReadOnlyList<Music>> GetByFolderAsync(
            LibraryFolderId folderId);

        /// <summary>
        /// 按输入 ID 的顺序读取歌曲。
        /// 保留重复项，跳过不存在的歌曲。
        /// </summary>
        Task<IReadOnlyList<Music>> GetByIdsAsync(
            IEnumerable<MusicId> musicIds);
    }
}

using CorePlanetMusicPlayer.Core.Albums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories
{
    public interface IAlbumRepository
    {
        Task<IReadOnlyList<Album>> GetAllAsync();

        Task<Album> GetByIdAsync(AlbumId id);

        Task<IReadOnlyList<Album>> SearchAsync(string keyword);

        Task UpsertAsync(Album album);

        Task UpsertRangeAsync(IEnumerable<Album> albums);

        /// <summary>
        /// 更新简介和更新时间。
        /// 返回 false 表示目标专辑不存在。
        /// </summary>
        Task<bool> UpdateDescriptionAsync(AlbumId albumId, string description, DateTimeOffset updatedAt);

        Task DeleteAsync(AlbumId id);

        Task ClearAsync();
    }
}

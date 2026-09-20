using CorePlanetMusicPlayer.Core.Artists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Artists
{
    public interface IArtistService
    {
        Task<IReadOnlyList<Artist>> GetAllAsync();

        Task<Artist> GetByIdAsync(ArtistId artistId);

        Task<ArtistDetails> GetDetailsAsync(ArtistId artistId);

        /// <summary>
        /// 删除没有关联歌曲的艺术家及其资料。
        /// </summary>
        Task DeleteAsync(ArtistId artistId);

        Task UpdateDescriptionAsync(ArtistId artistId, string description);
    }
}

using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories
{
    public interface ILyricRepository
    {
        Task<LyricDocument> GetByIdAsync(string id);

        Task<LyricDocument> GetByMusicIdAsync(MusicId musicId);

        Task<IReadOnlyList<LyricDocument>> GetAllByMusicIdAsync(MusicId musicId);

        Task UpsertAsync(LyricDocument document);

        Task DeleteAsync(string id);

        Task DeleteByMusicIdAsync(MusicId musicId);

        Task ClearAsync();
    }
}

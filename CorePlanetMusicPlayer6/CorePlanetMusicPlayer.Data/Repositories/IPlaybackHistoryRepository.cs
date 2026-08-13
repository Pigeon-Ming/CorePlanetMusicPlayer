using CorePlanetMusicPlayer.Core.History;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories
{
    public interface IPlaybackHistoryRepository
    {
        Task<IReadOnlyList<PlaybackHistoryItem>> GetRecentAsync(int maxCount);

        Task<IReadOnlyList<PlaybackHistoryItem>> GetByMusicIdAsync(MusicId musicId);

        Task<IReadOnlyList<PlaybackHistoryItem>> GetByDateRangeAsync(DateTimeOffset startTime, DateTimeOffset endTime);

        Task<IReadOnlyList<PlaybackHistoryItem>> GetByMusicIdAndDateRangeAsync(MusicId musicId, DateTimeOffset startTime, DateTimeOffset endTime);

        Task AddAsync(PlaybackHistoryItem item);

        Task DeleteAsync(PlaybackHistoryId id);

        Task ClearAsync();
    }
}

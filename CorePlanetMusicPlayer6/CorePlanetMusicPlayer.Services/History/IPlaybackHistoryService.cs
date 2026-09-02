using CorePlanetMusicPlayer.Core.History;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.History
{
    public interface IPlaybackHistoryService
    {
        Task RecordPlaybackAsync(MusicId musicId, TimeSpan musicDuration, TimeSpan playedDuration, TimeSpan lastPosition);

        Task<IReadOnlyList<PlaybackHistoryItem>> GetByMusicIdAsync(MusicId musicId);

        Task<IReadOnlyList<PlaybackHistoryItem>> GetByDateRangeAsync(DateTimeOffset startTime, DateTimeOffset endTime);

        Task DeleteAsync(PlaybackHistoryId historyId);

        Task DeleteBeforeAsync(DateTimeOffset time);

        Task ClearAsync();

        bool IsCompleted(TimeSpan musicDUration, TimeSpan playedDuration);
    }
}

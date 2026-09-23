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
        Task RecordPlaybackAsync(
            PlaybackHistoryId historyId, 
            MusicId musicId, 
            DateTimeOffset playedAt, 
            TimeSpan musicDuration, 
            TimeSpan playedDuration, 
            TimeSpan lastPosition,
            string titleSnapshot,
            string artistNameSnapshot,
            string albumTitleSnapshot);

        Task<IReadOnlyList<PlaybackHistoryItem>> GetByMusicIdAsync(MusicId musicId);

        //Task<IReadOnlyList<PlaybackHistoryItem>> GetByDateRangeAsync(DateTimeOffset startTime, DateTimeOffset endTime);

        Task<IReadOnlyList<PlaybackHistoryItem>> GetByDateRangeAsync(DateTimeOffset? startTime = null, DateTimeOffset? endTime = null);

        Task DeleteAsync(PlaybackHistoryId historyId);

        Task DeleteBeforeAsync(DateTimeOffset time);

        Task ClearAsync();

        bool IsCompleted(TimeSpan musicDUration, TimeSpan playedDuration);
    }
}

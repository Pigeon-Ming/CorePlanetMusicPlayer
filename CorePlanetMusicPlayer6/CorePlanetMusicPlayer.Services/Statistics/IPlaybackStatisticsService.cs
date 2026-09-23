using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Statistics
{
    public interface IPlaybackStatisticsService
    {
        Task<PlaybackStatisticsSummary> GetSummaryAsync(DateTimeOffset startTime, DateTimeOffset endTime);

        Task<PlaybackStatisticsSummary> GetLast7DaysSummaryAsync();

        Task<PlaybackStatisticsSummary> GetLast30DaysSummaryAsync();

        Task<IReadOnlyList<DailyPlaybackStat>> GetDailyStatsAsync(DateTimeOffset startTime, DateTimeOffset endTime);

        Task<IReadOnlyList<DailyPlaybackStat>> GetLast7DaysDailyStatsAsync();

        Task<IReadOnlyList<DailyPlaybackStat>> GetLast30DaysDailyStatsAsync();

        Task<IReadOnlyList<MusicPlaybackStat>> GetTopMusicAsync(DateTimeOffset startTime, DateTimeOffset endTime, int maxCount);

        Task<IReadOnlyList<ArtistPlaybackStat>> GetTopArtistsAsync(DateTimeOffset startTime, DateTimeOffset endTime, int maxCount);

        Task<IReadOnlyList<AlbumPlaybackStat>> GetTopAlbumsAsync(DateTimeOffset startTime, DateTimeOffset endTime, int maxCount);
    }
}

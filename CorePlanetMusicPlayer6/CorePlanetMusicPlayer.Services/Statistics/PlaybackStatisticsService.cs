using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.History;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Statistics
{
    public sealed class PlaybackStatisticsService : IPlaybackStatisticsService
    {
        private readonly IPlaybackHistoryRepository _historyRepository;
        private readonly IMusicRepository _musicRepository;

        public PlaybackStatisticsService(
            IPlaybackHistoryRepository historyRepository,
            IMusicRepository musicRepository)
        {
            Guard.NotNull(historyRepository, nameof(historyRepository));
            Guard.NotNull(musicRepository, nameof(musicRepository));

            _historyRepository = historyRepository;
            _musicRepository = musicRepository;
        }

        public async Task<PlaybackStatisticsSummary> GetSummaryAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            ValidateDateRange(startTime, endTime);

            var histories = await _historyRepository.GetByDateRangeAsync(
                startTime,
                endTime);

            return BuildSummary(histories, startTime, endTime);
        }

        public Task<PlaybackStatisticsSummary> GetLast7DaysSummaryAsync()
        {
            var now = DateTimeOffset.Now;
            var startTime = GetStartOfLocalDay(now).AddDays(-6);

            return GetSummaryAsync(startTime, now);
        }

        public Task<PlaybackStatisticsSummary> GetLast30DaysSummaryAsync()
        {
            var now = DateTimeOffset.Now;
            var startTime = GetStartOfLocalDay(now).AddDays(-29);

            return GetSummaryAsync(startTime, now);
        }

        public async Task<IReadOnlyList<DailyPlaybackStat>> GetDailyStatsAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            ValidateDateRange(startTime, endTime);

            var histories = await _historyRepository.GetByDateRangeAsync(
                startTime,
                endTime);

            return BuildDailyStats(histories, startTime, endTime);
        }

        public Task<IReadOnlyList<DailyPlaybackStat>> GetLast7DaysDailyStatsAsync()
        {
            var now = DateTimeOffset.Now;
            var startTime = GetStartOfLocalDay(now).AddDays(-6);

            return GetDailyStatsAsync(startTime, now);
        }

        public Task<IReadOnlyList<DailyPlaybackStat>> GetLast30DaysDailyStatsAsync()
        {
            var now = DateTimeOffset.Now;
            var startTime = GetStartOfLocalDay(now).AddDays(-29);

            return GetDailyStatsAsync(startTime, now);
        }

        public async Task<IReadOnlyList<MusicPlaybackStat>> GetTopMusicAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int maxCount)
        {
            ValidateDateRange(startTime, endTime);
            Guard.NotNegative(maxCount, nameof(maxCount));

            var histories = await _historyRepository.GetByDateRangeAsync(
                startTime,
                endTime);

            var musicLookup = await BuildMusicLookupAsync();

            var stats = BuildMusicStats(histories, musicLookup);
            stats.Sort(CompareMusicStats);

            return TakeMusicStats(stats, maxCount);
        }

        public async Task<IReadOnlyList<ArtistPlaybackStat>> GetTopArtistsAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int maxCount)
        {
            ValidateDateRange(startTime, endTime);
            Guard.NotNegative(maxCount, nameof(maxCount));

            var histories = await _historyRepository.GetByDateRangeAsync(
                startTime,
                endTime);

            var musicLookup = await BuildMusicLookupAsync();

            var stats = BuildArtistStats(histories, musicLookup);
            stats.Sort(CompareArtistStats);

            return TakeArtistStats(stats, maxCount);
        }

        public async Task<IReadOnlyList<AlbumPlaybackStat>> GetTopAlbumsAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int maxCount)
        {
            ValidateDateRange(startTime, endTime);
            Guard.NotNegative(maxCount, nameof(maxCount));

            var histories = await _historyRepository.GetByDateRangeAsync(
                startTime,
                endTime);

            var musicLookup = await BuildMusicLookupAsync();

            var stats = BuildAlbumStats(histories, musicLookup);
            stats.Sort(CompareAlbumStats);

            return TakeAlbumStats(stats, maxCount);
        }

        private static PlaybackStatisticsSummary BuildSummary(
            IEnumerable<PlaybackHistoryItem> histories,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            var summary = new PlaybackStatisticsSummary
            {
                StartTime = startTime,
                EndTime = endTime,
                TotalPlayedDuration = TimeSpan.Zero
            };

            var musicIds = new HashSet<string>();

            if (histories == null)
            {
                return summary;
            }

            foreach (var history in histories)
            {
                if (!IsValidHistory(history))
                {
                    continue;
                }

                summary.TotalPlayCount++;
                summary.TotalPlayedDuration =
                    summary.TotalPlayedDuration + NormalizeDuration(history.PlayedDuration);

                if (history.IsCompleted)
                {
                    summary.CompletedPlayCount++;
                }
                else
                {
                    summary.SkippedPlayCount++;
                }

                musicIds.Add(history.MusicId.ToString());
            }

            summary.DistinctMusicCount = musicIds.Count;

            return summary;
        }

        private static IReadOnlyList<DailyPlaybackStat> BuildDailyStats(
            IEnumerable<PlaybackHistoryItem> histories,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            var stats = new Dictionary<DateTime, DailyPlaybackStat>();

            var startDate = startTime.ToLocalTime().Date;
            var endDate = endTime.ToLocalTime().Date;

            var date = startDate;

            while (date <= endDate)
            {
                stats[date] = new DailyPlaybackStat
                {
                    Date = date,
                    TotalPlayedDuration = TimeSpan.Zero
                };

                date = date.AddDays(1);
            }

            if (histories != null)
            {
                foreach (var history in histories)
                {
                    if (!IsValidHistory(history))
                    {
                        continue;
                    }

                    var historyDate = history.PlayedAt.ToLocalTime().Date;

                    DailyPlaybackStat stat;

                    if (!stats.TryGetValue(historyDate, out stat))
                    {
                        stat = new DailyPlaybackStat
                        {
                            Date = historyDate,
                            TotalPlayedDuration = TimeSpan.Zero
                        };

                        stats[historyDate] = stat;
                    }

                    stat.PlayCount++;
                    stat.TotalPlayedDuration =
                        stat.TotalPlayedDuration + NormalizeDuration(history.PlayedDuration);

                    if (history.IsCompleted)
                    {
                        stat.CompletedPlayCount++;
                    }
                    else
                    {
                        stat.SkippedPlayCount++;
                    }
                }
            }

            var result = new List<DailyPlaybackStat>(stats.Values);
            result.Sort(CompareDailyStats);

            return result;
        }

        private async Task<Dictionary<string, Music>> BuildMusicLookupAsync()
        {
            var result = new Dictionary<string, Music>();
            var musicList = await _musicRepository.GetAllAsync();

            if (musicList == null)
            {
                return result;
            }

            for (int i = 0; i < musicList.Count; i++)
            {
                var music = musicList[i];

                if (music == null || music.Id.IsEmpty)
                {
                    continue;
                }

                result[music.Id.ToString()] = music;
            }

            return result;
        }

        private static List<MusicPlaybackStat> BuildMusicStats(
            IEnumerable<PlaybackHistoryItem> histories,
            Dictionary<string, Music> musicLookup)
        {
            var stats = new Dictionary<string, MusicPlaybackStat>();

            if (histories == null)
            {
                return new List<MusicPlaybackStat>();
            }

            foreach (var history in histories)
            {
                if (!IsValidHistory(history))
                {
                    continue;
                }

                var key = history.MusicId.ToString();

                MusicPlaybackStat stat;

                if (!stats.TryGetValue(key, out stat))
                {
                    Music music = null;

                    if (musicLookup != null)
                    {
                        musicLookup.TryGetValue(key, out music);
                    }

                    stat = new MusicPlaybackStat
                    {
                        MusicId = history.MusicId,
                        Title = music == null ? "未知歌曲" : NormalizeText(music.Title, "未知歌曲"),
                        ArtistName = music == null ? "未知艺术家" : NormalizeText(music.ArtistName, "未知艺术家"),
                        AlbumTitle = music == null ? "未知专辑" : NormalizeText(music.AlbumTitle, "未知专辑"),
                        TotalPlayedDuration = TimeSpan.Zero
                    };

                    stats[key] = stat;
                }

                ApplyHistory(stat, history);
            }

            return new List<MusicPlaybackStat>(stats.Values);
        }

        private static List<ArtistPlaybackStat> BuildArtistStats(
            IEnumerable<PlaybackHistoryItem> histories,
            Dictionary<string, Music> musicLookup)
        {
            var stats = new Dictionary<string, ArtistPlaybackStat>();
            var artistMusicIds = new Dictionary<string, HashSet<string>>();

            if (histories == null)
            {
                return new List<ArtistPlaybackStat>();
            }

            foreach (var history in histories)
            {
                if (!IsValidHistory(history))
                {
                    continue;
                }

                var musicKey = history.MusicId.ToString();

                Music music = null;

                if (musicLookup != null)
                {
                    musicLookup.TryGetValue(musicKey, out music);
                }

                var artistName = music == null
                    ? "未知艺术家"
                    : NormalizeText(music.ArtistName, "未知艺术家");

                ArtistPlaybackStat stat;

                if (!stats.TryGetValue(artistName, out stat))
                {
                    stat = new ArtistPlaybackStat
                    {
                        ArtistName = artistName,
                        TotalPlayedDuration = TimeSpan.Zero
                    };

                    stats[artistName] = stat;
                    artistMusicIds[artistName] = new HashSet<string>();
                }

                ApplyHistory(stat, history);
                artistMusicIds[artistName].Add(musicKey);
                stat.MusicCount = artistMusicIds[artistName].Count;
            }

            return new List<ArtistPlaybackStat>(stats.Values);
        }

        private static List<AlbumPlaybackStat> BuildAlbumStats(
            IEnumerable<PlaybackHistoryItem> histories,
            Dictionary<string, Music> musicLookup)
        {
            var stats = new Dictionary<string, AlbumPlaybackStat>();
            var albumMusicIds = new Dictionary<string, HashSet<string>>();

            if (histories == null)
            {
                return new List<AlbumPlaybackStat>();
            }

            foreach (var history in histories)
            {
                if (!IsValidHistory(history))
                {
                    continue;
                }

                var musicKey = history.MusicId.ToString();

                Music music = null;

                if (musicLookup != null)
                {
                    musicLookup.TryGetValue(musicKey, out music);
                }

                var albumTitle = music == null
                    ? "未知专辑"
                    : NormalizeText(music.AlbumTitle, "未知专辑");

                var artistName = music == null
                    ? "未知艺术家"
                    : NormalizeText(music.ArtistName, "未知艺术家");

                var albumKey = albumTitle + "|" + artistName;

                AlbumPlaybackStat stat;

                if (!stats.TryGetValue(albumKey, out stat))
                {
                    stat = new AlbumPlaybackStat
                    {
                        AlbumTitle = albumTitle,
                        ArtistName = artistName,
                        TotalPlayedDuration = TimeSpan.Zero
                    };

                    stats[albumKey] = stat;
                    albumMusicIds[albumKey] = new HashSet<string>();
                }

                ApplyHistory(stat, history);
                albumMusicIds[albumKey].Add(musicKey);
                stat.MusicCount = albumMusicIds[albumKey].Count;
            }

            return new List<AlbumPlaybackStat>(stats.Values);
        }

        private static void ApplyHistory(
            MusicPlaybackStat stat,
            PlaybackHistoryItem history)
        {
            stat.PlayCount++;
            stat.TotalPlayedDuration =
                stat.TotalPlayedDuration + NormalizeDuration(history.PlayedDuration);

            if (history.IsCompleted)
            {
                stat.CompletedPlayCount++;
            }
            else
            {
                stat.SkippedPlayCount++;
            }
        }

        private static void ApplyHistory(
            ArtistPlaybackStat stat,
            PlaybackHistoryItem history)
        {
            stat.PlayCount++;
            stat.TotalPlayedDuration =
                stat.TotalPlayedDuration + NormalizeDuration(history.PlayedDuration);

            if (history.IsCompleted)
            {
                stat.CompletedPlayCount++;
            }
            else
            {
                stat.SkippedPlayCount++;
            }
        }

        private static void ApplyHistory(
            AlbumPlaybackStat stat,
            PlaybackHistoryItem history)
        {
            stat.PlayCount++;
            stat.TotalPlayedDuration =
                stat.TotalPlayedDuration + NormalizeDuration(history.PlayedDuration);

            if (history.IsCompleted)
            {
                stat.CompletedPlayCount++;
            }
            else
            {
                stat.SkippedPlayCount++;
            }
        }

        private static IReadOnlyList<MusicPlaybackStat> TakeMusicStats(
            List<MusicPlaybackStat> stats,
            int maxCount)
        {
            var result = new List<MusicPlaybackStat>();

            if (stats == null || maxCount <= 0)
            {
                return result;
            }

            for (int i = 0; i < stats.Count && i < maxCount; i++)
            {
                result.Add(stats[i]);
            }

            return result;
        }

        private static IReadOnlyList<ArtistPlaybackStat> TakeArtistStats(
            List<ArtistPlaybackStat> stats,
            int maxCount)
        {
            var result = new List<ArtistPlaybackStat>();

            if (stats == null || maxCount <= 0)
            {
                return result;
            }

            for (int i = 0; i < stats.Count && i < maxCount; i++)
            {
                result.Add(stats[i]);
            }

            return result;
        }

        private static IReadOnlyList<AlbumPlaybackStat> TakeAlbumStats(
            List<AlbumPlaybackStat> stats,
            int maxCount)
        {
            var result = new List<AlbumPlaybackStat>();

            if (stats == null || maxCount <= 0)
            {
                return result;
            }

            for (int i = 0; i < stats.Count && i < maxCount; i++)
            {
                result.Add(stats[i]);
            }

            return result;
        }

        private static int CompareMusicStats(
            MusicPlaybackStat left,
            MusicPlaybackStat right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var playCountCompare = right.PlayCount.CompareTo(left.PlayCount);

            if (playCountCompare != 0)
            {
                return playCountCompare;
            }

            return right.TotalPlayedDuration.CompareTo(left.TotalPlayedDuration);
        }

        private static int CompareArtistStats(
            ArtistPlaybackStat left,
            ArtistPlaybackStat right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var playCountCompare = right.PlayCount.CompareTo(left.PlayCount);

            if (playCountCompare != 0)
            {
                return playCountCompare;
            }

            return right.TotalPlayedDuration.CompareTo(left.TotalPlayedDuration);
        }

        private static int CompareAlbumStats(
            AlbumPlaybackStat left,
            AlbumPlaybackStat right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var playCountCompare = right.PlayCount.CompareTo(left.PlayCount);

            if (playCountCompare != 0)
            {
                return playCountCompare;
            }

            return right.TotalPlayedDuration.CompareTo(left.TotalPlayedDuration);
        }

        private static int CompareDailyStats(
            DailyPlaybackStat left,
            DailyPlaybackStat right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Date.CompareTo(right.Date);
        }

        private static bool IsValidHistory(PlaybackHistoryItem history)
        {
            if (history == null)
            {
                return false;
            }

            if (history.MusicId.IsEmpty)
            {
                return false;
            }

            return true;
        }

        private static TimeSpan NormalizeDuration(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            return duration;
        }

        private static string NormalizeText(
            string value,
            string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback ?? string.Empty;
            }

            return value.Trim();
        }

        private static DateTimeOffset GetStartOfLocalDay(
            DateTimeOffset value)
        {
            var local = value.ToLocalTime();

            return new DateTimeOffset(
                local.Year,
                local.Month,
                local.Day,
                0,
                0,
                0,
                local.Offset);
        }

        private static void ValidateDateRange(
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            if (endTime < startTime)
            {
                throw new ArgumentException(
                    "End time cannot be earlier than start time.",
                    nameof(endTime));
            }
        }
    }
}

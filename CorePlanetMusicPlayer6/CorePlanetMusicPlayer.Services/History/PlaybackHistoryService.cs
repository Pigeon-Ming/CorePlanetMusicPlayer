using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.History;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.History
{
    public sealed class PlaybackHistoryService : IPlaybackHistoryService
    {
        /// <summary>
        /// 累计播放时长达到歌曲总时长的该比例时，认为播放完成。
        /// </summary>
        private const double CompletedThreshold = 0.8;

        private readonly IPlaybackHistoryRepository _historyRepository;

        public PlaybackHistoryService(IPlaybackHistoryRepository historyRepository)
        {
            Guard.NotNull(historyRepository, nameof(historyRepository));

            _historyRepository = historyRepository;
        }

        public Task RecordPlaybackAsync(
            PlaybackHistoryId historyId, 
            MusicId musicId, 
            DateTimeOffset playedAt, 
            TimeSpan musicDuration, 
            TimeSpan playedDuration, 
            TimeSpan lastPosition,
            string titleSnapshot,
            string artistNameSnapshot,
            string albumTitleSnapshot)
        {
            if (historyId.IsEmpty)
            {
                throw new ArgumentException("历史记录 ID 不能为空。", nameof(historyId));
            }

            ValidateMusicId(musicId);

            if (playedAt == default(DateTimeOffset))
            {
                throw new ArgumentException("播放开始时间不能为空。", nameof(playedAt));
            }

            Guard.NotNegative(musicDuration, nameof(musicDuration));

            Guard.NotNegative(playedDuration, nameof(playedDuration));

            Guard.NotNegative(lastPosition, nameof(lastPosition));

            // 未产生有效播放时长时，不写入历史。
            if (playedDuration == TimeSpan.Zero)
            {
                return Task.CompletedTask;
            }

            var item = new PlaybackHistoryItem
            {
                Id = historyId,
                MusicId = musicId,
                TitleSnapshot = titleSnapshot ?? string.Empty,
                ArtistNameSnapshot = artistNameSnapshot ?? string.Empty,
                AlbumTitleSnapshot = albumTitleSnapshot ?? string.Empty,
                PlayedAt = playedAt,
                MusicDuration = musicDuration,
                PlayedDuration = playedDuration,

                LastPosition = NormalizePosition(lastPosition, musicDuration),

                IsCompleted = IsCompleted(musicDuration, playedDuration)
            };

            return _historyRepository.UpsertAsync(item);
        }

        private void ValidateMusicId(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }
        }

        private static TimeSpan NormalizePosition(TimeSpan position, TimeSpan duration)
        {
            if (position < TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            if (duration > TimeSpan.Zero && position > duration)
            {
                return duration;
            }

            return position;
        }

        public Task<IReadOnlyList<PlaybackHistoryItem>> GetRecentAsync(
            int maxCount)
        {
            Guard.NotNegative(maxCount, nameof(maxCount));

            return _historyRepository.GetRecentAsync(maxCount);
        }

        public Task<IReadOnlyList<PlaybackHistoryItem>> GetByMusicIdAsync(
            MusicId musicId)
        {
            ValidateMusicId(musicId);

            return _historyRepository.GetByMusicIdAsync(musicId);
        }

        public Task<IReadOnlyList<PlaybackHistoryItem>> GetByDateRangeAsync(DateTimeOffset? startTime = null, DateTimeOffset? endTime = null)
        {
            if (startTime.HasValue && endTime.HasValue && endTime.Value < startTime.Value)
            {
                throw new ArgumentException("结束时间不能早于开始时间。", nameof(endTime));
            }

            return _historyRepository.GetByDateRangeAsync(startTime, endTime);
        }

        public Task DeleteAsync(PlaybackHistoryId historyId)
        {
            if (historyId.IsEmpty)
            {
                throw new ArgumentException("History id cannot be empty.", nameof(historyId));
            }

            return _historyRepository.DeleteAsync(historyId);
        }

        public Task DeleteBeforeAsync(DateTimeOffset time)
        {
            return _historyRepository.DeleteBeforeAsync(time);
        }

        public Task ClearAsync()
        {
            return _historyRepository.ClearAsync();
        }

        public bool IsCompleted(TimeSpan musicDuration, TimeSpan playedDuration)
        {
            if (musicDuration <= TimeSpan.Zero)
            {
                return false;
            }

            if (playedDuration <= TimeSpan.Zero)
            {
                return false;
            }

            var ratio = playedDuration.TotalMilliseconds
                / musicDuration.TotalMilliseconds;

            return ratio >= CompletedThreshold;
        }
    }
}

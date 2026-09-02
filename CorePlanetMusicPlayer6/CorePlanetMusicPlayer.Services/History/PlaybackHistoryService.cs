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
        /// 播放进度超过该阈值时，认为播放完成
        /// </summary>
        private const double CompletedThreshold = 0.8;

        private readonly IPlaybackHistoryRepository _historyRepository;

        public PlaybackHistoryService(IPlaybackHistoryRepository historyRepository)
        {
            Guard.NotNull(historyRepository, nameof(historyRepository));

            _historyRepository = historyRepository;
        }

        public async Task RecordPlaybackAsync(MusicId musicId, TimeSpan musicDuration, TimeSpan playedDuration, TimeSpan lastPosition)
        {
            ValidateMusicId(musicId);

            Guard.NotNegative(musicDuration, nameof(musicDuration));
            Guard.NotNegative(playedDuration, nameof(playedDuration));
            Guard.NotNegative(lastPosition, nameof(lastPosition));

            var normalizedLastPosition = NormalizePosition(lastPosition, musicDuration);

            var item = new PlaybackHistoryItem
            {
                Id = PlaybackHistoryId.NewId(),
                MusicId = musicId,
                PlayedAt = DateTimeOffset.Now,
                MusicDuration = musicDuration,
                PlayedDuration = playedDuration,
                LastPosition = normalizedLastPosition,
                IsCompleted = IsCompleted(musicDuration, playedDuration)
            };

            await _historyRepository.AddAsync(item);
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

        public Task<IReadOnlyList<PlaybackHistoryItem>> GetByDateRangeAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            if (endTime < startTime)
            {
                throw new ArgumentException("End time cannot be earlier than start time.", nameof(endTime));
            }

            return _historyRepository.GetByDateRangeAsync(
                startTime,
                endTime);
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

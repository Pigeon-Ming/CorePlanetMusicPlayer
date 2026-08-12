using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.History
{
    public sealed class PlaybackHistoryItem
    {
        public PlaybackHistoryId Id { get; set; }

        public MusicId MusicId { get; set; }

        public DateTimeOffset PlayedAt { get; set; }

        public TimeSpan MusicDuration { get; set; }

        public TimeSpan PlayedDuration { get; set; }

        public TimeSpan LastPosition { get; set; }

        /// <summary>
        /// 【待评估实际作用】
        /// </summary>
        public bool IsCompleted { get; set; }

        public bool HasValidMusic
        {
            get { return !MusicId.IsEmpty; }
        }

        public bool HasPlayed
        {
            get { return PlayedDuration > TimeSpan.Zero; }
        }

        public static PlaybackHistoryItem Create(MusicId musicId, TimeSpan musicDuration)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("MusicId cannot be empty.", nameof(musicId));
            }

            Guard.NotNegative(musicDuration, nameof(musicDuration));

            return new PlaybackHistoryItem
            {
                Id = PlaybackHistoryId.NewId(),
                MusicId = musicId,
                PlayedAt = DateTimeOffset.Now,
                MusicDuration = musicDuration,
                PlayedDuration = TimeSpan.Zero,
                LastPosition = TimeSpan.Zero,
                IsCompleted = false
            };
        }

        public void UpdateProgress(TimeSpan playedDuration, TimeSpan lastPosition)
        {
            Guard.NotNegative(playedDuration, nameof(playedDuration));
            Guard.NotNegative(lastPosition, nameof(lastPosition));

            PlayedDuration = playedDuration;
            LastPosition = NormalizePosition(lastPosition);
        }

        public void MarkCompleted()
        {
            IsCompleted = true;

            if (MusicDuration > TimeSpan.Zero)
            {
                LastPosition = MusicDuration;
            }
        }

        public void MarkStopped(TimeSpan playedDuration, TimeSpan lastPosition, bool isCompleted)
        {
            UpdateProgress(playedDuration, lastPosition);

            IsCompleted = isCompleted;

            if (IsCompleted && MusicDuration > TimeSpan.Zero)
            {
                LastPosition = MusicDuration;
            }
        }

        private TimeSpan NormalizePosition(TimeSpan position)
        {
            if (position < TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            if (MusicDuration > TimeSpan.Zero && position > MusicDuration)
            {
                return MusicDuration;
            }

            return position;
        }
    }
}

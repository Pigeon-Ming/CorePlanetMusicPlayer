using CorePlanetMusicPlayer.Core.History;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Hisrory
{
    /// <summary>
    /// 一次播放过程在某个时刻的历史数据。
    /// 创建后保持不变。
    /// </summary>
    public sealed class PlaybackHistorySnapshot
    {
        public PlaybackHistoryId HistoryId { get; }

        public MusicId MusicId { get; }

        public string TitleSnapshot { get; }

        public string ArtistNameSnapshot { get; }

        public string AlbumTitleSnapshot { get; }

        public DateTimeOffset PlayedAt { get; }

        public TimeSpan MusicDuration { get; }

        public TimeSpan PlayedDuration { get; }

        public TimeSpan LastPosition { get; }

        internal PlaybackHistorySnapshot(
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
            HistoryId = historyId;
            MusicId = musicId;
            PlayedAt = playedAt;
            MusicDuration = musicDuration;
            PlayedDuration = playedDuration;
            LastPosition = lastPosition;

            TitleSnapshot = titleSnapshot ?? string.Empty;
            ArtistNameSnapshot = artistNameSnapshot ?? string.Empty;
            AlbumTitleSnapshot = albumTitleSnapshot ?? string.Empty;
        }
    }
}

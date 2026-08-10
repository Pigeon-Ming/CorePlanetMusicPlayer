using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Music
{
    /// <summary>
    /// 表示单首音乐
    /// </summary>
    public class Music
    {
        public MusicId Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string AlbumTitle { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public TimeSpan Duration { get; set; }

        public MusicSourceType SourceType { get; set; }

        public MusicMetadata Metadata { get; set; } = MusicMetadata.Empty;

        public MusicFileInfo FileInfo { get; set; }

        public DateTimeOffset? AddedAt { get; set; }

        public DateTimeOffset? LastPlayedAt { get; set; }

        public bool IsLocal
        {
            get { return SourceType == MusicSourceType.Local; }
        }

        public bool IsStream
        {
            get { return SourceType == MusicSourceType.Stream; }
        }

        public bool IsTemporary
        {
            get { return SourceType == MusicSourceType.Temporary; }
        }

        public static Music CreateLocal(string title, string artistName, string albumTitle, TimeSpan duration, MusicFileInfo fileInfo)
        {
            Guard.NotNullOrWhiteSpace(title, nameof(title));
            Guard.NotNegative(duration, nameof(duration));
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return new Music
            {
                Id = MusicId.NewId(),
                Title = title,
                ArtistName = artistName ?? string.Empty,
                AlbumTitle = albumTitle ?? string.Empty,
                Duration = duration,
                SourceType = MusicSourceType.Local,
                Metadata = MusicMetadata.Empty,
                FileInfo = fileInfo,
                AddedAt = DateTimeOffset.Now
            };
        }

        public static Music CreateStream(string title, string artistName, string albumTitle, TimeSpan duration)
        {
            Guard.NotNullOrWhiteSpace(title, nameof(title));
            Guard.NotNegative(duration, nameof(duration));

            return new Music
            {
                Id = MusicId.NewId(),
                Title = title,
                ArtistName = artistName ?? string.Empty,
                AlbumTitle = albumTitle ?? string.Empty,
                Duration = duration,
                SourceType = MusicSourceType.Stream,
                Metadata = MusicMetadata.Empty,
                FileInfo = null,
                AddedAt = DateTimeOffset.Now
            };
        }

        public static Music CreateTemporary(string title, string artistName, string albumTitle, TimeSpan duration, MusicFileInfo fileInfo)
        {
            Guard.NotNullOrWhiteSpace(title, nameof(title));
            Guard.NotNegative(duration, nameof(duration));
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return new Music
            {
                Id = MusicId.NewId(),
                Title = title,
                ArtistName = artistName ?? string.Empty,
                AlbumTitle = albumTitle ?? string.Empty,
                Duration = duration,
                Metadata = MusicMetadata.Empty,
                FileInfo = fileInfo,
                AddedAt = DateTimeOffset.Now
            };
        }
    }
}

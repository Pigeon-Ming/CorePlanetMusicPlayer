using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Music
{
    /// <summary>
    /// 单首音乐的核心
    /// </summary>
    public class Music
    {
        public MusicId Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string AlbumTitle { get; set; }

        public string ArtistName { get;set; }

        public TimeSpan Duration { get; set; }

        public MusicSourceType SourceType { get; set; }

        public MusicMetadata Metadata { get; set; }

        public MusicFileInfo FileInfo { get; set; }

        public DateTimeOffset AddedAt { get; set; }

        public DateTimeOffset LastPlayedAt { get; set; }
    }
}

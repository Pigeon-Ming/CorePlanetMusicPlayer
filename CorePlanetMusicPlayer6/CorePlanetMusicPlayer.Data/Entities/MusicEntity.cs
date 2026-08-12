using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Entities
{
    public sealed class MusicEntity
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string AlbumTitle { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public string AlbumArtistName { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public int? Year { get; set; }

        public int? TrackNumber { get; set; }

        public int? DiscNumber { get; set; }

        public string Composer { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public long DurationTicks { get; set; }

        public int SourceType { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public string RelativePath { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public long? Size { get; set; }

        public long? LastModifiedAtUnixTimeMilliseconds { get; set; }

        public string LibraryFolderId { get; set; } = string.Empty;

        public long? AddedAtUnixTimeMilliseconds { get; set; }

        public long? LastPlayedAtUnixTimeMilliseconds { get; set; }


    }
}

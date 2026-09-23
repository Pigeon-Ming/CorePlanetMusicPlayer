using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Entities
{
    public sealed class LibraryFolderEntity
    {
        public string Id { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public int AccessKind { get; set; }

        public long AddedAtUnixTimeMilliseconds { get; set; }

        public long UpdatedAtUnixTimeMilliseconds { get; set; }
    }
}

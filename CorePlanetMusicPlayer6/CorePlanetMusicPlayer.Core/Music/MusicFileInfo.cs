using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Music
{
    /// <summary>
    /// 本地文件信息
    /// </summary>
    public sealed class MusicFileInfo
    {
        public string Path { get; set; } = string.Empty;

        public string RelativePath { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        public long? Size { get; set; }

        public DateTimeOffset? LastModifiedAt { get; set; }

        public string LibraryFolderId { get; set; } = string.Empty;

        public bool HasPath
        {
            get { return !string.IsNullOrWhiteSpace(Path); }
        }

        public bool HasRelativePath
        {
            get { return !string.IsNullOrWhiteSpace(RelativePath); }
        }

        public bool HasLibraryFolder
        {
            get { return !string.IsNullOrWhiteSpace(LibraryFolderId); }
        }
    }
}

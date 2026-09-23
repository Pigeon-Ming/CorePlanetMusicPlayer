using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Music
{
    /// <summary>
    /// 流媒体音乐的扩展类
    /// </summary>
    public sealed class StreamMusic
    {
        public Music Music { get; set; }

        public Uri SourceUri { get; set; }

        public string ProviderName { get; set; } = string.Empty;

        public bool HasSource
        {
            get { return SourceUri != null; }
        }

        public static StreamMusic Create(Music music, Uri SourceUri, string providerName)
        {
            Guard.NotNull(music, nameof(music));
            Guard.NotNull(SourceUri, nameof(SourceUri));

            return new StreamMusic
            {
                Music = music,
                SourceUri = SourceUri,
                ProviderName = providerName
            };
        }
    }
}

using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Services.Lyrics;
using CorePlanetMusicPlayer.Services.Metadata;
using CorePlanetMusicPlayer.Uwp.Platform.Metadata;

namespace CorePlanetMusicPlayer.TagLib.Uwp.Composition
{
    public sealed class TagLibFeatureSet
    {
        public TagLibFeatureSet(
            IEmbeddedLyricAccessor embeddedLyricAccessor,
            IUwpMusicTagAccessor musicTagAccessor,
            MusicTagCapabilities capabilities)
        {
            Guard.NotNull(
                embeddedLyricAccessor,
                nameof(embeddedLyricAccessor));
            Guard.NotNull(capabilities, nameof(capabilities));

            EmbeddedLyricAccessor = embeddedLyricAccessor;
            MusicTagAccessor = musicTagAccessor;
            Capabilities = capabilities;
        }

        public IEmbeddedLyricAccessor EmbeddedLyricAccessor
        {
            get;
            private set;
        }

        public MusicTagCapabilities Capabilities
        {
            get;
            private set;
        }

        public IUwpMusicTagAccessor MusicTagAccessor
        {
            get;
            private set;
        }
    }
}

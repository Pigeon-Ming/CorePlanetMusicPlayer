using CorePlanetMusicPlayer.Core.Music;
using System;

namespace CorePlanetMusicPlayer.Services.Metadata
{
    public sealed class MusicTagReadResult
    {
        public MusicTagReadResult(
            MusicMetadata metadata,
            TimeSpan duration)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            if (duration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }

            Metadata = metadata;
            Duration = duration;
        }

        public MusicMetadata Metadata { get; private set; }

        public TimeSpan Duration { get; private set; }
    }
}

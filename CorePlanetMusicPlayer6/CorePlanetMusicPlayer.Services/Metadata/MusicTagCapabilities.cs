namespace CorePlanetMusicPlayer.Services.Metadata
{
    public sealed class MusicTagCapabilities
    {
        public bool IsTagLibExtensionInstalled { get; set; }

        public bool IsTagLibPlatformSupported { get; set; }

        public bool CanReadTagsWithTagLib { get; set; }

        public bool CanWriteTagsWithTagLib { get; set; }

        public bool CanReadEmbeddedLyrics { get; set; }

        public bool CanWriteEmbeddedLyrics { get; set; }

        public string UnavailableReason { get; set; } = string.Empty;

        public bool IsTagLibAvailable
        {
            get
            {
                return IsTagLibExtensionInstalled &&
                       IsTagLibPlatformSupported;
            }
        }
    }
}

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class ExternalLyricSearchOptions
    {
        public string FileNameRegex { get; set; } = string.Empty;

        public static ExternalLyricSearchOptions CreateDefault()
        {
            return new ExternalLyricSearchOptions
            {
                FileNameRegex = string.Empty
            };
        }
    }
}

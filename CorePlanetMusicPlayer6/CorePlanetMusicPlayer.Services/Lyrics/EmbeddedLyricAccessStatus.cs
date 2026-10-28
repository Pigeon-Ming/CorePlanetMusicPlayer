namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public enum EmbeddedLyricAccessStatus
    {
        Success = 0,

        NotFound = 1,

        Unsupported = 2,

        AccessDenied = 3,

        Busy = 4,

        Failed = 5
    }
}

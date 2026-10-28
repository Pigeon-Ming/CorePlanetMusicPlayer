namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class LyricSaveResult
    {
        private LyricSaveResult()
        {
        }

        public bool Succeeded { get; private set; }

        public LyricSaveTarget Target { get; private set; }

        public bool CanFallbackToDatabase { get; private set; }

        public string Message { get; private set; } = string.Empty;

        public string ErrorMessage { get; private set; } = string.Empty;

        public static LyricSaveResult Success(LyricSaveTarget target, string message)
        {
            return new LyricSaveResult
            {
                Succeeded = true,
                Target = target,
                Message = message ?? string.Empty
            };
        }

        public static LyricSaveResult Failure(LyricSaveTarget target, string errorMessage, bool canFallbackToDatabase)
        {
            return new LyricSaveResult
            {
                Target = target,
                ErrorMessage = errorMessage ?? string.Empty,
                CanFallbackToDatabase = canFallbackToDatabase
            };
        }
    }
}

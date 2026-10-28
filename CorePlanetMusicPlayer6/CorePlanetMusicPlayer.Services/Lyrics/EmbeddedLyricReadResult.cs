namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class EmbeddedLyricReadResult
    {
        private EmbeddedLyricReadResult()
        {
            Status = EmbeddedLyricAccessStatus.Failed;
        }

        public EmbeddedLyricAccessStatus Status { get; private set; }

        public string RawText { get; private set; } = string.Empty;

        public string ErrorMessage { get; private set; } = string.Empty;

        public bool Found
        {
            get { return Status == EmbeddedLyricAccessStatus.Success; }
        }

        public static EmbeddedLyricReadResult Success(string rawText)
        {
            return new EmbeddedLyricReadResult
            {
                Status = EmbeddedLyricAccessStatus.Success,
                RawText = rawText ?? string.Empty
            };
        }

        public static EmbeddedLyricReadResult NotFound()
        {
            return Create(EmbeddedLyricAccessStatus.NotFound, string.Empty);
        }

        public static EmbeddedLyricReadResult Failure(EmbeddedLyricAccessStatus status, string errorMessage)
        {
            return Create(status, errorMessage);
        }

        private static EmbeddedLyricReadResult Create(EmbeddedLyricAccessStatus status, string errorMessage)
        {
            return new EmbeddedLyricReadResult
            {
                Status = status,
                ErrorMessage = errorMessage ?? string.Empty
            };
        }
    }
}

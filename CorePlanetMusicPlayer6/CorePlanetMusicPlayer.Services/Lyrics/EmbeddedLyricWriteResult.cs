namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class EmbeddedLyricWriteResult
    {
        private EmbeddedLyricWriteResult()
        {
            Status = EmbeddedLyricAccessStatus.Failed;
        }

        public EmbeddedLyricAccessStatus Status { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public bool Succeeded
        {
            get { return Status == EmbeddedLyricAccessStatus.Success; }
        }

        public static EmbeddedLyricWriteResult Success()
        {
            return new EmbeddedLyricWriteResult
            {
                Status = EmbeddedLyricAccessStatus.Success
            };
        }

        public static EmbeddedLyricWriteResult Failure(EmbeddedLyricAccessStatus status, string errorMessage)
        {
            return new EmbeddedLyricWriteResult
            {
                Status = status,
                ErrorMessage = errorMessage ?? string.Empty
            };
        }
    }
}

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class ExternalLyricWriteResult
    {
        private ExternalLyricWriteResult()
        {
            Status = ExternalLyricWriteStatus.Failed;
        }

        public ExternalLyricWriteStatus Status { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public bool Succeeded
        {
            get { return Status == ExternalLyricWriteStatus.Success; }
        }

        public static ExternalLyricWriteResult Success()
        {
            return new ExternalLyricWriteResult
            {
                Status = ExternalLyricWriteStatus.Success
            };
        }

        public static ExternalLyricWriteResult Failure(ExternalLyricWriteStatus status, string errorMessage)
        {
            return new ExternalLyricWriteResult
            {
                Status = status,
                ErrorMessage = errorMessage ?? string.Empty
            };
        }
    }
}

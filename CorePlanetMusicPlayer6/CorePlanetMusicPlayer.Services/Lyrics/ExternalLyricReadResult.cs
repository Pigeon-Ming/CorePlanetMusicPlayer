using System;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class ExternalLyricReadResult
    {
        public bool Found { get; private set; }

        public string FileName { get; private set; } = string.Empty;

        public string RelativePath { get; private set; } = string.Empty;

        public string RawText { get; private set; } = string.Empty;

        public DateTimeOffset? LastModifiedAt { get; private set; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public static ExternalLyricReadResult NotFound()
        {
            return new ExternalLyricReadResult();
        }

        public static ExternalLyricReadResult Success(
            string fileName,
            string relativePath,
            string rawText,
            DateTimeOffset? lastModifiedAt)
        {
            return new ExternalLyricReadResult
            {
                Found = true,
                FileName = fileName ?? string.Empty,
                RelativePath = relativePath ?? string.Empty,
                RawText = rawText ?? string.Empty,
                LastModifiedAt = lastModifiedAt
            };
        }

        public static ExternalLyricReadResult Failure(string errorMessage)
        {
            return new ExternalLyricReadResult
            {
                ErrorMessage = errorMessage ?? string.Empty
            };
        }
    }
}

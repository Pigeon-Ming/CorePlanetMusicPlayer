using CorePlanetMusicPlayer.Core.Music;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class UnsupportedEmbeddedLyricAccessor
        : IEmbeddedLyricAccessor
    {
        private readonly string _unavailableReason;

        public UnsupportedEmbeddedLyricAccessor(
            string unavailableReason)
        {
            _unavailableReason = string.IsNullOrWhiteSpace(
                unavailableReason)
                ? "当前环境不支持内嵌歌词标签。"
                : unavailableReason.Trim();
        }

        public Task<EmbeddedLyricReadResult> ReadAsync(Music music)
        {
            return Task.FromResult(
                EmbeddedLyricReadResult.Failure(
                    EmbeddedLyricAccessStatus.Unsupported,
                    _unavailableReason));
        }

        public Task<EmbeddedLyricWriteResult> WriteAsync(
            Music music,
            string rawText)
        {
            return Task.FromResult(
                EmbeddedLyricWriteResult.Failure(
                    EmbeddedLyricAccessStatus.Unsupported,
                    _unavailableReason));
        }
    }
}

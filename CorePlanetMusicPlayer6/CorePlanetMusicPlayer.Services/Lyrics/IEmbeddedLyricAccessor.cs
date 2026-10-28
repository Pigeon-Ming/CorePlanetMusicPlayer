using CorePlanetMusicPlayer.Core.Music;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public interface IEmbeddedLyricAccessor
    {
        Task<EmbeddedLyricReadResult> ReadAsync(Music music);

        Task<EmbeddedLyricWriteResult> WriteAsync(Music music, string rawText);
    }
}

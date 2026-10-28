using CorePlanetMusicPlayer.Core.Music;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public interface IExternalLyricWriter
    {
        Task<ExternalLyricWriteResult> WriteAsync(Music music, string relativePath, string rawText);
    }
}

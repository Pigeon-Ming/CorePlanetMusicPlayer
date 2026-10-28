using CorePlanetMusicPlayer.Core.Music;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public interface IExternalLyricReader
    {
        Task<ExternalLyricReadResult> FindAsync(Music music, ExternalLyricSearchOptions options);
    }
}

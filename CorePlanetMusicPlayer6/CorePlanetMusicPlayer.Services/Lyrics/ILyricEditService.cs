using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public interface ILyricEditService
    {
        Task<LyricSaveResult> SaveAsync(MusicId musicId, string rawText, LyricSaveTarget target, LyricDocument sourceDocument);
    }
}

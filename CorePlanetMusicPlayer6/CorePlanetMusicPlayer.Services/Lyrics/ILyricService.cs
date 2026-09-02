using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public interface ILyricService
    {
        Task<LyricDocument> GetPreferredByMusicIdAsync(MusicId musicId);

        Task<IReadOnlyList<LyricDocument>> GetAllByMusicIdAsync(MusicId musicId);

        Task<LyricDocument> SaveManualLyricsAsync(MusicId musicId, string rawText);

        Task<LyricDocument> SaveExternalLyricsAsync(MusicId musicId, string rawText);

        Task<LyricDocument> SaveEmbeddedLyricsAsync(MusicId musicId, string rawText);

        Task<LyricDocument> SaveOnlineLyricsAsync(MusicId musicId, string rawText);

        Task<LyricLine> GetCurrentLineAsync(MusicId musicId, TimeSpan position);

        Task DeleteAsync(string lyricId);

        Task DeleteByMusicIdAsync(MusicId musicId);
    }
}

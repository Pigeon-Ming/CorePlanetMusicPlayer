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

        Task<LyricDocument> SaveOnlineLyricsAsync(MusicId musicId, string rawText);

        Task<LyricDocument> SaveUserOverrideAsync(MusicId musicId, LyricSourceType sourceType, string sourcePath, string rawText);

        Task DeleteUserOverrideAsync(MusicId musicId);

        Task<LyricLine> GetCurrentLineAsync(MusicId musicId, TimeSpan position);

        Task DeleteAsync(string lyricId);

        Task DeleteByMusicIdAsync(MusicId musicId);
    }
}

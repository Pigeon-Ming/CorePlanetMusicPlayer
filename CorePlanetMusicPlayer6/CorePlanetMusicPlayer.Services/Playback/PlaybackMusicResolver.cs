using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Playback.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Playback
{
    public sealed class PlaybackMusicResolver : IPlaybackMusicResolver
    {
        private readonly IMusicRepository _musicRepository;

        public PlaybackMusicResolver(IMusicRepository musicRepository)
        {
            _musicRepository = musicRepository ?? throw new ArgumentNullException(nameof(musicRepository));
        }

        public Task<Music> GetByIdAsync(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("歌曲 ID 不能为空。", nameof(musicId));
            }

            return _musicRepository.GetByIdAsync(musicId);
        }
    }
}

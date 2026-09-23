using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.MusicQueries
{
    public sealed class MusicQueryService : IMusicQueryService
    {
        private readonly IMusicRepository _musicRepository;

        public MusicQueryService(IMusicRepository musicRepository)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));

            _musicRepository = musicRepository;
        }

        public Task<IReadOnlyList<Music>> GetAllAsync()
        {
            return _musicRepository.GetAllAsync();
        }

        public Task<Music> GetByIdAsync(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("歌曲 ID 不能为空。", nameof(musicId));
            }

            return _musicRepository.GetByIdAsync(musicId);
        }

        public Task<IReadOnlyList<Music>> SearchAsync(string keyword)
        {
            return _musicRepository.SearchAsync(keyword);
        }

        public Task<IReadOnlyList<Music>> GetByFolderAsync(LibraryFolderId folderId)
        {
            if (folderId.IsEmpty)
            {
                throw new ArgumentException("来源 ID 不能为空。", nameof(folderId));
            }

            return _musicRepository.GetByLibraryFolderIdAsync(folderId);
        }

        public async Task<IReadOnlyList<Music>> GetByIdsAsync(IEnumerable<MusicId> musicIds)
        {
            if (musicIds == null)
            {
                throw new ArgumentNullException(nameof(musicIds));
            }

            // 先完整读取、校验输入，再开始查询。
            var requestedIds = musicIds.ToList();

            if (requestedIds.Any(id => id.IsEmpty))
            {
                throw new ArgumentException("歌曲 ID 列表包含空 ID。", nameof(musicIds));
            }

            var result = new List<Music>();

            // 只在本次调用内缓存，避免重复查询相同 ID。
            var queriedMusic = new Dictionary<MusicId, Music>();

            foreach (var musicId in requestedIds)
            {
                Music music;

                if (!queriedMusic.TryGetValue(musicId, out music))
                {
                    music = await _musicRepository.GetByIdAsync(musicId);

                    // 不存在的结果也记住，避免重复查询。
                    queriedMusic.Add(musicId, music);
                }

                if (music != null)
                {
                    result.Add(music);
                }
            }

            return result.AsReadOnly();
        }
    }
}

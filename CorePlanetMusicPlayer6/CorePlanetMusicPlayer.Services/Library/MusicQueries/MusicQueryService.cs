using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Library;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        public async Task<IReadOnlyList<Music>> GetByIdsAsync(
            IEnumerable<MusicId> musicIds,
            CancellationToken cancellationToken = default(CancellationToken))
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

            cancellationToken.ThrowIfCancellationRequested();

            var queriedMusic = await _musicRepository.GetByIdsAsync(
                requestedIds,
                cancellationToken);

            // Repository 不保证顺序，也不会保留重复项。
            var musicById = queriedMusic.ToDictionary(music => music.Id);

            var result = new List<Music>(requestedIds.Count);

            // 按调用者的输入重新组织结果。
            foreach (var musicId in requestedIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Music music;

                if (musicById.TryGetValue(musicId, out music))
                {
                    result.Add(music);
                }
            }

            return result.AsReadOnly();
        }
    }
}

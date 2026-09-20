using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Genres
{
    public sealed class GenreQueryService : IGenreQueryService
    {
        private readonly IMusicRepository _musicRepository;

        public GenreQueryService(IMusicRepository musicRepository)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));

            _musicRepository = musicRepository;
        }

        public async Task<IReadOnlyList<GenreGroup>> GetAllAsync()
        {
            var musicItems = await _musicRepository.GetAllAsync();

            var groups = musicItems
                .Where(music => music != null && !music.Id.IsEmpty)
                .Select(music => NormalizeGenre(music.Metadata?.Genre))
                .GroupBy(
                    genre => genre,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group => new GenreGroup
                {
                    // 同一组可能包含 Rock、rock 等写法。
                    // 按固定规则选择显示名称，不依赖歌曲读取顺序。
                    Genre = group
                        .OrderBy(name => name, StringComparer.Ordinal)
                        .First(),

                    MusicCount = group.Count()
                })
                .OrderBy(group => group.Genre == null ? 1 : 0)
                .ThenBy(
                    group => group.Genre,
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

            return groups.AsReadOnly();
        }

        public async Task<IReadOnlyList<Music>> GetMusicAsync(
            string genre)
        {
            string requestedGenre = NormalizeGenre(genre);

            var musicItems = await _musicRepository.GetAllAsync();

            var result = musicItems
                .Where(music => music != null && !music.Id.IsEmpty)
                .Where(music => string.Equals(
                    NormalizeGenre(music.Metadata?.Genre),
                    requestedGenre,
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(
                    music => music.Title?.Trim() ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    music => music.Id.ToString(),
                    StringComparer.Ordinal)
                .ToList();

            return result.AsReadOnly();
        }

        private static string NormalizeGenre(string genre)
        {
            return string.IsNullOrWhiteSpace(genre)
                ? null
                : genre.Trim();
        }
    }
}

using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Library.Years
{
    public sealed class YearQueryService : IYearQueryService
    {
        private readonly IMusicRepository _musicRepository;

        public YearQueryService(IMusicRepository musicRepository)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));

            _musicRepository = musicRepository;
        }

        public async Task<IReadOnlyList<YearGroup>> GetAllAsync()
        {
            var musicItems = await _musicRepository.GetAllAsync();

            var groups = musicItems
                .Where(music => music != null && !music.Id.IsEmpty)
                .GroupBy(music => NormalizeYear(music.Metadata?.Year))
                .Select(group => new YearGroup
                {
                    Year = group.Key,
                    MusicCount = group.Count()
                })
                .OrderBy(group => group.Year.HasValue ? 0 : 1)
                .ThenByDescending(group => group.Year)
                .ToList();

            return groups.AsReadOnly();
        }

        public async Task<IReadOnlyList<Music>> GetMusicAsync(
            int? year)
        {
            int? requestedYear = NormalizeYear(year);

            var musicItems = await _musicRepository.GetAllAsync();

            var result = musicItems
                .Where(music => music != null && !music.Id.IsEmpty)
                .Where(music =>
                    NormalizeYear(music.Metadata?.Year) == requestedYear)
                .OrderBy(
                    music => music.Title?.Trim() ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    music => music.Id.ToString(),
                    StringComparer.Ordinal)
                .ToList();

            return result.AsReadOnly();
        }

        private static int? NormalizeYear(int? year)
        {
            if (!year.HasValue || year.Value <= 0)
            {
                return null;
            }

            return year.Value;
        }
    }
}

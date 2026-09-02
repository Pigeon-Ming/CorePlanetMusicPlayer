using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class LyricService : ILyricService
    {
        private readonly ILyricRepository _lyricRepository;
        private readonly ILyricParser _parser;
        private readonly LyricSearchService _searchService;

        public LyricService(ILyricRepository lyricRepository, ILyricParser parser, LyricSearchService searchService)
        {
            Guard.NotNull(lyricRepository, nameof(lyricRepository));

            _lyricRepository = lyricRepository;
            _parser = parser ?? new LyricParserCollection(new ILyricParser[]{ new LrcParser()});
            _searchService = searchService ?? new LyricSearchService();
        }

        public async Task<LyricDocument> GetPreferredByMusicIdAsync(MusicId musicId)
        {
            ValidateMusicId(musicId);

            var documents = await _lyricRepository.GetAllByMusicIdAsync(musicId);
            var selected = _searchService.SelectPreferred(documents);

            if (selected == null)
            {
                return null;
            }

            EnsureParsedLines(selected);

            return selected;
        }

        public Task<IReadOnlyList<LyricDocument>> GetAllByMusicIdAsync(MusicId musicId)
        {
            ValidateMusicId(musicId);

            return _lyricRepository.GetAllByMusicIdAsync(musicId);
        }

        public Task<LyricDocument> SaveManualLyricsAsync(MusicId musicId, string rawText)
        {
            return SaveLyricCoreAsync(musicId, LyricSourceType.Manual, string.Empty, rawText);
        }

        public Task<LyricDocument> SaveExternalLyricsAsync(MusicId musicId, string rawText)
        {
            return SaveLyricCoreAsync(musicId, LyricSourceType.ExternalFile, string.Empty, rawText);
        }

        public Task<LyricDocument> SaveEmbeddedLyricsAsync(MusicId musicId, string rawText)
        {
            return SaveLyricCoreAsync(musicId, LyricSourceType.Embedded, string.Empty, rawText);
        }

        public Task<LyricDocument> SaveOnlineLyricsAsync(MusicId musicId, string rawText)
        {
            return SaveLyricCoreAsync(musicId, LyricSourceType.Online, string.Empty, rawText);
        }
        public async Task<LyricLine> GetCurrentLineAsync(MusicId musicId, TimeSpan position)
        {
            ValidateMusicId(musicId);

            if (position < TimeSpan.Zero)
            {
                position = TimeSpan.Zero;
            }

            var document = await GetPreferredByMusicIdAsync(musicId);

            if (document == null)
            {
                return null;
            }

            EnsureParsedLines(document);

            return _searchService.FindCurrentLine(document.Lines, position);
        }

        public Task DeleteAsync(string lyricId)
        {
            Guard.NotNullOrWhiteSpace(lyricId, nameof(lyricId));

            return _lyricRepository.DeleteAsync(lyricId);
        }

        public Task DeleteByMusicIdAsync(MusicId musicId)
        {
            ValidateMusicId(musicId);

            return _lyricRepository.DeleteByMusicIdAsync(musicId);
        }

        private static void ValidateMusicId(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }
        }

        private void EnsureParsedLines(LyricDocument document)
        {
            if (document == null)
            {
                return;
            }

            if (document.Lines != null && document.Lines.Count > 0)
            {
                return;
            }

            document.Lines = new List<LyricLine>(_parser.Parse(document.RawText));
        }

        private async Task<LyricDocument> SaveLyricCoreAsync(MusicId musicId, LyricSourceType sourceType, string sourcePath, string rawText)
        {
            ValidateMusicId(musicId);

            var normalizedRawText = _parser.NormalizeRawText(rawText);
            var now = DateTimeOffset.Now;

            var document = await FindExistingDocumentAsync(
                musicId,
                sourceType,
                sourcePath);

            if (document == null)
            {
                document = new LyricDocument
                {
                    Id = EntityId.New(),
                    MusicId = musicId,
                    SourceType = sourceType,
                    SourcePath = sourcePath ?? string.Empty,
                    RawText = normalizedRawText,
                    Lines = new List<LyricLine>(),
                    CreatedAt = now,
                    UpdatedAt = now
                };
            }
            else
            {
                document.SourcePath = sourcePath ?? string.Empty;
                document.RawText = normalizedRawText;
                document.UpdatedAt = now;
            }

            document.Lines = new List<LyricLine>(_parser.Parse(normalizedRawText));

            await _lyricRepository.UpsertAsync(document);

            return document;
        }

        private async Task<LyricDocument> FindExistingDocumentAsync(MusicId musicId, LyricSourceType sourceType,string sourcePath)
        {
            var documents = await _lyricRepository.GetAllByMusicIdAsync(musicId);

            if (documents == null)
            {
                return null;
            }

            var normalizedSourcePath = sourcePath ?? string.Empty;

            for (int i = 0; i < documents.Count; i++)
            {
                var document = documents[i];

                if (document == null)
                {
                    continue;
                }

                if (document.SourceType != sourceType)
                {
                    continue;
                }

                if (sourceType == LyricSourceType.ExternalFile)
                {
                    if ((document.SourcePath ?? string.Empty) == normalizedSourcePath)
                    {
                        return document;
                    }

                    continue;
                }

                return document;
            }

            return null;
        }
    }
}

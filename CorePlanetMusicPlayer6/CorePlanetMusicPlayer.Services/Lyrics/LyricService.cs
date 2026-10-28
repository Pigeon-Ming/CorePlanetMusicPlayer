using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using CorePlanetMusicPlayer.Services.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class LyricService : ILyricService
    {
        private readonly ILyricRepository _lyricRepository;
        private readonly IMusicRepository _musicRepository;
        private readonly ILyricParser _parser;
        private readonly LyricSearchService _searchService;
        private readonly IExternalLyricReader _externalLyricReader;
        private readonly IEmbeddedLyricAccessor _embeddedLyricAccessor;
        private readonly ISettingsService _settingsService;

        public LyricService(
            ILyricRepository lyricRepository,
            IMusicRepository musicRepository,
            ILyricParser parser,
            LyricSearchService searchService,
            IExternalLyricReader externalLyricReader,
            IEmbeddedLyricAccessor embeddedLyricAccessor,
            ISettingsService settingsService)
        {
            Guard.NotNull(lyricRepository, nameof(lyricRepository));
            Guard.NotNull(musicRepository, nameof(musicRepository));
            Guard.NotNull(externalLyricReader, nameof(externalLyricReader));
            Guard.NotNull(embeddedLyricAccessor, nameof(embeddedLyricAccessor));
            Guard.NotNull(settingsService, nameof(settingsService));

            _lyricRepository = lyricRepository;
            _musicRepository = musicRepository;

            _parser = parser ?? new LyricParserCollection(
                new ILyricParser[] 
                {
                    new LrcParser()
                });

            _searchService = searchService ?? new LyricSearchService();
            _externalLyricReader = externalLyricReader;
            _embeddedLyricAccessor = embeddedLyricAccessor;
            _settingsService = settingsService;
        }

        public async Task<LyricDocument> GetPreferredByMusicIdAsync(MusicId musicId)
        {
            ValidateMusicId(musicId);
            var documents = await GetAllByMusicIdAsync(musicId);

            return _searchService.SelectPreferred(documents);
        }

        public async Task<IReadOnlyList<LyricDocument>> GetAllByMusicIdAsync(MusicId musicId)
        {
            ValidateMusicId(musicId);

            var documents = await _lyricRepository.GetAllByMusicIdAsync(musicId);
            var result = new List<LyricDocument>();
            LyricDocument userOverride = FindUserOverride(documents);
            Music music = await _musicRepository.GetByIdAsync(musicId);
            LyricDocument externalDocument =
                await TryLoadExternalLyricsAsync(music);
            LyricDocument embeddedDocument =
                await TryLoadEmbeddedLyricsAsync(music);

            if (userOverride != null)
            {
                EnsureParsedLines(userOverride);
                result.Add(userOverride);
            }

            if (externalDocument != null)
            {
                result.Add(externalDocument);
            }

            if (embeddedDocument != null)
            {
                result.Add(embeddedDocument);
            }

            if (documents != null)
            {
                for (int i = 0; i < documents.Count; i++)
                {
                    var document = documents[i];

                    if (document == null ||
                        document.IsUserOverride ||
                        document.SourceType == LyricSourceType.ExternalFile ||
                        document.SourceType == LyricSourceType.Embedded)
                    {
                        continue;
                    }

                    // LyricDataMapper 从数据库恢复对象时不会恢复Lines，因此在服务边界统一重新解析。
                    EnsureParsedLines(document);

                    result.Add(document);
                }
            }

            return _searchService.OrderByPreference(result);
        }

        private Task<LyricDocument> SaveExternalLyricsAsync(MusicId musicId, string sourcePath, string rawText)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentException(
                    "外部歌词来源路径不能为空。",
                    nameof(sourcePath));
            }

            return SaveLyricCoreAsync(
                musicId,
                LyricSourceType.ExternalFile,
                sourcePath,
                rawText,
                false);
        }

        private Task<LyricDocument> SaveEmbeddedLyricsCacheAsync(MusicId musicId, string rawText)
        {
            return SaveLyricCoreAsync(
                musicId,
                LyricSourceType.Embedded,
                string.Empty,
                rawText,
                false);
        }

        public Task<LyricDocument> SaveOnlineLyricsAsync(MusicId musicId, string rawText)
        {
            return SaveLyricCoreAsync(
                musicId,
                LyricSourceType.Online,
                string.Empty,
                rawText,
                false);
        }

        public Task<LyricDocument> SaveUserOverrideAsync(MusicId musicId, LyricSourceType sourceType, string sourcePath, string rawText)
        {
            return SaveLyricCoreAsync(
                musicId,
                sourceType,
                sourcePath,
                rawText,
                true);
        }

        public async Task DeleteUserOverrideAsync(MusicId musicId)
        {
            ValidateMusicId(musicId);

            IReadOnlyList<LyricDocument> documents = await _lyricRepository.GetAllByMusicIdAsync(musicId);

            if (documents == null)
            {
                return;
            }

            for (int i = 0; i < documents.Count; i++)
            {
                LyricDocument document = documents[i];

                if (document != null && document.IsUserOverride)
                {
                    await _lyricRepository.DeleteAsync(document.Id);
                }
            }
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

            /*
             * 此方法适合偶尔查询。
             * 滚动歌词控件不会在计时器中调用它，
             * 而是一次加载文档后在内存中查找当前行。
             */
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

        private async Task<LyricDocument> TryLoadExternalLyricsAsync(Music music)
        {
            try
            {
                if (music == null)
                {
                    return null;
                }

                AppSettings settings = await _settingsService.LoadAsync();

                if (settings == null)
                {
                    settings = _settingsService.GetDefault() ?? AppSettings.CreateDefault();
                }

                settings.Normalize();

                var options = ExternalLyricSearchOptions.CreateDefault();
                options.FileNameRegex = settings.Lyrics.ExternalFileNameRegex;

                ExternalLyricReadResult readResult = await _externalLyricReader.FindAsync(music, options);

                if (readResult == null)
                {
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(readResult.ErrorMessage))
                {
                    Debug.WriteLine(readResult.ErrorMessage);
                    return null;
                }

                if (!readResult.Found)
                {
                    return null;
                }

                return await SaveExternalLyricsAsync(
                    music.Id,
                    readResult.RelativePath,
                    readResult.RawText);
            }
            catch (Exception ex)
            {
                /*
                 * 外部歌词不可用不应阻止内嵌或在线歌词继续参与选择。
                 */
                Debug.WriteLine(ex);
                return null;
            }
        }

        private async Task<LyricDocument> TryLoadEmbeddedLyricsAsync(Music music)
        {
            try
            {
                if (music == null)
                {
                    return null;
                }

                EmbeddedLyricReadResult readResult = await _embeddedLyricAccessor.ReadAsync(music);

                if (readResult == null)
                {
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(readResult.ErrorMessage))
                {
                    Debug.WriteLine(readResult.ErrorMessage);
                    return null;
                }

                if (!readResult.Found)
                {
                    return null;
                }

                return await SaveEmbeddedLyricsCacheAsync(music.Id, readResult.RawText);
            }
            catch (Exception ex)
            {
                /*
                 * 内嵌歌词不可用不应阻止外部或在线歌词继续参与选择。
                 */
                Debug.WriteLine(ex);
                return null;
            }
        }

        private async Task<LyricDocument> SaveLyricCoreAsync(MusicId musicId, LyricSourceType sourceType, string sourcePath, string rawText, bool isUserOverride)
        {
            ValidateMusicId(musicId);
            ValidateSource(sourceType, sourcePath);

            if (string.IsNullOrWhiteSpace(rawText))
            {
                throw new ArgumentException("歌词内容不能为空。", nameof(rawText));
            }

            string normalizedRawText = _parser.NormalizeRawText(rawText);

            if (string.IsNullOrWhiteSpace(normalizedRawText))
            {
                throw new ArgumentException("歌词内容不能为空。", nameof(rawText));
            }

            if (!_parser.CanParse(normalizedRawText))
            {
                throw new FormatException("没有识别到有效的 LRC 时间标签。");
            }

            var parsedLines = _parser.Parse(normalizedRawText);

            if (parsedLines == null || parsedLines.Count == 0)
            {
                throw new FormatException("歌词中没有可用的同步歌词行。");
            }

            string normalizedSourcePath =
                sourceType == LyricSourceType.ExternalFile
                    ? sourcePath ?? string.Empty
                    : string.Empty;

            // 只有完成全部输入检查和解析之后，才查询和修改现有文档。
            var document = await FindExistingDocumentAsync(
                musicId,
                sourceType,
                normalizedSourcePath,
                isUserOverride);

            DateTimeOffset now = DateTimeOffset.Now;

            if (document == null)
            {
                document = new LyricDocument
                {
                    Id = EntityId.New(),
                    MusicId = musicId,
                    SourceType = sourceType,
                    SourcePath = normalizedSourcePath,
                    IsUserOverride = isUserOverride,
                    RawText = normalizedRawText,
                    Lines = new List<LyricLine>(parsedLines),
                    CreatedAt = now,
                    UpdatedAt = now
                };
            }
            else
            {
                document.SourceType = sourceType;

                document.SourcePath = normalizedSourcePath;

                document.IsUserOverride = isUserOverride;

                document.RawText = normalizedRawText;

                document.Lines = new List<LyricLine>(parsedLines);

                document.UpdatedAt = now;
            }

            await _lyricRepository.UpsertAsync(document);

            return document;
        }

        private async Task<LyricDocument> FindExistingDocumentAsync(MusicId musicId, LyricSourceType sourceType, string sourcePath, bool isUserOverride)
        {
            var documents = await _lyricRepository.GetAllByMusicIdAsync(musicId);

            if (documents == null)
            {
                return null;
            }

            string normalizedSourcePath = sourcePath ?? string.Empty;

            for (int i = 0; i < documents.Count; i++)
            {
                var document = documents[i];

                if (document == null)
                {
                    continue;
                }

                if (document.IsUserOverride != isUserOverride)
                {
                    continue;
                }

                // 每首歌曲只保存一份用户覆盖歌词。
                if (isUserOverride)
                {
                    return document;
                }

                if (document.SourceType != sourceType)
                {
                    continue;
                }

                // 内嵌和在线来源目前每首歌曲各保存一份，保存时更新已有记录。
                if (sourceType != LyricSourceType.ExternalFile)
                {
                    return document;
                }

                /*
                 * 外部歌词将来可能存在多份，
                 * 因此需要用来源路径区分。
                 * Windows 路径比较忽略大小写。
                 */
                if (string.Equals(document.SourcePath ?? string.Empty, normalizedSourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    return document;
                }
            }

            return null;
        }

        private static LyricDocument FindUserOverride(IReadOnlyList<LyricDocument> documents)
        {
            if (documents == null)
            {
                return null;
            }

            for (int i = 0; i < documents.Count; i++)
            {
                LyricDocument document = documents[i];

                if (document != null && document.IsUserOverride)
                {
                    return document;
                }
            }

            return null;
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

            if (string.IsNullOrWhiteSpace(document.RawText))
            {
                document.Lines = new List<LyricLine>();
                return;
            }

            var parsedLines = _parser.Parse(document.RawText);

            document.Lines = parsedLines == null ? new List<LyricLine>() : new List<LyricLine>(parsedLines);
        }

        private static void ValidateMusicId(MusicId musicId)
        {
            if (musicId.IsEmpty)
            {
                throw new ArgumentException("Music id cannot be empty.", nameof(musicId));
            }
        }

        private static void ValidateSource(LyricSourceType sourceType, string sourcePath)
        {
            if (sourceType != LyricSourceType.Embedded &&
                sourceType != LyricSourceType.ExternalFile &&
                sourceType != LyricSourceType.Online)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceType), "歌词来源类型无效。");
            }

            if (sourceType == LyricSourceType.ExternalFile &&
                string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentException(
                    "外部歌词来源路径不能为空。",
                    nameof(sourcePath));
            }
        }
    }
}

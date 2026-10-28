using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Lyrics
{
    public sealed class LyricEditService : ILyricEditService
    {
        private readonly IMusicRepository _musicRepository;
        private readonly ILyricService _lyricService;
        private readonly ILyricParser _parser;
        private readonly IExternalLyricWriter _externalLyricWriter;
        private readonly IEmbeddedLyricAccessor _embeddedLyricAccessor;

        public LyricEditService(
            IMusicRepository musicRepository,
            ILyricService lyricService,
            ILyricParser parser,
            IExternalLyricWriter externalLyricWriter,
            IEmbeddedLyricAccessor embeddedLyricAccessor)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));
            Guard.NotNull(lyricService, nameof(lyricService));
            Guard.NotNull(externalLyricWriter, nameof(externalLyricWriter));
            Guard.NotNull(embeddedLyricAccessor, nameof(embeddedLyricAccessor));

            _musicRepository = musicRepository;
            _lyricService = lyricService;
            _parser = parser ?? new LyricParserCollection(
                new ILyricParser[]
                {
                    new LrcParser()
                });
            _externalLyricWriter = externalLyricWriter;
            _embeddedLyricAccessor = embeddedLyricAccessor;
        }

        public async Task<LyricSaveResult> SaveAsync(
            MusicId musicId,
            string rawText,
            LyricSaveTarget target,
            LyricDocument sourceDocument)
        {
            if (musicId.IsEmpty)
            {
                return LyricSaveResult.Failure(target, "音乐 ID 不能为空。", false);
            }

            string normalizedRawText;

            try
            {
                normalizedRawText = ValidateAndNormalize(rawText);
            }
            catch (Exception ex)
            {
                return LyricSaveResult.Failure(target, ex.Message, false);
            }

            Music music = await _musicRepository.GetByIdAsync(musicId);

            if (music == null)
            {
                return LyricSaveResult.Failure(target, "当前歌曲已不在音乐库中。", false);
            }

            if (sourceDocument != null && sourceDocument.MusicId != musicId)
            {
                return LyricSaveResult.Failure(target, "编辑中的歌词不属于当前歌曲。", false);
            }

            switch (target)
            {
                case LyricSaveTarget.ExternalFile:
                    return await SaveExternalAsync(music, sourceDocument, normalizedRawText);

                case LyricSaveTarget.Embedded:
                    return await SaveEmbeddedAsync(music, normalizedRawText);

                case LyricSaveTarget.DatabaseOverride:
                    return await SaveDatabaseOverrideAsync(musicId, sourceDocument, normalizedRawText);

                default:
                    return LyricSaveResult.Failure(target, "歌词保存目标无效。", false);
            }
        }

        private async Task<LyricSaveResult> SaveExternalAsync(Music music, LyricDocument currentDocument, string rawText)
        {
            if (currentDocument == null || 
                currentDocument.SourceType != LyricSourceType.ExternalFile ||
                string.IsNullOrWhiteSpace(currentDocument.SourcePath))
            {
                return LyricSaveResult.Failure(LyricSaveTarget.ExternalFile, "当前歌词没有可写回的外部歌词文件。", true);
            }

            ExternalLyricWriteResult result = await _externalLyricWriter.WriteAsync(music, currentDocument.SourcePath, rawText);

            if (result == null || !result.Succeeded)
            {
                return LyricSaveResult.Failure(LyricSaveTarget.ExternalFile, 
                    result == null
                        ? "外部歌词写入没有返回结果。"
                        : result.ErrorMessage,
                    true);
            }

            string warning = await DeleteUserOverrideAsync(music.Id);

            return LyricSaveResult.Success(
                LyricSaveTarget.ExternalFile,
                string.IsNullOrWhiteSpace(warning)
                    ? "歌词已写回外部歌词文件。"
                    : warning);
        }

        private async Task<LyricSaveResult> SaveEmbeddedAsync(Music music, string rawText)
        {
            EmbeddedLyricWriteResult result = await _embeddedLyricAccessor.WriteAsync(music, rawText);

            if (result == null || !result.Succeeded)
            {
                return LyricSaveResult.Failure(
                    LyricSaveTarget.Embedded,
                    result == null
                        ? "内嵌歌词写入没有返回结果。"
                        : result.ErrorMessage,
                    true);
            }

            string warning = await DeleteUserOverrideAsync(music.Id);

            return LyricSaveResult.Success(
                LyricSaveTarget.Embedded,
                string.IsNullOrWhiteSpace(warning)
                    ? "歌词已写入音频文件。"
                    : warning);
        }

        private async Task<LyricSaveResult> SaveDatabaseOverrideAsync(MusicId musicId, LyricDocument currentDocument, string rawText)
        {
            LyricSourceType sourceType = GetOverrideSourceType(currentDocument);
            string sourcePath = sourceType == LyricSourceType.ExternalFile
                ? currentDocument.SourcePath
                : string.Empty;

            await _lyricService.SaveUserOverrideAsync(musicId, sourceType, sourcePath, rawText);

            return LyricSaveResult.Success(LyricSaveTarget.DatabaseOverride, "歌词已保存为应用缓存。");
        }

        private async Task<string> DeleteUserOverrideAsync(MusicId musicId)
        {
            try
            {
                await _lyricService.DeleteUserOverrideAsync(musicId);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return "歌词文件已保存，但清理旧应用缓存失败：" + ex.Message;
            }
        }

        private string ValidateAndNormalize(string rawText)
        {
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

            IReadOnlyList<LyricLine> lines = _parser.Parse(normalizedRawText);

            if (lines == null || lines.Count == 0)
            {
                throw new FormatException("歌词中没有可用的同步歌词行。");
            }

            return normalizedRawText;
        }

        private static LyricSourceType GetOverrideSourceType(LyricDocument currentDocument)
        {
            if (currentDocument == null)
            {
                return LyricSourceType.Embedded;
            }

            switch (currentDocument.SourceType)
            {
                case LyricSourceType.ExternalFile:
                case LyricSourceType.Embedded:
                case LyricSourceType.Online:
                    return currentDocument.SourceType;
                default:
                    return LyricSourceType.Embedded;
            }
        }
    }
}

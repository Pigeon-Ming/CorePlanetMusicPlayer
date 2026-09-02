using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Services.Metadata
{
    public sealed class MusicMetadataEditService : IMusicMetadataEditService
    {
        private readonly IMusicRepository _musicRepository;
        private readonly IMusicMetadataWriter _metadataWriter;

        public MusicMetadataEditService( IMusicRepository musicRepository, IMusicMetadataWriter metadataWriter)
        {
            Guard.NotNull(musicRepository, nameof(musicRepository));

            _musicRepository = musicRepository;
            _metadataWriter = metadataWriter;
        }

        public async Task<Result<Music>> UpdateAsync(MusicMetadataUpdateRequest request)
        {
            var validationResult = ValidateRequest(request);

            if (validationResult.IsFailure)
            {
                return Result<Music>.Failure(validationResult.ErrorMessage);
            }

            var music = await _musicRepository.GetByIdAsync(request.MusicId);

            if (music == null)
            {
                return Result<Music>.Failure("音乐不存在。");
            }

            if (RequiresFileWrite(music))
            {
                if (_metadataWriter == null)
                {
                    return Result<Music>.Failure("当前平台不支持写入本地音频文件元数据。");
                }

                var writeResult = await _metadataWriter.WriteAsync(
                    music,
                    request);

                if (writeResult == null)
                {
                    return Result<Music>.Failure("写入音频文件元数据失败。");
                }

                if (writeResult.IsFailure)
                {
                    return Result<Music>.Failure(writeResult.ErrorMessage);
                }
            }

            ApplyUpdate(music, request);

            await _musicRepository.UpsertAsync(music);

            return Result<Music>.Success(music);
        }

        private static Result ValidateRequest(MusicMetadataUpdateRequest request)
        {
            if (request == null)
            {
                return Result.Failure("元数据修改请求不能为空。");
            }

            if (request.MusicId.IsEmpty)
            {
                return Result.Failure("音乐 ID 不能为空。");
            }

            if (!request.HasAnyChange)
            {
                return Result.Failure("没有需要修改的元数据。");
            }

            if (request.HasYear && request.Year.HasValue && request.Year.Value < 0)
            {
                return Result.Failure("年份不能小于 0。");
            }

            if (request.HasTrackNumber &&
                request.TrackNumber.HasValue &&
                request.TrackNumber.Value < 0)
            {
                return Result.Failure("曲目号不能小于 0。");
            }

            if (request.HasDiscNumber &&
                request.DiscNumber.HasValue &&
                request.DiscNumber.Value < 0)
            {
                return Result.Failure("碟片号不能小于 0。");
            }

            return Result.Success();
        }

        private static bool RequiresFileWrite(Music music)
        {
            if (music == null)
            {
                return false;
            }

            return music.SourceType == MusicSourceType.Local || music.SourceType == MusicSourceType.Temporary;
        }

        private static void ApplyUpdate(Music music, MusicMetadataUpdateRequest request)
        {
            if (music.Metadata == null)
            {
                music.Metadata = MusicMetadata.Empty;
            }

            if (request.HasTitle)
            {
                var title = NormalizeText(request.Title);

                music.Title = title;
                music.Metadata.Title = title;
            }

            if (request.HasArtistName)
            {
                var artistName = NormalizeText(request.ArtistName);

                music.ArtistName = artistName;
                music.Metadata.ArtistName = artistName;
            }

            if (request.HasAlbumTitle)
            {
                var albumTitle = NormalizeText(request.AlbumTitle);

                music.AlbumTitle = albumTitle;
                music.Metadata.AlbumTitle = albumTitle;
            }

            if (request.HasAlbumArtistName)
            {
                music.Metadata.AlbumArtistName =
                    NormalizeText(request.AlbumArtistName);
            }

            if (request.HasGenre)
            {
                music.Metadata.Genre = NormalizeText(request.Genre);
            }

            if (request.HasYear)
            {
                music.Metadata.Year = request.Year;
            }

            if (request.HasTrackNumber)
            {
                music.Metadata.TrackNumber = request.TrackNumber;
            }

            if (request.HasDiscNumber)
            {
                music.Metadata.DiscNumber = request.DiscNumber;
            }

            if (request.HasComposer)
            {
                music.Metadata.Composer = NormalizeText(request.Composer);
            }

            if (request.HasComment)
            {
                music.Metadata.Comment = request.Comment ?? string.Empty;
            }
        }

        private static string NormalizeText(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.Trim();
        }
    }
}

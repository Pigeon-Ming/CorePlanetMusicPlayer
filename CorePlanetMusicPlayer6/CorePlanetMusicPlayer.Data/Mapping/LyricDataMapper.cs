using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Mapping
{
    public static class LyricDataMapper
    {
        public static LyricDocument ToModel(LyricEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new LyricDocument
            {
                Id = entity.Id ?? string.Empty,
                MusicId = new MusicId(entity.MusicId),
                SourceType = (LyricSourceType)entity.SourceType,
                SourcePath = entity.SourcePath ?? string.Empty,
                RawText = entity.RawText ?? string.Empty,
                Lines = new List<LyricLine>(),
                CreatedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.CreatedAtUnixTimeMilliseconds),
                UpdatedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.UpdatedAtUnixTimeMilliseconds)
            };
        }

        public static LyricEntity ToEntity(LyricDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new LyricEntity
            {
                Id = document.Id ?? string.Empty,
                MusicId = document.MusicId.ToString(),
                SourceType = (int)document.SourceType,
                SourcePath = document.SourcePath ?? string.Empty,
                RawText = document.RawText ?? string.Empty,
                LinesText = string.Empty,
                CreatedAtUnixTimeMilliseconds = document.CreatedAt.ToUnixTimeMilliseconds(),
                UpdatedAtUnixTimeMilliseconds = document.UpdatedAt.ToUnixTimeMilliseconds()
            };
        }
    }
}

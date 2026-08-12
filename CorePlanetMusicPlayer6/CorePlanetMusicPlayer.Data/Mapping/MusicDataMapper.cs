using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Mapping
{
    public static class MusicDataMapper
    {
        public static Music ToModel(MusicEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            var metadata = new MusicMetadata
            {
                Title = entity.Title ?? string.Empty,
                ArtistName = entity.ArtistName ?? string.Empty,
                AlbumTitle = entity.AlbumTitle ?? string.Empty,
                AlbumArtistName = entity.AlbumArtistName ?? string.Empty,
                Genre = entity.Genre ?? string.Empty,
                Year = entity.Year,
                TrackNumber = entity.TrackNumber,
                DiscNumber = entity.DiscNumber,
                Composer = entity.Composer ?? string.Empty,
                Comment = entity.Comment ?? string.Empty
            };

            var fileInfo = new MusicFileInfo
            {
                Path = entity.FilePath ?? string.Empty,
                RelativePath = entity.RelativePath ?? string.Empty,
                FileName = entity.FileName ?? string.Empty,
                Extension = entity.Extension ?? string.Empty,
                Size = entity.Size,
                LastModifiedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.LastModifiedAtUnixTimeMilliseconds),
                LibraryFolderId = entity.LibraryFolderId ?? string.Empty
            };

            return new Music
            {
                Id = new MusicId(entity.Id),
                Title = entity.Title ?? string.Empty,
                AlbumTitle = entity.AlbumTitle ?? string.Empty,
                ArtistName = entity.ArtistName ?? string.Empty,
                Duration = new TimeSpan(entity.DurationTicks),
                SourceType = (MusicSourceType)entity.SourceType,
                Metadata = metadata,
                FileInfo = fileInfo.HasPath ? fileInfo : null,
                AddedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.AddedAtUnixTimeMilliseconds),
                LastPlayedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.LastPlayedAtUnixTimeMilliseconds)
            };
        }

        public static MusicEntity ToEntity(Music music)
        {
            if (music == null)
            {
                return null;
            }

            var metadata = music.Metadata ?? MusicMetadata.Empty;
            var fileInfo = music.FileInfo;

            return new MusicEntity
            {
                Id = music.Id.ToString(),
                Title = music.Title ?? string.Empty,
                AlbumTitle = music.AlbumTitle ?? string.Empty,
                ArtistName = music.ArtistName ?? string.Empty,

                AlbumArtistName = metadata.AlbumArtistName ?? string.Empty,
                Genre = metadata.Genre ?? string.Empty,
                Year = metadata.Year,
                TrackNumber = metadata.TrackNumber,
                DiscNumber = metadata.DiscNumber,
                Composer = metadata.Composer ?? string.Empty,
                Comment = metadata.Comment ?? string.Empty,

                DurationTicks = music.Duration.Ticks,
                SourceType = (int)music.SourceType,

                FilePath = fileInfo == null ? string.Empty : fileInfo.Path ?? string.Empty,
                RelativePath = fileInfo == null ? string.Empty : fileInfo.RelativePath ?? string.Empty,
                FileName = fileInfo == null ? string.Empty : fileInfo.FileName ?? string.Empty,
                Extension = fileInfo == null ? string.Empty : fileInfo.Extension ?? string.Empty,
                Size = fileInfo == null ? null : fileInfo.Size,
                LastModifiedAtUnixTimeMilliseconds = fileInfo == null
                    ? null
                    : DataValueConverter.ToUnixTimeMilliseconds(fileInfo.LastModifiedAt),
                LibraryFolderId = fileInfo == null ? string.Empty : fileInfo.LibraryFolderId ?? string.Empty,

                AddedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(music.AddedAt),
                LastPlayedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(music.LastPlayedAt)
            };
        }
    }
}

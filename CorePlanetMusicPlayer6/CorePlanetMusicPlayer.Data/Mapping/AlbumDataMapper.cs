using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Mapping
{
    public static class AlbumDataMapper
    {
        private const char IdSeparator = '|';

        public static Album ToModel(AlbumEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new Album
            {
                Id = new AlbumId(entity.Id),
                Title = entity.Title ?? string.Empty,
                ArtistName = entity.ArtistName ?? string.Empty,
                AlbumArtistName = entity.AlbumArtistName ?? string.Empty,
                Genre = entity.Genre ?? string.Empty,
                Year = entity.Year,
                MusicIds = ParseMusicIds(entity.MusicIdsText),
                TotalDuration = new TimeSpan(entity.TotalDurationTicks),
                AddedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.AddedAtUnixTimeMilliseconds),
                UpdatedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.UpdatedAtUnixTimeMilliseconds)
            };
        }

        public static AlbumEntity ToEntity(Album album)
        {
            if (album == null)
            {
                return null;
            }

            return new AlbumEntity
            {
                Id = album.Id.ToString(),
                Title = album.Title ?? string.Empty,
                ArtistName = album.ArtistName ?? string.Empty,
                AlbumArtistName = album.AlbumArtistName ?? string.Empty,
                Genre = album.Genre ?? string.Empty,
                Year = album.Year,
                MusicIdsText = FormatMusicIds(album.MusicIds),
                TotalDurationTicks = album.TotalDuration.Ticks,
                AddedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(album.AddedAt),
                UpdatedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(album.UpdatedAt)
            };
        }

        private static List<MusicId> ParseMusicIds(string text)
        {
            var result = new List<MusicId>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return result;
            }

            var parts = text.Split(new[] { IdSeparator }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var id = new MusicId(part);

                if (!id.IsEmpty)
                {
                    result.Add(id);
                }
            }

            return result;
        }

        private static string FormatMusicIds(IEnumerable<MusicId> ids)
        {
            if (ids == null)
            {
                return string.Empty;
            }

            var values = new List<string>();

            foreach (var id in ids)
            {
                if (!id.IsEmpty)
                {
                    values.Add(id.ToString());
                }
            }

            return string.Join(IdSeparator.ToString(), values);
        }
    }
}

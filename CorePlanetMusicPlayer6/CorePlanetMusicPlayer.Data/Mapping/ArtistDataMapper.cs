using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Core.Artists;
using CorePlanetMusicPlayer.Core.Music;
using CorePlanetMusicPlayer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Mapping
{
    public static class ArtistDataMapper
    {
        private const char IdSeparator = '|';

        public static Artist ToModel(ArtistEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new Artist
            {
                Id = new ArtistId(entity.Id),
                Name = entity.Name ?? string.Empty,
                SortName = entity.SortName ?? string.Empty,
                MusicIds = ParseMusicIds(entity.MusicIdsText),
                AlbumIds = ParseAlbumIds(entity.AlbumIdsText),
                TotalDuration = new TimeSpan(entity.TotalDurationTicks),
                AddedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.AddedAtUnixTimeMilliseconds),
                UpdatedAt = DataValueConverter.FromUnixTimeMilliseconds(entity.UpdatedAtUnixTimeMilliseconds)
            };
        }

        public static ArtistEntity ToEntity(Artist artist)
        {
            if (artist == null)
            {
                return null;
            }

            return new ArtistEntity
            {
                Id = artist.Id.ToString(),
                Name = artist.Name ?? string.Empty,
                SortName = artist.SortName ?? string.Empty,
                MusicIdsText = FormatMusicIds(artist.MusicIds),
                AlbumIdsText = FormatAlbumIds(artist.AlbumIds),
                TotalDurationTicks = artist.TotalDuration.Ticks,
                AddedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(artist.AddedAt),
                UpdatedAtUnixTimeMilliseconds = DataValueConverter.ToUnixTimeMilliseconds(artist.UpdatedAt)
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

        private static List<AlbumId> ParseAlbumIds(string text)
        {
            var result = new List<AlbumId>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return result;
            }

            var parts = text.Split(new[] { IdSeparator }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var id = new AlbumId(part);

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

        private static string FormatAlbumIds(IEnumerable<AlbumId> ids)
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

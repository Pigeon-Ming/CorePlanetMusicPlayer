using CorePlanetMusicPlayer.Core.Albums;
using CorePlanetMusicPlayer.Data.Database;
using CorePlanetMusicPlayer.Data.Entities;
using CorePlanetMusicPlayer.Data.Mapping;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Repositories.Sqlite
{
    public sealed class SqliteAlbumRepository : IAlbumRepository
    {
        private readonly LibraryDatabase _database;

        public SqliteAlbumRepository(LibraryDatabase database)
        {
            _database = database;
        }

        public Task<IReadOnlyList<Album>> GetAllAsync()
        {
            var result = new List<Album>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from albums order by title;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(AlbumDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<Album>)result);
        }

        public Task<Album> GetByIdAsync(AlbumId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from albums where id = $id limit 1;";
                command.Parameters.AddWithValue("$id", id.ToString());

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Task.FromResult(AlbumDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult<Album>(null);
        }

        public Task<IReadOnlyList<Album>> SearchAsync(string keyword)
        {
            var result = new List<Album>();
            var safeKeyword = keyword ?? string.Empty;

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    select * from albums
                    where title like $keyword
                       or artist_name like $keyword
                       or album_artist_name like $keyword
                    ORDER BY title;";
                command.Parameters.AddWithValue("$keyword", $"%{safeKeyword}%");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(AlbumDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<Album>)result);
        }

        public Task UpsertAsync(Album album)
        {
            var entity = AlbumDataMapper.ToEntity(album);

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                ApplyUpsertCommand(command, entity);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task UpsertRangeAsync(IEnumerable<Album> albums)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                if (albums != null)
                {
                    foreach (var album in albums)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            ApplyUpsertCommand(command, AlbumDataMapper.ToEntity(album));
                            command.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(AlbumId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from albums where id = $id;";
                command.Parameters.AddWithValue("$id", id.ToString());
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM albums;";
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        private static void ApplyUpsertCommand(SqliteCommand command, AlbumEntity entity)
        {
            command.CommandText = @"
                insert or replace into albums (
                    id,
                    title,
                    artist_name,
                    album_artist_name,
                    genre,
                    year,
                    music_ids_text,
                    total_duration_ticks,
                    added_at,
                    updated_at
                ) values (
                    $id,
                    $title,
                    $artistName,
                    $albumArtistName,
                    $genre,
                    $year,
                    $musicIdsText,
                    $totalDurationTicks,
                    $addedAt,
                    $updatedAt
                );";

            command.Parameters.AddWithValue("$id", entity.Id);
            command.Parameters.AddWithValue("$title", entity.Title);
            command.Parameters.AddWithValue("$artistName", entity.ArtistName);
            command.Parameters.AddWithValue("$albumArtistName", entity.AlbumArtistName);
            command.Parameters.AddWithValue("$genre", entity.Genre);
            command.Parameters.AddWithValue("$year", (object)entity.Year ?? System.DBNull.Value);
            command.Parameters.AddWithValue("$musicIdsText", entity.MusicIdsText);
            command.Parameters.AddWithValue("$totalDurationTicks", entity.TotalDurationTicks);
            command.Parameters.AddWithValue("$addedAt", (object)entity.AddedAtUnixTimeMilliseconds ?? System.DBNull.Value);
            command.Parameters.AddWithValue("$updatedAt", (object)entity.UpdatedAtUnixTimeMilliseconds ?? System.DBNull.Value);
        }

        private static AlbumEntity ReadEntity(SqliteDataReader reader)
        {
            return new AlbumEntity
            {
                Id = reader.GetStringOrEmpty("id"),
                Title = reader.GetStringOrEmpty("title"),
                ArtistName = reader.GetStringOrEmpty("artist_name"),
                AlbumArtistName = reader.GetStringOrEmpty("album_artist_name"),
                Genre = reader.GetStringOrEmpty("genre"),
                Year = reader.GetNullableInt32("year"),
                MusicIdsText = reader.GetStringOrEmpty("music_ids_text"),
                TotalDurationTicks = reader.GetInt64OrDefault("total_duration_ticks"),
                AddedAtUnixTimeMilliseconds = reader.GetNullableInt64("added_at"),
                UpdatedAtUnixTimeMilliseconds = reader.GetNullableInt64("updated_at")
            };
        }
    }
}

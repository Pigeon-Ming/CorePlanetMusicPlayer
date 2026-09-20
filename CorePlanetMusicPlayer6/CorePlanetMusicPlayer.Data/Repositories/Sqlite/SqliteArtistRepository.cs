using CorePlanetMusicPlayer.Core.Artists;
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
    public sealed class SqliteArtistRepository : IArtistRepository
    {
        private readonly LibraryDatabase _database;

        public SqliteArtistRepository(LibraryDatabase database)
        {
            _database = database;
        }

        public Task<IReadOnlyList<Artist>> GetAllAsync()
        {
            var result = new List<Artist>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from artists order by name;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(ArtistDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<Artist>)result);
        }

        public Task<Artist> GetByIdAsync(ArtistId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from artists where id = $id limit 1;";
                command.Parameters.AddWithValue("$id", id.ToString());

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Task.FromResult(ArtistDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult<Artist>(null);
        }

        public Task<IReadOnlyList<Artist>> SearchAsync(string keyword)
        {
            var result = new List<Artist>();
            var safeKeyword = keyword ?? string.Empty;

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    select * from artists
                    where name like $keyword
                       OR sort_name like $keyword
                    order by name;";
                command.Parameters.AddWithValue("$keyword", $"%{safeKeyword}%");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(ArtistDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<Artist>)result);
        }

        public Task UpsertAsync(Artist artist)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                ApplyUpsertCommand(command, ArtistDataMapper.ToEntity(artist));
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task UpsertRangeAsync(IEnumerable<Artist> artists)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                if (artists != null)
                {
                    foreach (var artist in artists)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            ApplyUpsertCommand(command, ArtistDataMapper.ToEntity(artist));
                            command.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }

            return Task.CompletedTask;
        }

        public Task<bool> UpdateDescriptionAsync(ArtistId artistId, string description, DateTimeOffset updatedAt)
        {
            if (artistId.IsEmpty)
            {
                throw new ArgumentException("艺术家 ID 不能为空。", nameof(artistId));
            }

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE artists
                    SET description = $description,
                    updated_at = $updatedAt
                    WHERE id = $id;
                ";

                command.Parameters.AddWithValue("$description", description ?? string.Empty);

                command.Parameters.AddWithValue("$updatedAt", updatedAt.ToUnixTimeMilliseconds());

                command.Parameters.AddWithValue("$id", artistId.ToString());

                return Task.FromResult(command.ExecuteNonQuery() > 0);
            }
        }

        public Task DeleteIfEmptyAsync(ArtistId artistId)
        {
            if (artistId.IsEmpty)
            {
                throw new ArgumentException("艺术家 ID 不能为空。", nameof(artistId));
            }

            using (var connection = _database.CreateOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                Artist artist;

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    command.CommandText = @"
                        SELECT *
                        FROM artists
                        WHERE id = $id
                        LIMIT 1;
                    ";

                    command.Parameters.AddWithValue("$id", artistId.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            throw new InvalidOperationException("该艺术家已不存在，请刷新艺术家列表。");
                        }

                        artist = ArtistDataMapper.ToModel(ReadEntity(reader));
                    }
                }

                if (artist.MusicCount > 0)
                {
                    throw new InvalidOperationException("该艺术家仍有关联歌曲，不能删除。");
                }

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    command.CommandText = @"
                        DELETE FROM artists
                        WHERE id = $id;
                    ";

                    command.Parameters.AddWithValue("$id", artistId.ToString());

                    if (command.ExecuteNonQuery() != 1)
                    {
                        throw new InvalidOperationException("艺术家删除失败，请刷新列表后重试。");
                    }
                }

                transaction.Commit();
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from artists;";
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        private static void ApplyUpsertCommand(SqliteCommand command, ArtistEntity entity)
        {
            command.CommandText = @"
                insert or replace into artists (
                    id,
                    name,
                    sort_name,
                    description,
                    music_ids_text,
                    album_ids_text,
                    total_duration_ticks,
                    added_at,
                    updated_at
                ) values (
                    $id,
                    $name,
                    $sortName,
                    $description,
                    $musicIdsText,
                    $albumIdsText,
                    $totalDurationTicks,
                    $addedAt,
                    $updatedAt
                );";

            command.Parameters.AddWithValue("$id", entity.Id);
            command.Parameters.AddWithValue("$name", entity.Name);
            command.Parameters.AddWithValue("$sortName", entity.SortName);
            command.Parameters.AddWithValue("$description", entity.Description);
            command.Parameters.AddWithValue("$musicIdsText", entity.MusicIdsText);
            command.Parameters.AddWithValue("$albumIdsText", entity.AlbumIdsText);
            command.Parameters.AddWithValue("$totalDurationTicks", entity.TotalDurationTicks);
            command.Parameters.AddWithValue("$addedAt", (object)entity.AddedAtUnixTimeMilliseconds ?? System.DBNull.Value);
            command.Parameters.AddWithValue("$updatedAt", (object)entity.UpdatedAtUnixTimeMilliseconds ?? System.DBNull.Value);
        }

        private static ArtistEntity ReadEntity(SqliteDataReader reader)
        {
            return new ArtistEntity
            {
                Id = reader.GetStringOrEmpty("id"),
                Name = reader.GetStringOrEmpty("name"),
                SortName = reader.GetStringOrEmpty("sort_name"),
                Description = reader.GetStringOrEmpty("description"),
                MusicIdsText = reader.GetStringOrEmpty("music_ids_text"),
                AlbumIdsText = reader.GetStringOrEmpty("album_ids_text"),
                TotalDurationTicks = reader.GetInt64OrDefault("total_duration_ticks"),
                AddedAtUnixTimeMilliseconds = reader.GetNullableInt64("added_at"),
                UpdatedAtUnixTimeMilliseconds = reader.GetNullableInt64("updated_at")
            };
        }
    }
}

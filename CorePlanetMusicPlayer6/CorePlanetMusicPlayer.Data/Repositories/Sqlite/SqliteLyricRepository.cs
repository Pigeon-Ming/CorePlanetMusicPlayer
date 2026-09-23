using CorePlanetMusicPlayer.Core.Lyrics;
using CorePlanetMusicPlayer.Core.Music;
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
    public sealed class SqliteLyricRepository : ILyricRepository
    {
        private readonly LibraryDatabase _database;

        public SqliteLyricRepository(LibraryDatabase database)
        {
            _database = database;
        }

        public Task<LyricDocument> GetByIdAsync(string id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from lyrics where id = $id limit 1;";
                command.Parameters.AddWithValue("$id", id ?? string.Empty);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Task.FromResult(LyricDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult<LyricDocument>(null);
        }

        public Task<LyricDocument> GetByMusicIdAsync(MusicId musicId)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from lyrics where music_id = $musicId order by updated_at desc limit 1;";
                command.Parameters.AddWithValue("$musicId", musicId.ToString());

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Task.FromResult(LyricDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult<LyricDocument>(null);
        }

        public Task<IReadOnlyList<LyricDocument>> GetAllByMusicIdAsync(MusicId musicId)
        {
            var result = new List<LyricDocument>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from lyrics where music_id = $musicId order by updated_at desc;";
                command.Parameters.AddWithValue("$musicId", musicId.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(LyricDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<LyricDocument>)result);
        }

        public Task UpsertAsync(LyricDocument document)
        {
            var entity = LyricDataMapper.ToEntity(document);

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    insert or replace into lyrics (
                        id,
                        music_id,
                        source_type,
                        source_path,
                        raw_text,
                        lines_text,
                        created_at,
                        updated_at
                    ) values (
                        $id,
                        $musicId,
                        $sourceType,
                        $sourcePath,
                        $rawText,
                        $linesText,
                        $createdAt,
                        $updatedAt
                    );";

                command.Parameters.AddWithValue("$id", entity.Id);
                command.Parameters.AddWithValue("$musicId", entity.MusicId);
                command.Parameters.AddWithValue("$sourceType", entity.SourceType);
                command.Parameters.AddWithValue("$sourcePath", entity.SourcePath);
                command.Parameters.AddWithValue("$rawText", entity.RawText);
                command.Parameters.AddWithValue("$linesText", entity.LinesText);
                command.Parameters.AddWithValue("$createdAt", entity.CreatedAtUnixTimeMilliseconds);
                command.Parameters.AddWithValue("$updatedAt", entity.UpdatedAtUnixTimeMilliseconds);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from lyrics where id = $id;";
                command.Parameters.AddWithValue("$id", id ?? string.Empty);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task DeleteByMusicIdAsync(MusicId musicId)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from lyrics where music_id = $musicId;";
                command.Parameters.AddWithValue("$musicId", musicId.ToString());
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from lyrics;";
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        private static LyricEntity ReadEntity(SqliteDataReader reader)
        {
            return new LyricEntity
            {
                Id = reader.GetStringOrEmpty("id"),
                MusicId = reader.GetStringOrEmpty("music_id"),
                SourceType = reader.GetInt32OrDefault("source_type"),
                SourcePath = reader.GetStringOrEmpty("source_path"),
                RawText = reader.GetStringOrEmpty("raw_text"),
                LinesText = reader.GetStringOrEmpty("lines_text"),
                CreatedAtUnixTimeMilliseconds = reader.GetInt64OrDefault("created_at"),
                UpdatedAtUnixTimeMilliseconds = reader.GetInt64OrDefault("updated_at")
            };
        }
    }
}

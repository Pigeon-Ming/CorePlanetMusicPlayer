using CorePlanetMusicPlayer.Core.Library;
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
    public sealed class SqliteLibraryFolderRepository : ILibraryFolderRepository
    {
        private readonly LibraryDatabase _database;

        public SqliteLibraryFolderRepository(LibraryDatabase database)
        {
            _database = database;
        }

        public Task<IReadOnlyList<LibraryFolder>> GetAllAsync()
        {
            var result = new List<LibraryFolder>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from library_folders order by added_at;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(LibraryFolderDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<LibraryFolder>)result);
        }

        public Task<LibraryFolder> GetByIdAsync(LibraryFolderId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from library_folders where id = $id limit 1;";
                command.Parameters.AddWithValue("$id", id.ToString());

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Task.FromResult(LibraryFolderDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult<LibraryFolder>(null);
        }

        public Task UpsertAsync(LibraryFolder folder)
        {
            var entity = LibraryFolderDataMapper.ToEntity(folder);

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    insert or replace_into library_folders (
                        id,
                        display_name,
                        path,
                        access_key,
                        access_kind,
                        added_at,
                        updated_at
                    ) values (
                        $id,
                        $displayName,
                        $path,
                        $accessKey,
                        $accessKind,
                        $addedAt,
                        $updatedAt
                    );";

                command.Parameters.AddWithValue("$id", entity.Id);
                command.Parameters.AddWithValue("$displayName", entity.DisplayName);
                command.Parameters.AddWithValue("$path", entity.Path);
                command.Parameters.AddWithValue("$accessKey", entity.AccessKey);
                command.Parameters.AddWithValue("$accessKind", entity.AccessKind);
                command.Parameters.AddWithValue("$addedAt", entity.AddedAtUnixTimeMilliseconds);
                command.Parameters.AddWithValue("$updatedAt", entity.UpdatedAtUnixTimeMilliseconds);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(LibraryFolderId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from library_folders where id = $id;";
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
                command.CommandText = "delete from library_folders;";
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        private static LibraryFolderEntity ReadEntity(SqliteDataReader reader)
        {
            return new LibraryFolderEntity
            {
                Id = reader.GetStringOrEmpty("id"),
                DisplayName = reader.GetStringOrEmpty("display_name"),
                Path = reader.GetStringOrEmpty("path"),
                AccessKey = reader.GetStringOrEmpty("access_key"),
                AccessKind = reader.GetInt32OrDefault("access_kind"),
                AddedAtUnixTimeMilliseconds = reader.GetInt64OrDefault("added_at"),
                UpdatedAtUnixTimeMilliseconds = reader.GetInt64OrDefault("updated_at")
            };
        }
    }
}

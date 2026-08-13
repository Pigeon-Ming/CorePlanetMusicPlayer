using CorePlanetMusicPlayer.Core.Playlists;
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
    public sealed class SqlitePlaylistRepository : IPlaylistRepository
    {
        private readonly LibraryDatabase _database;

        public SqlitePlaylistRepository(LibraryDatabase database)
        {
            _database = database;
        }

        public Task<IReadOnlyList<Playlist>> GetAllAsync()
        {
            var result = new List<Playlist>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from playlists order by created_at;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var playlistEntity = ReadPlaylistEntity(reader);
                        var itemEntities = GetItemEntities(connection, playlistEntity.Id);
                        result.Add(PlaylistDataMapper.ToModel(playlistEntity, itemEntities));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<Playlist>)result);
        }

        public Task<Playlist> GetByIdAsync(PlaylistId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from playlists where id = $id limit 1;";
                command.Parameters.AddWithValue("$id", id.ToString());

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var playlistEntity = ReadPlaylistEntity(reader);
                        var itemEntities = GetItemEntities(connection, playlistEntity.Id);
                        return Task.FromResult(PlaylistDataMapper.ToModel(playlistEntity, itemEntities));
                    }
                }
            }

            return Task.FromResult<Playlist>(null);
        }

        public Task UpsertAsync(Playlist playlist)
        {
            var playlistEntity = PlaylistDataMapper.ToEntity(playlist);
            var itemEntities = PlaylistDataMapper.ToItemEntities(playlist);

            using (var connection = _database.CreateOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    ApplyPlaylistUpsertCommand(command, playlistEntity);
                    command.ExecuteNonQuery();
                }

                using (var deleteCommand = connection.CreateCommand())
                {
                    deleteCommand.Transaction = transaction;
                    deleteCommand.CommandText = "delete from playlist_items where playlist_id = $playlistId;";
                    deleteCommand.Parameters.AddWithValue("$playlistId", playlistEntity.Id);
                    deleteCommand.ExecuteNonQuery();
                }

                foreach (var itemEntity in itemEntities)
                {
                    using (var itemCommand = connection.CreateCommand())
                    {
                        itemCommand.Transaction = transaction;
                        ApplyPlaylistItemInsertCommand(itemCommand, itemEntity);
                        itemCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(PlaylistId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var itemCommand = connection.CreateCommand())
                {
                    itemCommand.Transaction = transaction;
                    itemCommand.CommandText = "delete from playlist_items where playlist_id = $playlistId;";
                    itemCommand.Parameters.AddWithValue("$playlistId", id.ToString());
                    itemCommand.ExecuteNonQuery();
                }

                using (var playlistCommand = connection.CreateCommand())
                {
                    playlistCommand.Transaction = transaction;
                    playlistCommand.CommandText = "delete from playlists where id = $id;";
                    playlistCommand.Parameters.AddWithValue("$id", id.ToString());
                    playlistCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            using (var connection = _database.CreateOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var itemCommand = connection.CreateCommand())
                {
                    itemCommand.Transaction = transaction;
                    itemCommand.CommandText = "delete from playlist_items;";
                    itemCommand.ExecuteNonQuery();
                }

                using (var playlistCommand = connection.CreateCommand())
                {
                    playlistCommand.Transaction = transaction;
                    playlistCommand.CommandText = "delete from playlists;";
                    playlistCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            return Task.CompletedTask;
        }

        private static List<PlaylistItemEntity> GetItemEntities(SqliteConnection connection, string playlistId)
        {
            var result = new List<PlaylistItemEntity>();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from playlist_items where playlist_id = $playlistId order by item_order;";
                command.Parameters.AddWithValue("$playlistId", playlistId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(ReadPlaylistItemEntity(reader));
                    }
                }
            }

            return result;
        }

        private static void ApplyPlaylistUpsertCommand(SqliteCommand command, PlaylistEntity entity)
        {
            command.CommandText = @"
                insert or replace into playlists (
                    id,
                    name,
                    description,
                    created_at,
                    updated_at
                ) values (
                    $id,
                    $name,
                    $description,
                    $createdAt,
                    $updatedAt
                );";

            command.Parameters.AddWithValue("$id", entity.Id);
            command.Parameters.AddWithValue("$name", entity.Name);
            command.Parameters.AddWithValue("$description", entity.Description);
            command.Parameters.AddWithValue("$createdAt", entity.CreatedAtUnixTimeMilliseconds);
            command.Parameters.AddWithValue("$updatedAt", entity.UpdatedAtUnixTimeMilliseconds);
        }

        private static void ApplyPlaylistItemInsertCommand(SqliteCommand command, PlaylistItemEntity entity)
        {
            command.CommandText = @"
                insert INTO playlist_items (
                    id,
                    playlist_id,
                    music_id,
                    item_order,
                    added_at
                ) values (
                    $id,
                    $playlistId,
                    $musicId,
                    $itemOrder,
                    $addedAt
                );";

            command.Parameters.AddWithValue("$id", entity.Id);
            command.Parameters.AddWithValue("$playlistId", entity.PlaylistId);
            command.Parameters.AddWithValue("$musicId", entity.MusicId);
            command.Parameters.AddWithValue("$itemOrder", entity.Order);
            command.Parameters.AddWithValue("$addedAt", entity.AddedAtUnixTimeMilliseconds);
        }

        private static PlaylistEntity ReadPlaylistEntity(SqliteDataReader reader)
        {
            return new PlaylistEntity
            {
                Id = reader.GetStringOrEmpty("id"),
                Name = reader.GetStringOrEmpty("name"),
                Description = reader.GetStringOrEmpty("description"),
                CreatedAtUnixTimeMilliseconds = reader.GetInt64OrDefault("created_at"),
                UpdatedAtUnixTimeMilliseconds = reader.GetInt64OrDefault("updated_at")
            };
        }

        private static PlaylistItemEntity ReadPlaylistItemEntity(SqliteDataReader reader)
        {
            return new PlaylistItemEntity
            {
                Id = reader.GetStringOrEmpty("id"),
                PlaylistId = reader.GetStringOrEmpty("playlist_id"),
                MusicId = reader.GetStringOrEmpty("music_id"),
                Order = reader.GetInt32OrDefault("item_order"),
                AddedAtUnixTimeMilliseconds = reader.GetInt64OrDefault("added_at")
            };
        }
    }
}

using CorePlanetMusicPlayer.Core.Artwork;
using CorePlanetMusicPlayer.Core.Common;
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
    public sealed class SqliteArtworkRepository : IArtworkRepository
    {
        private readonly LibraryDatabase _database;

        public SqliteArtworkRepository(LibraryDatabase database)
        {
            Guard.NotNull(database, nameof(database));

            _database = database;
        }

        public Task<ArtworkAssignment> GetByOwnerAsync(ArtworkOwner owner)
        {
            Guard.NotNull(owner, nameof(owner));

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT *
                    FROM artwork_assignments
                    WHERE owner_kind = $ownerKind
                      AND owner_id = $ownerId
                    LIMIT 1;";

                AddOwnerParameters(command, owner);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var assignment = ArtworkAssignmentDataMapper.ToModel(ReadEntity(reader));

                        return Task.FromResult(assignment);
                    }
                }
            }

            return Task.FromResult<ArtworkAssignment>(null);
        }

        public Task<IReadOnlyList<ArtworkAssignment>> GetAllAsync()
        {
            var result = new List<ArtworkAssignment>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT *
                    FROM artwork_assignments
                    ORDER BY owner_kind, owner_id;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(ArtworkAssignmentDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<ArtworkAssignment>)result);
        }

        public Task UpsertAsync(ArtworkAssignment assignment)
        {
            Guard.NotNull(assignment, nameof(assignment));

            var entity = ArtworkAssignmentDataMapper.ToEntity(assignment);

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                ApplyUpsertCommand(command, entity);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task DeleteByOwnerAsync(ArtworkOwner owner)
        {
            Guard.NotNull(owner, nameof(owner));

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    DELETE FROM artwork_assignments
                    WHERE owner_kind = $ownerKind
                      AND owner_id = $ownerId;";

                AddOwnerParameters(command, owner);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        private static void AddOwnerParameters(SqliteCommand command, ArtworkOwner owner)
        {
            command.Parameters.AddWithValue("$ownerKind", (int)owner.Kind);

            command.Parameters.AddWithValue("$ownerId", owner.Id);
        }

        private static void ApplyUpsertCommand(SqliteCommand command, ArtworkAssignmentEntity entity)
        {
            command.CommandText = @"
                INSERT INTO artwork_assignments (
                    owner_kind,
                    owner_id,
                    source_kind,
                    resource_key,
                    remote_url,
                    updated_at
                ) VALUES (
                    $ownerKind,
                    $ownerId,
                    $sourceKind,
                    $resourceKey,
                    $remoteUrl,
                    $updatedAt
                )
                ON CONFLICT (owner_kind, owner_id)
                DO UPDATE SET
                    source_kind = excluded.source_kind,
                    resource_key = excluded.resource_key,
                    remote_url = excluded.remote_url,
                    updated_at = excluded.updated_at;";

            command.Parameters.AddWithValue(
                "$ownerKind",
                entity.OwnerKind);

            command.Parameters.AddWithValue(
                "$ownerId",
                entity.OwnerId);

            command.Parameters.AddWithValue(
                "$sourceKind",
                entity.SourceKind);

            command.Parameters.AddWithValue(
                "$resourceKey",
                entity.ResourceKey);

            command.Parameters.AddWithValue(
                "$remoteUrl",
                entity.RemoteUrl);

            command.Parameters.AddWithValue(
                "$updatedAt",
                entity.UpdatedAtUnixTimeMilliseconds);
        }

        private static ArtworkAssignmentEntity ReadEntity(SqliteDataReader reader)
        {
            return new ArtworkAssignmentEntity
            {
                OwnerKind = reader.GetInt32OrDefault("owner_kind"),
                OwnerId = reader.GetStringOrEmpty("owner_id"),
                SourceKind = reader.GetInt32OrDefault("source_kind"),
                ResourceKey = reader.GetStringOrEmpty("resource_key"),
                RemoteUrl = reader.GetStringOrEmpty("remote_url"),
                UpdatedAtUnixTimeMilliseconds =reader.GetInt64OrDefault("updated_at")
            };
        }
    }
}

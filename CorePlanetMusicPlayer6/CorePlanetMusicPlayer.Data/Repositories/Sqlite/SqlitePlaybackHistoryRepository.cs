using CorePlanetMusicPlayer.Core.History;
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
    public sealed class SqlitePlaybackHistoryRepository : IPlaybackHistoryRepository
    {
        private readonly LibraryDatabase _database;

        public SqlitePlaybackHistoryRepository(LibraryDatabase database)
        {
            _database = database;
        }

        public Task<IReadOnlyList<PlaybackHistoryItem>> GetRecentAsync(int maxCount)
        {
            var result = new List<PlaybackHistoryItem>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from playback_history order_by played_at desc limit $maxCount;";
                command.Parameters.AddWithValue("$maxCount", maxCount);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(PlaybackHistoryDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<PlaybackHistoryItem>)result);
        }

        public Task<IReadOnlyList<PlaybackHistoryItem>> GetByMusicIdAsync(MusicId musicId)
        {
            var result = new List<PlaybackHistoryItem>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from playback_history where music_id = $musicId order_by played_at desc;";
                command.Parameters.AddWithValue("$musicId", musicId.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(PlaybackHistoryDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<PlaybackHistoryItem>)result);
        }

        public Task<IReadOnlyList<PlaybackHistoryItem>> GetByDateRangeAsync(
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            var result = new List<PlaybackHistoryItem>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    select * from playback_history
                    where played_at >= $startTime
                      and played_at <= $endTime
                    order by played_at DESC;";

                command.Parameters.AddWithValue("$startTime", startTime.ToUnixTimeMilliseconds());
                command.Parameters.AddWithValue("$endTime", endTime.ToUnixTimeMilliseconds());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(PlaybackHistoryDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<PlaybackHistoryItem>)result);
        }

        public Task<IReadOnlyList<PlaybackHistoryItem>> GetByMusicIdAndDateRangeAsync(
            MusicId musicId,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            var result = new List<PlaybackHistoryItem>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    select * from playback_history
                    where music_id = $musicId
                      AND played_at >= $startTime
                      AND played_at <= $endTime
                    order by played_at desc;";

                command.Parameters.AddWithValue("$musicId", musicId.ToString());
                command.Parameters.AddWithValue("$startTime", startTime.ToUnixTimeMilliseconds());
                command.Parameters.AddWithValue("$endTime", endTime.ToUnixTimeMilliseconds());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(PlaybackHistoryDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<PlaybackHistoryItem>)result);
        }

        public Task AddAsync(PlaybackHistoryItem item)
        {
            var entity = PlaybackHistoryDataMapper.ToEntity(item);

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    insert into playback_history (
                        id,
                        music_id,
                        played_at,
                        music_duration_ticks,
                        played_duration_ticks,
                        last_position_ticks,
                        is_completed
                    ) values (
                        $id,
                        $musicId,
                        $playedAt,
                        $musicDurationTicks,
                        $playedDurationTicks,
                        $lastPositionTicks,
                        $isCompleted
                    );";

                command.Parameters.AddWithValue("$id", entity.Id);
                command.Parameters.AddWithValue("$musicId", entity.MusicId);
                command.Parameters.AddWithValue("$playedAt", entity.PlayedAtUnixTimeMilliseconds);
                command.Parameters.AddWithValue("$musicDurationTicks", entity.MusicDurationTicks);
                command.Parameters.AddWithValue("$playedDurationTicks", entity.PlayedDurationTicks);
                command.Parameters.AddWithValue("$lastPositionTicks", entity.LastPositionTicks);
                command.Parameters.AddWithValue("$isCompleted", entity.IsCompleted ? 1 : 0);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(PlaybackHistoryId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from playback_history where id = $id;";
                command.Parameters.AddWithValue("$id", id.ToString());
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task DeleteBeforeAsync(DateTimeOffset time)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from playback_history where played_at < $time;";
                command.Parameters.AddWithValue("$time", time.ToUnixTimeMilliseconds());
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from playback_history;";
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        private static PlaybackHistoryEntity ReadEntity(SqliteDataReader reader)
        {
            return new PlaybackHistoryEntity
            {
                Id = reader.GetStringOrEmpty("id"),
                MusicId = reader.GetStringOrEmpty("music_id"),
                PlayedAtUnixTimeMilliseconds = reader.GetInt64OrDefault("played_at"),
                MusicDurationTicks = reader.GetInt64OrDefault("music_duration_ticks"),
                PlayedDurationTicks = reader.GetInt64OrDefault("played_duration_ticks"),
                LastPositionTicks = reader.GetInt64OrDefault("last_position_ticks"),
                IsCompleted = reader.GetBooleanFromInt32("is_completed")
            };
        }
    }
}

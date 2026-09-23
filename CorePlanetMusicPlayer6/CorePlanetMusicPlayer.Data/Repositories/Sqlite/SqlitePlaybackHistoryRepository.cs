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
                command.CommandText = "select * from playback_history order by played_at desc limit $maxCount;";
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
                command.CommandText = "select * from playback_history where music_id = $musicId order by played_at desc;";
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
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null)
        {
            if (startTime.HasValue &&
                endTime.HasValue &&
                endTime.Value < startTime.Value)
            {
                throw new ArgumentException(
                    "结束时间不能早于开始时间。",
                    nameof(endTime));
            }

            var result = new List<PlaybackHistoryItem>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                var conditions = new List<string>();

                if (startTime.HasValue)
                {
                    conditions.Add("played_at >= $startTime");

                    command.Parameters.AddWithValue(
                        "$startTime",
                        startTime.Value.ToUnixTimeMilliseconds());
                }

                if (endTime.HasValue)
                {
                    conditions.Add("played_at <= $endTime");

                    command.Parameters.AddWithValue(
                        "$endTime",
                        endTime.Value.ToUnixTimeMilliseconds());
                }

                var sql = new StringBuilder(
                    "select * from playback_history");

                if (conditions.Count > 0)
                {
                    sql.Append(" where ");
                    sql.Append(string.Join(" and ", conditions));
                }

                sql.Append(" order by played_at desc, id desc;");

                command.CommandText = sql.ToString();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(
                            PlaybackHistoryDataMapper.ToModel(
                                ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult(
                (IReadOnlyList<PlaybackHistoryItem>)result);
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
                        title_snapshot,
                        artist_name_snapshot,
                        album_title_snapshot,
                        played_at,
                        music_duration_ticks,
                        played_duration_ticks,
                        last_position_ticks,
                        is_completed
                    ) values (
                        $id,
                        $musicId,
                        $titleSnapshot,
                        $artistNameSnapshot,
                        $albumTitleSnapshot,
                        $playedAt,
                        $musicDurationTicks,
                        $playedDurationTicks,
                        $lastPositionTicks,
                        $isCompleted
                    );";

                command.Parameters.AddWithValue("$id", entity.Id);
                command.Parameters.AddWithValue("$musicId", entity.MusicId);
                command.Parameters.AddWithValue("$titleSnapshot", entity.TitleSnapshot);
                command.Parameters.AddWithValue("$artistNameSnapshot", entity.ArtistNameSnapshot);
                command.Parameters.AddWithValue("$albumTitleSnapshot", entity.AlbumTitleSnapshot);
                command.Parameters.AddWithValue("$playedAt", entity.PlayedAtUnixTimeMilliseconds);
                command.Parameters.AddWithValue("$musicDurationTicks", entity.MusicDurationTicks);
                command.Parameters.AddWithValue("$playedDurationTicks", entity.PlayedDurationTicks);
                command.Parameters.AddWithValue("$lastPositionTicks", entity.LastPositionTicks);
                command.Parameters.AddWithValue("$isCompleted", entity.IsCompleted ? 1 : 0);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }


        public Task UpsertAsync(PlaybackHistoryItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.Id.IsEmpty)
            {
                throw new ArgumentException(
                    "历史记录 ID 不能为空。",
                    nameof(item));
            }

            if (item.MusicId.IsEmpty)
            {
                throw new ArgumentException(
                    "歌曲 ID 不能为空。",
                    nameof(item));
            }

            var entity = PlaybackHistoryDataMapper.ToEntity(item);

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                insert into playback_history (
                    id,
                    music_id,
                    title_snapshot,
                    artist_name_snapshot,
                    album_title_snapshot,
                    played_at,
                    music_duration_ticks,
                    played_duration_ticks,
                    last_position_ticks,
                    is_completed
                ) values (
                    $id,
                    $musicId,
                    $titleSnapshot,
                    $artistNameSnapshot,
                    $albumTitleSnapshot,
                    $playedAt,
                    $musicDurationTicks,
                    $playedDurationTicks,
                    $lastPositionTicks,
                    $isCompleted
                )
                on conflict(id) do update set
                    music_id = excluded.music_id,
                    played_at = excluded.played_at,
                    music_duration_ticks = excluded.music_duration_ticks,
                    played_duration_ticks = excluded.played_duration_ticks,
                    last_position_ticks = excluded.last_position_ticks,
                    is_completed = excluded.is_completed;";

                command.Parameters.AddWithValue(
                    "$id",
                    entity.Id);

                command.Parameters.AddWithValue(
                    "$musicId",
                    entity.MusicId);

                command.Parameters.AddWithValue(
                    "$titleSnapshot",
                    entity.TitleSnapshot);

                command.Parameters.AddWithValue(
                    "$artistNameSnapshot",
                    entity.ArtistNameSnapshot);

                command.Parameters.AddWithValue(
                    "$albumTitleSnapshot",
                    entity.AlbumTitleSnapshot);

                command.Parameters.AddWithValue(
                    "$playedAt",
                    entity.PlayedAtUnixTimeMilliseconds);

                command.Parameters.AddWithValue(
                    "$musicDurationTicks",
                    entity.MusicDurationTicks);

                command.Parameters.AddWithValue(
                    "$playedDurationTicks",
                    entity.PlayedDurationTicks);

                command.Parameters.AddWithValue(
                    "$lastPositionTicks",
                    entity.LastPositionTicks);

                command.Parameters.AddWithValue(
                    "$isCompleted",
                    entity.IsCompleted ? 1 : 0);

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
                TitleSnapshot = reader.GetStringOrEmpty("title_snapshot"),
                ArtistNameSnapshot = reader.GetStringOrEmpty("artist_name_snapshot"),
                AlbumTitleSnapshot = reader.GetStringOrEmpty("album_title_snapshot"),
                PlayedAtUnixTimeMilliseconds = reader.GetInt64OrDefault("played_at"),
                MusicDurationTicks = reader.GetInt64OrDefault("music_duration_ticks"),
                PlayedDurationTicks = reader.GetInt64OrDefault("played_duration_ticks"),
                LastPositionTicks = reader.GetInt64OrDefault("last_position_ticks"),
                IsCompleted = reader.GetBooleanFromInt32("is_completed")
            };
        }
    }
}

using CorePlanetMusicPlayer.Core.Common;
using CorePlanetMusicPlayer.Core.Library;
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
    public sealed class SqliteMusicRepository : IMusicRepository
    {
        private readonly LibraryDatabase _database;

        public SqliteMusicRepository(LibraryDatabase libraryDatabase)
        {
            _database = libraryDatabase;
        }

        public Task<IReadOnlyList<Music>> GetAllAsync()
        {
            var result = new List<Music>();

            using(var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from music order by title;";

                using (var reader = command.ExecuteReader())
                {
                    result.Add(MusicDataMapper.ToModel(ReadEntity(reader)));
                }
            }

            return Task.FromResult((IReadOnlyList<Music>)result);
        }

        public Task<Music> GetByIdAsync(MusicId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from music where id = $id limit 1;";
                command.Parameters.AddWithValue("id", id.ToString());

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Task.FromResult(MusicDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult<Music>(null);
        }

        public Task<IReadOnlyList<Music>> SearchAsync(string keyword)
        {
            var result = new List<Music>();
            var safeKeyword = keyword ?? string.Empty;

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    select * from music
                    where title like $keyword
                        or artist_name like $keyword
                        or album_title like $keyword
                    order by title;";

                command.Parameters.AddWithValue("$keyword", $"%{safeKeyword}%");
                
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MusicDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<Music>)result);
        }

        public Task<IReadOnlyList<Music>> GetByLibraryFolderIdAsync(LibraryFolderId libraryFolderId)
        {
            var result = new List<Music>();

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from music where library_folder_id = $libraryFolderId order by title;";
                command.Parameters.AddWithValue("$libraryFolderId", libraryFolderId.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(MusicDataMapper.ToModel(ReadEntity(reader)));
                    }
                }
            }

            return Task.FromResult((IReadOnlyList<Music>)result);
        }

        public Task UpsertAsync(Music music)
        {
            var entity = MusicDataMapper.ToEntity(music);

            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                ApplyUpsertCommand(command, entity);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task UpsertRangeAsync(IEnumerable<Music> musicList)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                if (musicList != null)
                {
                    foreach(var music in musicList)
                    {
                        var entity = MusicDataMapper.ToEntity(music);

                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            ApplyUpsertCommand(command, entity);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                transaction.Commit();
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(MusicId id)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from music where id = $id;";
                command.Parameters.AddWithValue("$id", id.ToString());
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task DeleteByLibraryFolderIdAsync(LibraryFolderId libraryFolderId)
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from music where library_folder_id = $libraryFolderId;";
                command.Parameters.AddWithValue("$libraryFolderId", libraryFolderId.ToString());
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task ClearLocalMusicAsync()
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from music where source_type = $sourceType;";
                command.Parameters.AddWithValue("$sourceType", (int)MusicSourceType.Local);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        public Task ClearStreamMusicAsync()
        {
            using (var connection = _database.CreateOpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from music where source_type = $sourceType;";
                command.Parameters.AddWithValue("$sourceType", (int)MusicSourceType.Stream);
                command.ExecuteNonQuery();
            }

            return Task.CompletedTask;
        }

        private static void ApplyUpsertCommand(SqliteCommand command, MusicEntity entity)
        {
            command.CommandText = @"
                insert or replace into music (
                    id,
                    title,
                    album_title,
                    artist_name,
                    album_artist_name,
                    gener,
                    year,
                    track_number,
                    disc_number,
                    composer,
                    comment,
                    duration_ticks,
                    source_type,
                    file_path,
                    relative_path,
                    file_name,
                    extension,
                    size,
                    last_modified_at,
                    library_folder_id,
                    last_played_at
                ) values (
                    $id,
                    $title,
                    $albumTitle,
                    $artistName,
                    $albumArtistName,
                    $genre,
                    $year,
                    $trackNumber,
                    $discNumber,
                    $composer,
                    $comment,
                    $durationTicks,
                    $sourceType,
                    $filePath,
                    $relativePath,
                    $fileName,
                    $extension,
                    $size,
                    $lastModifiedAt,
                    $libraryFolderId,
                    $addedAt,
                    $lastPlayedAt
                );";

            command.Parameters.AddWithValue("$id", entity.Id);
            command.Parameters.AddWithValue("$title", entity.Title);
            command.Parameters.AddWithValue("$albumTitle", entity.AlbumTitle);
            command.Parameters.AddWithValue("$artistName", entity.ArtistName);
            command.Parameters.AddWithValue("$albumArtistName", entity.AlbumArtistName);
            command.Parameters.AddWithValue("$genre", entity.Genre);
            command.Parameters.AddWithValue("$year", (object)entity.Year ?? System.DBNull.Value);
            command.Parameters.AddWithValue("$trackNumber", (object)entity.TrackNumber ?? System.DBNull.Value);
            command.Parameters.AddWithValue("$discNumber", (object)entity.DiscNumber ?? System.DBNull.Value);
            command.Parameters.AddWithValue("$composer", entity.Composer);
            command.Parameters.AddWithValue("$comment", entity.Comment);
            command.Parameters.AddWithValue("$durationTicks", entity.DurationTicks);
            command.Parameters.AddWithValue("$sourceType", entity.SourceType);
            command.Parameters.AddWithValue("$filePath", entity.FilePath);
            command.Parameters.AddWithValue("$relativePath", entity.RelativePath);
            command.Parameters.AddWithValue("$fileName", entity.FileName);
            command.Parameters.AddWithValue("$extension", entity.Extension);
            command.Parameters.AddWithValue("$size", (object)entity.Size ?? System.DBNull.Value);
            command.Parameters.AddWithValue("$lastModifiedAt", (object)entity.LastModifiedAtUnixTimeMilliseconds ?? System.DBNull.Value);
            command.Parameters.AddWithValue("$libraryFolderId", entity.LibraryFolderId);
            command.Parameters.AddWithValue("$addedAt", (object)entity.AddedAtUnixTimeMilliseconds ?? System.DBNull.Value);
            command.Parameters.AddWithValue("$lastPlayedAt", (object)entity.LastPlayedAtUnixTimeMilliseconds ?? System.DBNull.Value);
        }

        private static MusicEntity ReadEntity(SqliteDataReader reader)
        {
            return new MusicEntity
            {
                Id = reader.GetStringOrEmpty("id"),
                Title = reader.GetStringOrEmpty("title"),
                AlbumTitle = reader.GetStringOrEmpty("album_title"),
                ArtistName = reader.GetStringOrEmpty("artist_name"),
                AlbumArtistName = reader.GetStringOrEmpty("album_artist_name"),
                Genre = reader.GetStringOrEmpty("genre"),
                Year = reader.GetNullableInt32("year"),
                TrackNumber = reader.GetNullableInt32("track_number"),
                DiscNumber = reader.GetNullableInt32("disc_number"),
                Composer = reader.GetStringOrEmpty("composer"),
                Comment = reader.GetStringOrEmpty("comment"),
                DurationTicks = reader.GetInt64OrDefault("duration_ticks"),
                SourceType = reader.GetInt32OrDefault("source_type"),
                FilePath = reader.GetStringOrEmpty("file_path"),
                RelativePath = reader.GetStringOrEmpty("relative_path"),
                FileName = reader.GetStringOrEmpty("file_name"),
                Extension = reader.GetStringOrEmpty("extension"),
                Size = reader.GetNullableInt64("size"),
                LastModifiedAtUnixTimeMilliseconds = reader.GetNullableInt64("last_modified_at"),
                LibraryFolderId = reader.GetStringOrEmpty("library_folder_id"),
                AddedAtUnixTimeMilliseconds = reader.GetNullableInt64("added_at"),
                LastPlayedAtUnixTimeMilliseconds = reader.GetNullableInt64("last_played_at")
            };
        }
    }
}

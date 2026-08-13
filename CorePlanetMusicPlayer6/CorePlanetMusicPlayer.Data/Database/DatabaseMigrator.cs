using CorePlanetMusicPlayer.Core.Common;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Database
{
    public sealed class DatabaseMigrator
    {
        public void Migrate(SqliteConnection connection, int targetVersion)
        {
            Guard.NotNull(connection, nameof(connection));
            Guard.NotNegative(targetVersion, nameof(targetVersion));

            var currentVersion = GetCurrentVersion(connection);

            if (currentVersion > targetVersion)
            {
                throw new InvalidOperationException("Database version is newer than the application supports.");
            }

            if (currentVersion < 1 && targetVersion >= 1)
            {
                ApplyVersion1(connection);
                SetCurrentVersion(connection, 1);
                currentVersion = 1;
            }

            if (currentVersion < targetVersion)
            {
                throw new NotSupportedException("No migration path is defined for the target database version.");
            }
        }

        public int GetCurrentVersion(SqliteConnection connection)
        {
            Guard.NotNull(connection, nameof(connection));

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA user_version;";
                var value = command.ExecuteScalar();

                if (value == null)
                {
                    return 0;
                }

                return Convert.ToInt32(value);
            }
        }

        private void SetCurrentVersion(SqliteConnection connection, int version)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"PRAGMA user_version = {version};";
                command.ExecuteNonQuery();
            }
        }

        private void ExecuteNonQuery(SqliteConnection connection, string commandText)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }

        private void ApplyVersion1(SqliteConnection connection)
        {
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS music (
                    id TEXT PRIMARY KEY,
                    title TEXT NOT NULL,
                    album_title TEXT NOT NULL,
                    artist_name TEXT NOT NULL,
                    album_artist_name TEXT NOT NULL,
                    genre TEXT NOT NULL,
                    year INTEGER NULL,
                    track_number INTEGER NULL,
                    disc_number INTEGER NULL,
                    composer TEXT NOT NULL,
                    comment TEXT NOT NULL,
                    duration_ticks INTEGER NOT NULL,
                    source_type INTEGER NOT NULL,
                    file_path TEXT NOT NULL,
                    relative_path TEXT NOT NULL,
                    file_name TEXT NOT NULL,
                    extension TEXT NOT NULL,
                    size INTEGER NULL,
                    last_modified_at INTEGER NULL,
                    library_folder_id TEXT NOT NULL,
                    added_at INTEGER NULL,
                    last_played_at INTEGER NULL
                );");
            
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS albums (
                    id TEXT PRIMARY KEY,
                    title TEXT NOT NULL,
                    artist_name TEXT NOT NULL,
                    album_artist_name TEXT NOT NULL,
                    genre TEXT NOT NULL,
                    year INTEGER NULL,
                    music_ids_text TEXT NOT NULL,
                    total_duration_ticks INTEGER NOT NULL,
                    added_at INTEGER NULL,
                    updated_at INTEGER NULL
                );");
            
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS artists (
                    id TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    sort_name TEXT NOT NULL,
                    music_ids_text TEXT NOT NULL,
                    album_ids_text TEXT NOT NULL,
                    total_duration_ticks INTEGER NOT NULL,
                    added_at INTEGER NULL,
                    updated_at INTEGER NULL
                );");
            
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS playlists (
                    id TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    description TEXT NOT NULL,
                    created_at INTEGER NOT NULL,
                    updated_at INTEGER NOT NULL
                );");
            
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS playlist_items (
                    id TEXT PRIMARY KEY,
                    playlist_id TEXT NOT NULL,
                    music_id TEXT NOT NULL,
                    item_order INTEGER NOT NULL,
                    added_at INTEGER NOT NULL
                );");
            
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS playback_history (
                    id TEXT PRIMARY KEY,
                    music_id TEXT NOT NULL,
                    played_at INTEGER NOT NULL,
                    music_duration_ticks INTEGER NOT NULL,
                    played_duration_ticks INTEGER NOT NULL,
                    last_position_ticks INTEGER NOT NULL,
                    is_completed INTEGER NOT NULL
                );");
            
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS library_folders (
                    id TEXT PRIMARY KEY,
                    display_name TEXT NOT NULL,
                    path TEXT NOT NULL,
                    access_key TEXT NOT NULL,
                    access_kind INTEGER NOT NULL,
                    added_at INTEGER NOT NULL,
                    updated_at INTEGER NOT NULL
                );");
            
            ExecuteNonQuery(connection, @"
                CREATE TABLE IF NOT EXISTS lyrics (
                    id TEXT PRIMARY KEY,
                    music_id TEXT NOT NULL,
                    source_type INTEGER NOT NULL,
                    source_path TEXT NOT NULL,
                    raw_text TEXT NOT NULL,
                    lines_text TEXT NOT NULL,
                    created_at INTEGER NOT NULL,
                    updated_at INTEGER NOT NULL
                );");
            
            ExecuteNonQuery(connection, "CREATE INDEX IF NOT EXISTS idx_music_library_folder_id ON music(library_folder_id);");
            ExecuteNonQuery(connection, "CREATE INDEX IF NOT EXISTS idx_music_title ON music(title);");
            ExecuteNonQuery(connection, "CREATE INDEX IF NOT EXISTS idx_music_artist_name ON music(artist_name);");
            ExecuteNonQuery(connection, "CREATE INDEX IF NOT EXISTS idx_playlist_items_playlist_id ON playlist_items(playlist_id);");
            ExecuteNonQuery(connection, "CREATE INDEX IF NOT EXISTS idx_playback_history_music_id ON playback_history(music_id);");
            ExecuteNonQuery(connection, "CREATE INDEX IF NOT EXISTS idx_playback_history_played_at ON playback_history(played_at);");
            ExecuteNonQuery(connection, "CREATE INDEX IF NOT EXISTS idx_lyrics_music_id ON lyrics(music_id);");
        }
    }
}

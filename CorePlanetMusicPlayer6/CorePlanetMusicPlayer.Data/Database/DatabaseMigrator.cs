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
                ApplyVerson1(connection);
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

        private void ApplyVersion1(SqliteConnection connection)
        {
            // TO-DO: 后续完善
        }
    }
}

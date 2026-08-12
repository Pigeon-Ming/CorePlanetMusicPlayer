using CorePlanetMusicPlayer.Core.Common;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Database
{
    public sealed class LibraryDatabase
    {
        private readonly LibraryDatabaseOptions _options;
        private readonly DatabaseConnectionFactory _connectionFactory;
        private readonly DatabaseMigrator _migrator;

        public LibraryDatabase(LibraryDatabaseOptions options, DatabaseConnectionFactory connectionFactory, DatabaseMigrator migrator)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(connectionFactory, nameof(connectionFactory));
            Guard.NotNull(migrator, nameof(migrator));

            _options = options;
            _connectionFactory = connectionFactory;
            _migrator = migrator;
        }

        public void Initialize()
        {
            EnsureDatabaseDirectory();

            using (var connection = _connectionFactory.CreateConnection())
            {
                _migrator.Migrate(connection, _options.TargetVersion);
            }
        }

        public SqliteConnection CreateOpenConnection()
        {
            EnsureDatabaseDictionary();

            return _connectionFactory.CreateOpenConnection();
        }

        private void EnsureDatabaseDirectory()
        {
            if (string.IsNullOrWhiteSpace(_options.DatabaseDirectory))
            {
                return;
            }

            if (!Directory.Exists(_options.DatabaseDirectory))
            {
                Directory.CreateDirectory(_options.DatabaseDirectory);
            }
        }
    }
}

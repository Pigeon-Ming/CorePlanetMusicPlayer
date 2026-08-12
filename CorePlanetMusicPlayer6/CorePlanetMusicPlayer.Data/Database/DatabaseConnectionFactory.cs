using CorePlanetMusicPlayer.Core.Common;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Database
{
    public sealed class DatabaseConnectionFactory
    {
        private readonly LibraryDatabaseOptions _options;

        public DatabaseConnectionFactory(LibraryDatabaseOptions options)
        {
            Guard.NotNull(options, nameof(options));

            _options = options;
        }

        public SqliteConnection CreateConnection()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = _options.DatabasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };

            return new SqliteConnection(connectionStringBuilder.ConnectionString);
        }

        public SqliteConnection CreateOpenConnection()
        {
            var connection = CreateConnection();
            connection.Open();

            return connection;
        }
    }
}

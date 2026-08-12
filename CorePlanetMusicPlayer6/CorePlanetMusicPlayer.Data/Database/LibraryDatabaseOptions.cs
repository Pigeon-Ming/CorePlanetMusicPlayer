using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Database
{
    public sealed class LibraryDatabaseOptions
    {
        public string DatabaseDirectory { get; set; } = string.Empty;

        public string DatabaseFileName { get; set; } = "library.db";

        public int TargetVersion { get; set; } = 1;

        public string DatabasePath
        {
            get
            {
                if (string.IsNullOrEmpty(DatabaseDirectory))
                {
                    return DatabaseFileName;
                }

                return Path.Combine(DatabaseDirectory, DatabaseFileName);
            }
        }

        public static LibraryDatabaseOptions Create(string databaseDirectory)
        {
            Guard.NotNullOrWhiteSpace(databaseDirectory, nameof(databaseDirectory));

            return new LibraryDatabaseOptions
            {
                DatabaseDirectory = databaseDirectory,
                DatabaseFileName = "library.db",
                TargetVersion = 1
            };
        }

        public static LibraryDatabaseOptions Create(string databaseDirectory, string databaseFileName, int targetVersion)
        {
            Guard.NotNullOrWhiteSpace(databaseDirectory, nameof(databaseDirectory));
            Guard.NotNullOrWhiteSpace(databaseFileName, nameof(databaseFileName));
            Guard.NotNegative(targetVersion, nameof(targetVersion));

            return new LibraryDatabaseOptions
            {
                DatabaseDirectory = databaseDirectory,
                DatabaseFileName = databaseFileName,
                TargetVersion = targetVersion
            };
        }
    }
}

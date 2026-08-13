using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Database
{
    public static class SqliteDataReaderExtensions
    {
        public static string GetStringOrEmpty(this SqliteDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);

            if (reader.IsDBNull(ordinal))
            {
                return string.Empty;
            }

            return reader.GetString(ordinal);
        }

        public static int GetInt32OrDefault(this SqliteDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);

            if (reader.IsDBNull(ordinal))
            {
                return 0;
            }

            return reader.GetInt32(ordinal);
        }

        public static long GetInt64OrDefault(this SqliteDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);

            if (reader.IsDBNull(ordinal))
            {
                return 0;
            }

            return reader.GetInt64(ordinal);
        }

        public static int? GetNullableInt32(this SqliteDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);

            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetInt32(ordinal);
        }

        public static long? GetNullableInt64(this SqliteDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);

            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetInt64(ordinal);
        }

        public static bool GetBooleanFromInt32(this SqliteDataReader reader, string name)
        {
            var ordinal = reader.GetOrdinal(name);

            if (reader.IsDBNull(ordinal))
            {
                return false;
            }

            return reader.GetInt32(ordinal) != 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Data.Mapping
{
    public static class DataValueConverter
    {
        public static DateTimeOffset? FromUnixTimeMilliseconds(long? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeMilliseconds(value.Value);
        }

        public static long? ToUnixTimeMilliseconds(DateTimeOffset? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value.ToUnixTimeMilliseconds();
        }

        public static DateTimeOffset FromUnixTimeMilliseconds(long value)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(value);
        }

        public static long ToUnixTimeMilliseconds(DateTimeOffset value)
        {
            return value.ToUnixTimeMilliseconds();
        }

        public static TimeSpan FromTicks(long ticks)
        {
            return new TimeSpan(ticks);
        }

        public static long ToTicks(TimeSpan value)
        {
            return value.Ticks;
        }
    }
}

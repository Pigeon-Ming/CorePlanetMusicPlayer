using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Common
{
    /// <summary>
    /// 所有 ID 类型的基础
    /// </summary>
    public static class EntityId
    {
        public static string New()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static bool IsEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string Normalize(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}

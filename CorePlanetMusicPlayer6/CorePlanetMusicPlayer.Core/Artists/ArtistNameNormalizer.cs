using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Artists
{
    /// <summary>
    /// 整理已经分开的艺术家名称。
    /// 不负责根据分隔符拆分文本。
    /// </summary>
    public static class ArtistNameNormalizer
    {
        public static List<string> Normalize(IEnumerable<string> names)
        {
            var result = new List<string>();

            if (names == null)
            {
                return result;
            }

            var seenNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (string value in names)
            {
                string name = value == null ? string.Empty : value.Trim();

                if (name.Length == 0)
                {
                    continue;
                }

                if (seenNames.Add(name))
                {
                    result.Add(name);
                }
            }

            return result;
        }
    }
}

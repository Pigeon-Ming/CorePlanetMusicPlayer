using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models.Helpers
{
    public class StringHelper
    {
        public static string TimeNumToString(int Time)
        {
            return Time.ToString().Length == 1 ? "0" + Time.ToString() : Time.ToString();
        }

        public static string StringArrayToString(string[] strArray, string separator)
        {
            if (strArray == null || strArray.Length == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                sb.Append(strArray[i]);
                if (i != strArray.Length - 1)
                    sb.Append(separator);
            }
            return sb.ToString();
        }
        public static string RemoveIllegalCharacter(String str)
        {
            return str.Replace("/", "").Replace("\\", "").Replace("*", "").Replace("?", "").Replace(":", "").Replace("|", "").Replace("\"", "").Replace("<", "").Replace(">", "");
        }


    }
}

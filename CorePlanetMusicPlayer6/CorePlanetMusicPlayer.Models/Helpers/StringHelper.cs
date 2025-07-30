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

        public static TimeSpan ConvertToTimeSpan(string timeString)
        {
            // 检查输入字符串是否为空或者无效
            if (string.IsNullOrEmpty(timeString))
            {
                throw new ArgumentNullException(nameof(timeString), "输入字符串不能为空");
            }

            // 按冒号分割分钟和秒.毫秒部分
            string[] parts = timeString.Split(':');
            if (parts.Length != 2)
            {
                throw new FormatException("输入字符串格式应为'分钟:秒.毫秒'");
            }

            // 尝试解析分钟部分
            if (!int.TryParse(parts[0], out int minutes))
            {
                throw new FormatException("分钟部分解析失败");
            }

            // 按小数点分割秒和毫秒部分
            string[] secondParts = parts[1].Split('.');
            if (secondParts.Length != 2)
            {
                throw new FormatException("秒和毫秒部分格式应为'秒.毫秒'");
            }

            // 尝试解析秒和毫秒部分
            if (!int.TryParse(secondParts[0], out int seconds))
            {
                throw new FormatException("秒部分解析失败");
            }

            if (!int.TryParse(secondParts[1], out int milliseconds))
            {
                throw new FormatException("毫秒部分解析失败");
            }

            // 创建并返回TimeSpan对象
            return new TimeSpan(0, 0, minutes, seconds, milliseconds);
        }
    }
}

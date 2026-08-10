using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Common
{
    /// <summary>
    /// 用于统一做参数检查
    /// </summary>
    public static class Guard
    {
        public static void NotNull(object value, string parmeterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parmeterName);
            }
        }

        public static void NotNullOrWhiteSpace(string value, string parmeterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException("Value cannot be null, empty, or whitespace.", parmeterName);
            }
        }

        /// <summary>
        /// 数值不能为负数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parmeterName"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void NotNegative(int value, string parmeterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parmeterName, "Value cannot be negative.");
            }
        }

        /// <summary>
        /// 数值不能为负数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parmeterName"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void NotNegative(long value, string parmeterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parmeterName, "Value cannot be negative.");
            }
        }

        /// <summary>
        /// 时间不能为负数
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parmeterName"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void NotNegative(TimeSpan value, string parmeterName)
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(parmeterName, "Value cannot be negative.");
            }
        }


        public static void NotDefault<T>(T value, string parmeterName) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                throw new ArgumentException("Value cannot be default.", parmeterName);
            }
        }
    }
}

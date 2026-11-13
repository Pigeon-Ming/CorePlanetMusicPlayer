using System;

namespace CorePlanetMusicPlayer.TagLib.Uwp.Platform.TagLib
{
    /// <summary>
    /// 判断当前 UWP 设备系列是否允许启用 TagLib 扩展。
    /// </summary>
    public static class UwpTagLibPlatformSupport
    {
        public static bool IsSupported(string deviceFamily)
        {
            return string.Equals(
                       deviceFamily,
                       "Windows.Desktop",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       deviceFamily,
                       "Windows.Xbox",
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       deviceFamily,
                       "Windows.Holographic",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}

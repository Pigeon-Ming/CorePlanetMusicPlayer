using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;

namespace CorePlanetMusicPlayer.Uwp.Platform.System
{
    /// <summary>
    /// 获取基础设备信息的工具类
    /// </summary>
    public sealed class UwpDeviceInfoService
    {
        public string GetDeviceFamily()
        {
            try
            {
                return AnalyticsInfo.VersionInfo.DeviceFamily ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetDeviceFamilyVersion()
        {
            try
            {
                return AnalyticsInfo.VersionInfo.DeviceFamilyVersion ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetReadableOsVersion()
        {
            var versionText = GetDeviceFamilyVersion();

            if (string.IsNullOrWhiteSpace(versionText))
            {
                return string.Empty;
            }

            ulong version;

            if (!ulong.TryParse(versionText, out version))
            {
                return versionText;
            }

            var major = (version & 0xFFFF000000000000L) >> 48;
            var minor = (version & 0x0000FFFF00000000L) >> 32;
            var build = (version & 0x00000000FFFF0000L) >> 16;
            var revision = version & 0x000000000000FFFFL;

            return major + "." + minor + "." + build + "." + revision;
        }

        public string GetFriendlyName()
        {
            try
            {
                var info = new EasClientDeviceInformation();

                return info.FriendlyName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetSystemManufacturer()
        {
            try
            {
                var info = new EasClientDeviceInformation();

                return info.SystemManufacturer ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetSystemProductName()
        {
            try
            {
                var info = new EasClientDeviceInformation();

                return info.SystemProductName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool IsDesktopFamily()
        {
            return string.Equals(
                GetDeviceFamily(),
                "Windows.Desktop",
                StringComparison.OrdinalIgnoreCase);
        }

        public bool IsMobileFamily()
        {
            return string.Equals(
                GetDeviceFamily(),
                "Windows.Mobile",
                StringComparison.OrdinalIgnoreCase);
        }

        public bool IsXboxFamily()
        {
            return string.Equals(
                GetDeviceFamily(),
                "Windows.Xbox",
                StringComparison.OrdinalIgnoreCase);
        }
    }
}

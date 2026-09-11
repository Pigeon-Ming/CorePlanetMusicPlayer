using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;

namespace CorePlanetMusicPlayer6.Models
{
    public enum DeviceFormFactorType
    {
        Phone,
        Desktop,
        Tablet,
        IoT,
        SurfaceHub,
        Xbox,
        Holographic,
        Other
    }

    public class SystemHelper
    {
        public static DeviceFormFactorType GetDeviceFormFactorType()
        {
            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Mobile":
                    return DeviceFormFactorType.Phone;
                case "Windows.Desktop":
                    return DeviceFormFactorType.Desktop; //UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse
                                                         //    ? DeviceFormFactorType.Desktop
                                                         //    : DeviceFormFactorType.Tablet;
                case "Windows.Tablet":
                    return DeviceFormFactorType.Tablet;
                case "Windows.IoT":
                    return DeviceFormFactorType.IoT;
                case "Windows.Team":
                    return DeviceFormFactorType.SurfaceHub;
                case "Windows.Xbox":
                    return DeviceFormFactorType.Xbox;
                case "Windows.Holographic":
                    return DeviceFormFactorType.Holographic;
                default:
                    return DeviceFormFactorType.Other;
            }
        }

        public static string GetWindowsVersion()
        {
            string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (v & 0x000000000000FFFFL);
            return $"{v1}.{v2}.{v3}.{v4}";
        }
    }
}

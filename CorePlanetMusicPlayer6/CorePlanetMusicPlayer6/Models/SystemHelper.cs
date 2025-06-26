using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;

namespace CorePlanetMusicPlayer6.Models
{
    public class SystemHelper
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
    }
}

using CorePlanetMusicPlayer6.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.Dev
{
    public sealed partial class DevToolsControl : UserControl
    {
        public DevToolsControl()
        {
            this.InitializeComponent();
        }

        private void OpenLocalFolderButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            _ = Launcher.LaunchFolderAsync(storageFolder);
        }

        private void SetSystemInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("------设备信息------");
            stringBuilder.Append("Windows版本：");
            stringBuilder.AppendLine(SystemHelper.GetWindowsVersion());
            stringBuilder.Append("设备类型：");
            stringBuilder.AppendLine(SystemHelper.GetDeviceFormFactorType().ToString());
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            stringBuilder.Append("系统Sku：");
            stringBuilder.AppendLine(string.IsNullOrEmpty(deviceInfo.SystemSku) ? "（无数据）" : deviceInfo.SystemSku);
            stringBuilder.Append("设备型号：");
            stringBuilder.AppendLine(deviceInfo.SystemProductName);
            stringBuilder.Append("设备名称：");
            stringBuilder.AppendLine(deviceInfo.FriendlyName);
            stringBuilder.Append("设备制造商：");
            stringBuilder.AppendLine(deviceInfo.SystemManufacturer);
            stringBuilder.Append("设备制造商：");
            stringBuilder.AppendLine(deviceInfo.SystemManufacturer);
            stringBuilder.AppendLine();
            SystemInfoTextBlock.Text = stringBuilder.ToString();
        }

        private void SetPackageInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("------软件包信息------");
            Package package = Package.Current;
            stringBuilder.Append("包显示名：");
            stringBuilder.AppendLine(package.DisplayName);
            stringBuilder.Append("包名：");
            stringBuilder.AppendLine(package.Id.FullName);
            stringBuilder.Append("架构：");
            stringBuilder.AppendLine(package.Id.Architecture.ToString());
            stringBuilder.Append("发布者：");
            stringBuilder.AppendLine(package.PublisherDisplayName);
            stringBuilder.Append("软件版本：");
            stringBuilder.AppendLine($"{package.Id.Version.Major.ToString()}.{package.Id.Version.Minor.ToString()}.{package.Id.Version.Build.ToString()}.{package.Id.Version.Revision.ToString()}");
            stringBuilder.Append("此版本安装日期：");
            stringBuilder.AppendLine(package.InstalledDate.ToString());
            stringBuilder.AppendLine("依赖项：");
            IReadOnlyList<Package> dependencies = package.Dependencies;
            foreach (Package item in dependencies)
            {
                stringBuilder.AppendLine(item.DisplayName);
            }
            stringBuilder.AppendLine();
            PackageInfoTextBlock.Text = stringBuilder.ToString();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetSystemInfo();
            SetPackageInfo();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Devices.Enumeration;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public class RemovableDevice
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public StorageFolder StorageFolder { get; set; }

        public List<RemovableMusic> Music { get; set; }

        public List<Artist> Artists { get; set; }

        public List<Album> Albums { get; set; }

        public List<Genre> Genres { get; set; }
    }

    public class RemovableDeviceManager
    {
        public static ObservableCollection<RemovableDevice> RemovableDevices { get; set; } = new ObservableCollection<RemovableDevice>();

        public static DeviceWatcher RemovableDevicesWatcher { get; set; }
        public static bool WatcherRunning { get; private set; } = false;

        public static void StartWatcher()
        {
            //string[] requestedProperties = { "System.Devices.InterfaceClassGuid" };
            //string aqsFilter = "System.Devices.InterfaceClassGuid:=\"{A5DCBF10-6530-11D2-901F-00C04FB951ED}\"";
            RemovableDevicesWatcher = DeviceInformation.CreateWatcher(DeviceClass.PortableStorageDevice);
            RemovableDevicesWatcher.Added += RemovableDevicesWatcher_Added;
            RemovableDevicesWatcher.Removed += RemovableDevicesWatcher_Removed;
            RemovableDevicesWatcher.Start();
            WatcherRunning = true;
            Debug.WriteLine("对可移动设备的监听已开始。");
        }

        public static void StopWatcher()
        {
            //string[] requestedProperties = { "System.Devices.InterfaceClassGuid" };
            //string aqsFilter = "System.Devices.InterfaceClassGuid:=\"{A5DCBF10-6530-11D2-901F-00C04FB951ED}\"";
            //RemovableDevicesWatcher = DeviceInformation.CreateWatcher(DeviceClass.PortableStorageDevice);
            if (RemovableDevicesWatcher == null)
            {
                return;
            }
            RemovableDevicesWatcher.Added -= RemovableDevicesWatcher_Added;
            RemovableDevicesWatcher.Removed -= RemovableDevicesWatcher_Removed;
            RemovableDevicesWatcher.Stop();
            WatcherRunning = false;
            Debug.WriteLine("对可移动设备的监听已停止。");
        }

        public static async Task RefreshDevicesListAsync()
        {
            RemovableDevices.Clear();
            
            List<StorageFolder> folders = await StorageHelper.GetRemovableDevicesStorageFolderAsync();
            List<RemovableDevice> devices = new List<RemovableDevice>();
            foreach (StorageFolder folder in folders)
            {
                devices.Add(CreateRemovableDevice(folder));
            }
            RemovableDevices = new ObservableCollection<RemovableDevice>(devices);
        }

        private static RemovableDevice CreateRemovableDevice(StorageFolder storageFolder)
        {
            RemovableDevice removableDevice = new RemovableDevice();
            removableDevice.StorageFolder = storageFolder;
            /*To-Do: 增加可移动设备的唯一标识*/
            //removableDevice.Id = storageFolder.Properties.;
            //Debug.WriteLine($"可移动设备：{storageFolder.Name}（{storageFolder.Path}）的Id为：{removableDevice.Id}");
            removableDevice.Name = storageFolder.Name;
            return removableDevice;
        }

        private static async void RemovableDevicesWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            await RefreshDevicesListAsync();
        }

        private static async void RemovableDevicesWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            await RefreshDevicesListAsync();
        }

        public static async Task GetRemovableDeviceMusicListAsync(RemovableDevice removableDevice)
        {
            if (await StorageHelper.IsFolderExistAsync(removableDevice.StorageFolder, "music"))
                removableDevice.Music = await RemovableMusicManager.GetRemovableMusicFromStorageFolderAsync(await removableDevice.StorageFolder.GetFolderAsync("music"), removableDevice);
            else
                removableDevice.Music = await RemovableMusicManager.GetRemovableMusicFromStorageFolderAsync(removableDevice.StorageFolder, removableDevice);
            Debug.WriteLine(removableDevice.Music.Count);
        }
    }
}

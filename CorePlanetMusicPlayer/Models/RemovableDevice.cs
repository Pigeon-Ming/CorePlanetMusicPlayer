using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.WebUI;

namespace CorePlanetMusicPlayer.Models
{
    public class RemovableDevice
    {
        public string Name { get; set; } = "";

        public string Key { get; set; } = "";
        public StorageFolder RootFolder { get; set; } = null;
        public List<Music> Music { get; set; } = new List<Music>();
        public List<StorageFile> Files { get; set; } = new List<StorageFile>();
    }

    public class RemovableDeviceManager
    {
        public static List<RemovableDevice>Devices { get; set; } = new List<RemovableDevice>();

        public static DeviceWatcher RemovableDevicesWatcher { get; set; }

        public static EventHandler DevicesChanged { get; set; }

        public static EventHandler DevicesAdded { get; set; }

        public static RemovableDevice GetRemovableDevice(StorageFolder storageFolder)
        {
            RemovableDevice device = new RemovableDevice();
            device.Name = storageFolder.Name;
            device.RootFolder = storageFolder;
            device.Key = storageFolder.Path;
            //Queue<StorageFolder> folderQueue = new Queue<StorageFolder>();
            //folderQueue.Enqueue(storageFolder);
            //do
            //{
            //    IReadOnlyList<IStorageItem> folderList = await folderQueue.Dequeue().GetItemsAsync();
            //    foreach (var item in folderList)
            //    {
            //        if (item is StorageFolder)
            //        {
            //            folderQueue.Enqueue((StorageFolder)item);
            //        }
            //        else
            //        {
            //            //MusicFileCount ++;
            //            string fileName = item.Name;
            //            //Debug.WriteLine(fileName+"|||"+fileName.Substring(fileName.LastIndexOf(".")));
            //            string fileSuffix = fileName.Substring(fileName.LastIndexOf("."));
            //            if (fileSuffix == ".mp3" || fileSuffix == ".flac" || fileSuffix == ".wma" || fileSuffix == ".m4a" || fileSuffix == ".ac3" || fileSuffix == ".aac")
            //            {
            //                StorageFile storageFile = item as StorageFile;
            //                device.Files.Add(storageFile);
            //            }
            //        }
            //    }
            //} while (folderQueue.Count > 0);

            return device;
        }

        public static bool WatcherRunning { get; set; } = false;

        public static void InitWatcher()
        {
            //string[] requestedProperties = { "System.Devices.InterfaceClassGuid" };
            //string aqsFilter = "System.Devices.InterfaceClassGuid:=\"{A5DCBF10-6530-11D2-901F-00C04FB951ED}\"";
            RemovableDevicesWatcher = DeviceInformation.CreateWatcher(DeviceClass.PortableStorageDevice);
            RemovableDevicesWatcher.Added += DeviceWatcher_Added;
            RemovableDevicesWatcher.Removed += DeviceWatcher_Removed;
            RemovableDevicesWatcher.Start();
            WatcherRunning = true;
            Debug.WriteLine("对可移动设备的监听已开始。");
        }

        public static void StopWatcher()
        {
            //string[] requestedProperties = { "System.Devices.InterfaceClassGuid" };
            //string aqsFilter = "System.Devices.InterfaceClassGuid:=\"{A5DCBF10-6530-11D2-901F-00C04FB951ED}\"";
            //RemovableDevicesWatcher = DeviceInformation.CreateWatcher(DeviceClass.PortableStorageDevice);
            if (RemovableDevicesWatcher==null)
            {
                return;
            }
            RemovableDevicesWatcher.Added -= DeviceWatcher_Added;
            RemovableDevicesWatcher.Removed -= DeviceWatcher_Removed;
            RemovableDevicesWatcher.Stop();
            WatcherRunning = false;
            Debug.WriteLine("对可移动设备的监听已停止。");
        }



        public static async Task RefreshDevicesListAsync()
        {
            Devices.Clear();
            List<StorageFolder> folders = await StorageManager.GetRemovableDevicesAsync();
            foreach (StorageFolder folder in folders)
            {
                Devices.Add(GetRemovableDevice(folder));
            }

        }

        public static async Task RefreshDeviceData(RemovableDevice removableDevice)
        {
            removableDevice.Files = await RemovableDeviceManager.ScanMusicFilesAsync(removableDevice);
            removableDevice.Music = RemovableDeviceManager.GetMusicList(removableDevice);
            await RemovableDeviceManager.GetMusicPropertiesAsync(removableDevice);
        }

        private static async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            Debug.WriteLine("可移动设备："+args.Id+"已拔出。kind:"+args.Kind);
            await RefreshDevicesListAsync();
            DevicesChanged?.Invoke(null,null);
        }

        private static async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            Debug.WriteLine("可移动设备：" + args.Id + "已连接。kind:" + args.Kind);
            await RefreshDevicesListAsync();
            DevicesChanged?.Invoke(null,null);
            DevicesAdded?.Invoke(null, null);
        }

        public static async Task<List<StorageFile>> ScanMusicFilesAsync(RemovableDevice removableDevice)
        {
            //var results = removableDevice.RootFolder.CreateFileQueryWithOptions(new QueryOptions(CommonFileQuery.OrderByTitle,StorageManager.SupportedMusicFileTypes));

            //// Iterate over the results and print the list of files
            //// to the Visual Studio Output window.
            //IReadOnlyList<StorageFile> sortedFiles = await results.GetFilesAsync();
            List<StorageFile> files = new List<StorageFile>();
            Queue<StorageFolder> folderQueue = new Queue<StorageFolder>();
            folderQueue.Enqueue(removableDevice.RootFolder);
            do
            {
                IReadOnlyList<IStorageItem> folderList = await folderQueue.Dequeue().GetItemsAsync();
                foreach (var item in folderList)
                {
                    if (item is StorageFolder)
                    {
                        folderQueue.Enqueue((StorageFolder)item);
                    }
                    else
                    {
                        //MusicFileCount ++;
                        string fileName = item.Name;
                        //Debug.WriteLine(fileName+"|||"+fileName.Substring(fileName.LastIndexOf(".")));
                        string fileSuffix = fileName.Substring(fileName.LastIndexOf("."));
                        if (fileSuffix == ".mp3" || fileSuffix == ".flac" || fileSuffix == ".wma" || fileSuffix == ".m4a" || fileSuffix == ".ac3" || fileSuffix == ".aac")
                        {
                            StorageFile storageFile = item as StorageFile;
                            files.Add(storageFile);
                        }
                    }
                }
            } while (folderQueue.Count > 0);
            return files.ToList();
        }

        public static List<Music> GetMusicList(RemovableDevice removableDevice)
        {
            List<StorageFile>files = removableDevice.Files;
            List<Music>musicList = new List<Music>();
            foreach (StorageFile file in files)
            {
                Music music = new Music();
                music.Key = removableDevice.Key;
                music.DataCode = file.Path;
                music.MusicType = MusicType.Removable;
                musicList.Add(music);
            }
            return musicList;
        }

        public static async Task GetMusicPropertiesAsync(RemovableDevice removableDevice)
        {
            for (int i = 0; i < removableDevice.Music.Count; i++)
            {
                removableDevice.Music[i].MusicType = MusicType.Removable;
                removableDevice.Music[i] = await MusicManager.GetRemovableMusicPropertiesAsync(removableDevice.Files[i], removableDevice.Music[i]);
                removableDevice.Music[i].Key = removableDevice.Key;
                removableDevice.Music[i].DataCode = removableDevice.Files[i].Path;
            }

        }

        public static StorageFile GetMusicFile(Music music)
        {
            if (music.MusicType != MusicType.Removable)
                return null;
            RemovableDevice removableDevice = Devices.Find(x=>x.Key == music.Key);
            if(removableDevice==null)
                return null;
            StorageFile file = removableDevice.Files.Find(x=>x.Path == music.DataCode);
            return file;
        }

        public static Music GetMusic(Music music)
        {
            if (music.MusicType != MusicType.Removable)
                return null;
            if (String.IsNullOrEmpty(music.Key) == false)
            {
                RemovableDevice removableDevice = Devices.Find(x => x.Key == music.Key);
                if (removableDevice == null)
                {
                    music.Title = "[未知]可移动存储-" + music.DataCode;
                    music.Artist = "请检查文件是否存在";
                    music.Available = false;
                    return music;
                }
                Music music1 = removableDevice.Music.Find(x => x.DataCode == music.DataCode);
                if (music == null)
                    return music;
                else
                    return music1;
            }
            else
            {
                foreach (RemovableDevice Device in Devices)
                {
                    Music music1 = Device.Music.Find(x=>x.DataCode == music.DataCode);
                    if(music1!=null)
                        return music1;
                }
                music.Title = "可移动存储-" + music.DataCode;
                music.Artist = "需要对可移动存储进行扫描";
                music.Available = false;
                return music;
            }

              
            
        }

        public static async Task<Music> GetMusicFromStorageFile(RemovableDevice removableDevice,StorageFile storageFile)
        {
            StorageFile file = removableDevice.Files.Find(x => x.Path == storageFile.Path);
            if (file!=null)
            {
                return removableDevice.Music.Find(x=>x.DataCode == file.Path);
            }
            else
            {
                removableDevice.Files.Add(storageFile);
                Music music = new Music { MusicType = MusicType.Removable,Title = storageFile.Name,Key = removableDevice.Key,DataCode = storageFile.Path};
                music =await MusicManager.GetRemovableMusicPropertiesAsync(storageFile, music);
                removableDevice.Music.Add(music);
                return music;
            }
        }
    }

}

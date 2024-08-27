using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public class FileManagerDisplayItem
    {
        public String Name { get; set; }
        public String type { get; set; }
        public StorageFile file { get; set; }
        public StorageFolder folder { get; set; }
    }

    public class FileManager
    {
        public static async Task<StorageFile> createLocalFolderFileAsync(String Path)
        {
            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile storageFile = await storageFolder.CreateFileAsync(Path);
            return storageFile;
        }

        public static async Task<StorageFile> createFileAsync(String Path,StorageFolder folder)
        {
            StorageFile storageFile = await folder.CreateFileAsync(Path);
            return storageFile;
        }

        public static async Task<StorageFile> getLocalFolderFileAsync(String Path)
        {
            StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile storageFile = await storageFolder.GetFileAsync(Path);
            return storageFile;
        }
    }
}

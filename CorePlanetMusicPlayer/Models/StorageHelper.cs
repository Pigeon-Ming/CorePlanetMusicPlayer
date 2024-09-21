using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public class StorageHelper
    {
        public static async Task WriteFile(StorageFolder storageFolder, string fileName, string content)
        {
            IStorageItem item = (await storageFolder.GetItemsAsync()).ToList().Find(x => x.Name == fileName);
            StorageFile storageFile = null;
            if (item != null)
                if (item is StorageFile)
                    storageFile = item as StorageFile;
            if (storageFile == null)
                storageFile = await storageFolder.CreateFileAsync(fileName);
            await Windows.Storage.FileIO.WriteTextAsync(storageFile, content);
        }

        public static async Task<string> ReadFile(StorageFolder storageFolder, string fileName)
        {
            IStorageItem item = (await storageFolder.GetItemsAsync()).ToList().Find(x => x.Name == fileName);
            StorageFile storageFile = null;
            if (item != null)
                if (item is StorageFile)
                    storageFile = item as StorageFile;
            if (storageFile == null)
                return "";
            return await Windows.Storage.FileIO.ReadTextAsync(storageFile);
        }

        public static async Task<string> ReadFile(StorageFile storageFile)
        {
            if (storageFile == null)
                return "";
            return await Windows.Storage.FileIO.ReadTextAsync(storageFile);
        }

        public static async Task<StorageFolder> GetApplicationDataFolder(string folderName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await IsItemExsitAsync(folder,folderName))
                return await folder.GetFolderAsync(folderName);
            return await folder.CreateFolderAsync(folderName);
        }

        public static async Task<bool> IsItemExsitAsync(StorageFolder parentFolder,string itemName)
        {
            
            IStorageItem storageItem = await parentFolder.TryGetItemAsync(itemName);
            if (storageItem == null)
                return false;
            else
                return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public class StorageManager
    {
        public static readonly String LocalFolderPath = ApplicationData.Current.LocalFolder.Path;

        public static async Task WriteFile(StorageFolder storageFolder, string fileName, string content)
        {
            IStorageItem item = (await storageFolder.GetItemsAsync()).ToList().Find(x => x.Name == fileName);
            StorageFile storageFile = null;
            if (item != null)
                if (item is StorageFile)
                    storageFile = item as StorageFile;
            if (storageFile == null)
                storageFile = await storageFolder.CreateFileAsync(fileName);
            await Windows.Storage.FileIO.WriteTextAsync(storageFile, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);
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
            return await Windows.Storage.FileIO.ReadTextAsync(storageFile, Windows.Storage.Streams.UnicodeEncoding.Utf8);
        }

        public static async Task<string> ReadFile(StorageFile storageFile)
        {
            if (storageFile == null)
                return "";
            return await Windows.Storage.FileIO.ReadTextAsync(storageFile,Windows.Storage.Streams.UnicodeEncoding.Utf8);
        }

        //public static async Task<string> ReadFile(StorageFile storageFile)
        //{
        //    if (storageFile == null)
        //        return "";
        //    return await Windows.Storage.FileIO.ReadTextAsync(storageFile);
        //}

        public static async Task<StorageFolder> GetApplicationDataFolder(string folderName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await IsItemExsitAsync(folder, folderName))
                return await folder.GetFolderAsync(folderName);
            return await folder.CreateFolderAsync(folderName);
        }

        public static async Task<StorageFolder> GetFolder(StorageFolder paretntFolder, string folderName)
        {
            if (await IsItemExsitAsync(paretntFolder, folderName))
                return await paretntFolder.GetFolderAsync(folderName);
            return await paretntFolder.CreateFolderAsync(folderName);
        }

        public static async Task<bool> IsItemExsitAsync(StorageFolder parentFolder, string itemName)
        {

            IStorageItem storageItem = await parentFolder.TryGetItemAsync(itemName);
            if (storageItem == null)
                return false;
            else
                return true;
        }

        public static string RemoveIllegalCharacter(String str)
        {
            return str.Replace("/", "").Replace("\\", "").Replace("*", "").Replace("?", "").Replace(":", "").Replace("|", "").Replace("\"","").Replace("<","").Replace(">","");
        }

        public static bool IsSupportFileType(string FileType)
        {
            if(FileType != ".mp3" && FileType != ".flac" && FileType != ".wma" && FileType != ".m4a" && FileType != ".ac3" && FileType!= ".aac")
                return false;
            return true;
        }

        public static List<string> SupportedMusicFileTypes { get; private set; } = new List<string> {".mp3"/*,".flac",".wma",".m4a",".ac3",".aac"*/};

        public static async Task<List<StorageFolder>> GetRemovableDevicesAsync()
        {
            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
            return (await externalDevices.GetFoldersAsync()).ToList();
        }
    }

}

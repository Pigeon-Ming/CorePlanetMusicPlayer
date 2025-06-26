using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models.Helpers
{
    public class StorageHelper
    {
        public static List<string> SupportedMusicFileTypes = new List<string> { ".mp3",".flac"};

        public static string SupportedMusicFileTypesString = String.Join("",SupportedMusicFileTypes);
        
        public static async Task<StorageFile> CreateFile(StorageFolder storageFolder,string fileName)
        {
            if (!await IsItemExsitAsync(storageFolder, fileName))
            {
                return await storageFolder.CreateFileAsync(fileName);
            }
            return null;
        }

        public static async Task<bool> WriteFileAsync(StorageFile storageFile,string content)
        {
            if (storageFile == null)
                return false;
            try
            {
                await Windows.Storage.FileIO.WriteTextAsync(storageFile, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static async Task<string> ReadFileAsync(StorageFile storageFile)
        {
            if (storageFile == null)
                return null;
            try
            {
                return await Windows.Storage.FileIO.ReadTextAsync(storageFile, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch
            {
                return null;
            }
        }

        public static async Task<StorageFolder> GetStorageFolderFromFutureAccessListAsync(string token)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
        }

        public static async Task<StorageFile> GetStorageFileFromFutureAccessListAsync(string token)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
        }

        public static async Task<StorageFolder> GetApplicationDataFolderAsync(string folderName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await IsItemExsitAsync(folder, folderName))
                return await folder.GetFolderAsync(folderName);
            return await folder.CreateFolderAsync(folderName);
        }

        public static async Task<StorageFile> GetStorageFileFromStorageFolderAsync(StorageFolder storageFolder,string fileName,string contentIfNotExsit)
        {
            if(await IsItemExsitAsync(storageFolder, fileName))
            {
                return await storageFolder.GetFileAsync(fileName);
            }
            else
            {
                return await CreateFile(storageFolder, fileName);
            }
        }

        public static async Task<bool> IsItemExsitAsync(StorageFolder parentFolder, string itemName)
        {

            IStorageItem storageItem = await parentFolder.TryGetItemAsync(itemName);
            if (storageItem == null)
                return false;
            else
                return true;
        }

        public static async Task<List<StorageFolder>> GetRemovableDevicesAsync()
        {
            StorageFolder externalDevices = Windows.Storage.KnownFolders.RemovableDevices;
            return (await externalDevices.GetFoldersAsync()).ToList();
        }
    }
}

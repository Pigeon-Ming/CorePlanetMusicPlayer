using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using CorePlanetMusicPlayer.Models.Helpers;
using Windows.Data.Json;

namespace CorePlanetMusicPlayer6.Models
{
    public class FutureAccessListManager
    {
        public const string FutureAccessListFolderTokensFileName = "FutureAccessListFolderTokens.json";
        public const string FutureAccessListFileTokensFileName = "FutureAccessListFileTokens.json";

        public static async Task<StorageFolder> GetFolderFromTokensAsync(string Token)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(Token);
        }

        public static async Task<StorageFile> GetFileFromTokensAsync(string Token)
        {
            return await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(Token);
        }


        private static async Task<List<string>> ReadTokensFromStorageFileAsync(StorageFile storageFile)//从StorageFile中读取Token
        {
            string fileContent = await StorageHelper.ReadFileAsync(storageFile);
            JsonArray jsonValues;
            if (JsonArray.TryParse(fileContent, out jsonValues))
            {
                List<string> tokens = new List<string>();
                foreach(JsonObject value in jsonValues)
                {
                    tokens.Add(value.GetString());
                }
                return tokens;
            }
            else
                return null;
        }

        public static async Task<List<string>> ReadFolderTokensAsync()//读取文件夹列表的FutureAccessListToken
        {
            StorageFolder storageFolder = await StorageHelper.GetApplicationDataFolderAsync("Cache");
            StorageFile storageFile = await StorageHelper.GetStorageFileFromStorageFolderAsync(storageFolder, FutureAccessListFolderTokensFileName, "{}");
            return await ReadTokensFromStorageFileAsync(storageFile);
        }

        public static async Task<List<string>> ReadFileTokensAsync()//读取文件列表的FutureAccessListToken
        {
            StorageFolder storageFolder = await StorageHelper.GetApplicationDataFolderAsync("Cache");
            StorageFile storageFile = await StorageHelper.GetStorageFileFromStorageFolderAsync(storageFolder, FutureAccessListFolderTokensFileName, "{}");
            return await ReadTokensFromStorageFileAsync(storageFile);
        }


        public static async Task<bool> SaveTokensToStorageFileAsync(List<string>Tokens,StorageFile storageFile)
        {
            if (storageFile == null)
                return false;
            JsonArray jsonValues = new JsonArray();
            foreach (string Token in Tokens)
            {
                jsonValues.Add(JsonValue.CreateStringValue(Token));
            }
            await StorageHelper.WriteFileAsync(storageFile,jsonValues.ToString());
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Data.Json;
using UWPTools.Models;

namespace CorePlanetMusicPlayer.App
{
    public class FutureAccessListManager
    {
        public const string FutureAccessListFolderTokensFileName = "FutureAccessListFolderTokens.json";
        public const string FutureAccessListFileTokensFileName = "FutureAccessListFileTokens.json";


        private static async Task<List<string>> ReadTokensFromStorageFileAsync(StorageFile storageFile)//从StorageFile中读取Token
        {
            string fileContent = await StorageHelper.ReadFileAsStringAsync(storageFile);
            JsonArray jsonValues;
            if (JsonArray.TryParse(fileContent, out jsonValues))
            {
                List<string> tokens = new List<string>();
                foreach(JsonValue value in jsonValues)
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
            StorageFolder storageFolder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile storageFile = await StorageHelper.GetStorageFileFromStorageFolderAsync(storageFolder, FutureAccessListFolderTokensFileName);
            return await ReadTokensFromStorageFileAsync(storageFile);
        }

        public static async Task<List<string>> ReadFileTokensAsync()//读取文件列表的FutureAccessListToken
        {
            StorageFolder storageFolder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile storageFile = await StorageHelper.GetStorageFileFromStorageFolderAsync(storageFolder, FutureAccessListFileTokensFileName);
            return await ReadTokensFromStorageFileAsync(storageFile);
        }

        public static async Task SaveFolderTokensAsync(List<string> tokens)
        {
            StorageFolder storageFolder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile storageFile = await StorageHelper.GetStorageFileFromStorageFolderAsync(storageFolder, FutureAccessListFolderTokensFileName);
            await SaveTokensToStorageFileAsync(tokens, storageFile);
        }

        public static async Task SaveFileTokensAsync(List<string> tokens)
        {
            StorageFolder storageFolder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            StorageFile storageFile = await StorageHelper.GetStorageFileFromStorageFolderAsync(storageFolder, FutureAccessListFileTokensFileName);
            await SaveTokensToStorageFileAsync(tokens, storageFile);
        }


        private static async Task<bool> SaveTokensToStorageFileAsync(List<string>Tokens,StorageFile storageFile)
        {
            if (storageFile == null)
                return false;
            JsonArray jsonValues = new JsonArray();
            foreach (string Token in Tokens)
            {
                jsonValues.Add(JsonValue.CreateStringValue(Token));
            }
            await StorageHelper.WriteStringToFileAsync(storageFile,jsonValues.ToString());
            return true;
        }
    }
}

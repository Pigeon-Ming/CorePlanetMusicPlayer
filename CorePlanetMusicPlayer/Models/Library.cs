using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Ape;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace CorePlanetMusicPlayer.Models
{
    public class Library
    {
        public static EventList<Music> LocalLibraryMusic = new EventList<Music>();
    }

    public class LibraryManager
    {
        static Queue<StorageFolder> foreachFolderQueue = new Queue<StorageFolder>();
        public static int gotPropertyCount { get; set; }

        public static async Task ReloadLibraryAsync(bool GetCover)
        {
            gotPropertyCount = 0;
            Library.LocalLibraryMusic.Clear();
            ArtistManager.Artists.Clear();
            AlbumManager.Albums.Clear();

            StorageFolder folder = KnownFolders.MusicLibrary;
            IReadOnlyList<IStorageItem> itemsList = await folder.GetItemsAsync();
            ForeachLibrary(itemsList);
            while (foreachFolderQueue.Count != 0)
            {
                itemsList = await foreachFolderQueue.Dequeue().GetItemsAsync();
                ForeachLibrary(itemsList);
            }

            GetAllMusicInfo(GetCover);
        }//重新载入音乐库

        public static async void GetAllMusicInfo(bool GetCover)
        {
            Debug.WriteLine("遍历文件获取音乐信息");

            if (GetCover)
                for (int i = 0; i < Library.LocalLibraryMusic.Count; i++)
                {
                    await MusicManager.GetMusicPropertiesAsync(Library.LocalLibraryMusic[i]);
                    await MusicManager.GetMusicCoverAsync_Taglib(Library.LocalLibraryMusic[i]);
                }
            else
                for (int i = 0; i < Library.LocalLibraryMusic.Count; i++)
                {
                    await MusicManager.GetMusicPropertiesAsync(Library.LocalLibraryMusic[i]);
                }
        }

        public static async Task ReloadLibraryAsync_GetPropertiesFromJson(bool GetCover)
        {
            Debug.WriteLine("使用Json获取音乐信息");
            gotPropertyCount = 0;
            Library.LocalLibraryMusic.Clear();
            ArtistManager.Artists.Clear();
            AlbumManager.Albums.Clear();
            StorageFolder folder = KnownFolders.MusicLibrary;
            IReadOnlyList<IStorageItem> itemsList = await folder.GetItemsAsync();
            ForeachLibrary(itemsList);
            while (foreachFolderQueue.Count != 0)
            {
                itemsList = await foreachFolderQueue.Dequeue().GetItemsAsync();
                ForeachLibrary(itemsList);
            }
            

            await MusicManager.ReadMusicPropertiesFromJson(GetCover);
            MusicManager.SetMusicPropertiesToJson();
        }

        //private static void ForeachLibraryByToken()
        //{
        //    for(int i)
        //}


        private static void ForeachLibrary(IReadOnlyList<IStorageItem> itemsList)
        {
            foreach (var item in itemsList)
            {
                if (item is StorageFolder)
                {
                    foreachFolderQueue.Enqueue((StorageFolder)item);
                }
                else
                {
                    
                    string fileName = item.Name;
                    //Debug.WriteLine(fileName+"|||"+fileName.Substring(fileName.LastIndexOf(".")));
                    string fileSuffix = fileName.Substring(fileName.LastIndexOf("."));
                    if (fileSuffix == ".mp3" || fileSuffix == ".flac" || fileSuffix == ".wma" || fileSuffix == ".m4a" || fileSuffix == ".ac3" || fileSuffix == ".aac")
                    {
                        Music music = new Music { Title = fileName, Artist = "未知艺术家", Album = "未知专辑", Bitrate = 0, Year = 0, Duration = "", file = (StorageFile)item};
                        
                        Library.LocalLibraryMusic.Add(music);
                    }
                }
            }
        }//遍历文件夹

        public static async Task<bool> AddFoldersAsync()
        {
            var myMusics = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            if (await myMusics.RequestAddFolderAsync() != null) return true;
            else return false;
        }

        public static async Task<List<StorageFolder>> GetFoldersAsync()
        {
            var myMusics = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            return myMusics.Folders.ToList();
        }

        public static async Task RemoveFolder(StorageFolder storageFolder)
        {
            var myMusics = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            await myMusics.RequestRemoveFolderAsync(storageFolder);
        }
    }
}

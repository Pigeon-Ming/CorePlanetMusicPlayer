using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public class Library
    {
        public static EventList<Music> LocalLibraryMusic = new EventList<Music>();
    }

    public class LibraryManager
    {
        static Queue<StorageFolder> foreachFolderQueue = new Queue<StorageFolder>();
        public static int gotPropertyCount = 0;

        public static async Task ReloadLibraryAsync()
        {
            Library.LocalLibraryMusic.Clear();

            StorageFolder folder = KnownFolders.MusicLibrary;
            IReadOnlyList<IStorageItem> itemsList = await folder.GetItemsAsync();
            ForeachLibrary(itemsList);
            while (foreachFolderQueue.Count != 0)
            {
                itemsList = await foreachFolderQueue.Dequeue().GetItemsAsync();
                ForeachLibrary(itemsList);
            }
            for (int i = 0; i < Library.LocalLibraryMusic.Count; i++)
                MusicManager.GetMusicPropertiesAsync(Library.LocalLibraryMusic[i]);
            
        }//重新载入音乐库

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
                        Music music = new Music { Title = fileName, Artist = "未知艺术家", Album = "未知专辑", Bitrate = 0, Year = 0, Duration = "", file = (StorageFile)item,source= MediaSource.CreateFromStorageFile((StorageFile)item) };
                        Library.LocalLibraryMusic.Add(music);
                    }
                }
            }
        }//遍历文件夹
    }
}

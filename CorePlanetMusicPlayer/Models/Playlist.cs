using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace CorePlanetMusicPlayer.Models
{
    public class Playlist
    {
        public String Name {  get; set; }
        public List<Music> includeMusic { get; set; }

        public String Description {  get; set; }
    }

    public class JsonPlaylist
    {
        public String Name { get; set; }
        public List<String> includeMusic { get; set; }
        public String Description { get; set; }
    }

    public class PlaylistManager
    {
        public static async Task CreatePlaylistAsync(String PlaylistName)
        {
            StorageFolder folder = (StorageFolder)await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync("playlists");
            if (folder==null)
            {
                folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("playlists");
            }
            StorageFile file = await FileManager.createFileAsync(PlaylistName+".pmplist4",folder);
            await Windows.Storage.FileIO.WriteTextAsync(file, JsonSerializer.Serialize(new JsonPlaylist { Name=PlaylistName,includeMusic=new List<String>()}));
        }

        public static async Task<List<Playlist>> GetPlaylistsListAsync()
        {
            List<Playlist>playlists = new List<Playlist>();
            StorageFolder folder = (StorageFolder)await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync("playlists");
            if (folder==null)
            {
                return new List<Playlist>();
            }
            IReadOnlyList<IStorageItem> items = await folder.GetItemsAsync();
            foreach (IStorageItem item in items)
            {
                Playlist playlist = new Playlist();
                if(item is StorageFile)
                {
                    string name = ((StorageFile)item).Name;
                    name = name.Substring(0, name.LastIndexOf("."));
                    playlist = await ReadPlaylistAsync(name);
                    playlists.Add(playlist);
                }
            }
            return playlists;
        }

        public static async Task<Playlist> ReadPlaylistAsync(String listName)
        {
            Playlist playlist = new Playlist();
            playlist.Name = listName;
            StorageFolder folder = (StorageFolder)await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync("playlists");
            if (folder == null)
            {
                return playlist;
            }
            StorageFile file = await folder.GetFileAsync(listName + ".pmplist4");
            string filecontent = await Windows.Storage.FileIO.ReadTextAsync(file);
            JsonPlaylist jsonPlayList = JsonSerializer.Deserialize<JsonPlaylist>(filecontent);
            playlist.includeMusic = new List<Music>();
            for (int i = 0; i < jsonPlayList.includeMusic.Count; i++)
            {
                playlist.includeMusic.Add(MusicManager.FindMusicByFileName(jsonPlayList.includeMusic[i]));
            }
            if (!String.IsNullOrEmpty(jsonPlayList.Description))
            {
                playlist.Description = jsonPlayList.Description;
            }
            else
            {
                playlist.Description = "此播放列表没有添加描述。";
            }
            
            
            return  playlist;
        }

        public static async Task SetPlayList(Playlist playlist)
        {
            List<String> list = new List<String>();
            for(int i = 0; i < playlist.includeMusic.Count; i++)
            {
                list.Add(playlist.includeMusic[i].file.Name);
            }
            StorageFolder folder = (StorageFolder)await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync("playlists");
            if (folder == null)
            {
                return;
            }
            StorageFile file = await folder.GetFileAsync(playlist.Name + ".pmplist4");
            await Windows.Storage.FileIO.WriteTextAsync(file, JsonSerializer.Serialize(new JsonPlaylist { Name = playlist.Name, includeMusic = list ,Description = playlist.Description}));
        }

        public static async Task RenamePlaylistAsync(String oldName,String newName)
        {
            StorageFolder folder = (StorageFolder)await Windows.Storage.ApplicationData.Current.LocalFolder.TryGetItemAsync("playlists");
            if (folder == null)
            {
                return;
            }
            StorageFile file = await folder.GetFileAsync(oldName + ".pmplist4");
            await file.RenameAsync(newName + ".pmplist4");
        }
    }

}

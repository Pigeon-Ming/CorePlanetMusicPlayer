using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public class Playlist
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<Music> Music { get; set; } = new List<Music>();
    }

    public class PlaylistManager
    {
        public static async Task<Playlist> ReadPlaylistFromStorageFileAsync(StorageFile storageFile)
        {
            string fileStr = await StorageHelper.ReadFile(storageFile);
            if (String.IsNullOrEmpty(fileStr)) return new Playlist();
            JsonObject jsonObject;
            if(JsonObject.TryParse(fileStr,out jsonObject)==false)return new Playlist();
            Playlist playlist = new Playlist();
            playlist.Name = jsonObject.GetNamedString("name");
            playlist.Description = jsonObject.GetNamedString("description");
            JsonArray array = jsonObject.GetNamedArray("music");
            for(int i = 0; i < array.Count; i++)
            {
                playlist.Music.Add(MusicManager.GetMusicFromJsonObject(array[i].GetObject()));
            }
            return playlist;
        }

        public static async Task SavePlaylistAsync(Playlist playlist)
        {
            if(Library.PlayLists.Find(x=>x.Name == playlist.Name)==null)
                Library.PlayLists.Add(playlist);
            StorageFolder folder = await StorageHelper.GetApplicationDataFolder("Playlists");
            JsonObject jsonObject = new JsonObject();
            jsonObject.Add("name",JsonValue.CreateStringValue(playlist.Name));
            jsonObject.Add("description",JsonValue.CreateStringValue(playlist.Description));
            JsonArray jsonArray = new JsonArray();
            for(int i = 0; i < playlist.Music.Count;i++)
            {
                jsonArray.Add(JsonHelper.MusicToJsonObject(playlist.Music[i]));
            }
            jsonObject.Add("music", jsonArray);
            string content = jsonObject.ToString();
            //Debug.WriteLine(content);
            await StorageHelper.WriteFile(folder,playlist.Name+".pmplist5",content);
        }
    }
}

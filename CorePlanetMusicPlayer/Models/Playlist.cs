using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public static event EventHandler PlaylistsDataChanged;
        public static async Task<Playlist> ReadPlaylistFromStorageFileAsync(StorageFile storageFile)
        {
            string fileStr = await StorageManager.ReadFile(storageFile);
            if (String.IsNullOrEmpty(fileStr)) return new Playlist();
            JsonObject jsonObject;
            if (JsonObject.TryParse(fileStr, out jsonObject) == false) return new Playlist();
            Playlist playlist = new Playlist();
            playlist.Name = jsonObject.GetNamedString("name");
            playlist.Description = jsonObject.GetNamedString("description");
            JsonArray array = jsonObject.GetNamedArray("music");
            for (int i = 0; i < array.Count; i++)
            {
                playlist.Music.Add(JsonHelper.JsonObjectToMusic(array[i].GetObject()));
            }
            return playlist;
        }

        public static async Task SavePlaylistAsync(Playlist playlist)
        {
            if (Library.Playlists.Find(x => x.Name == playlist.Name) == null)
            {
                Library.Playlists.Add(playlist);
                PlaylistsDataChanged.Invoke(null,null);
            }
            StorageFolder folder = await StorageManager.GetApplicationDataFolder("Playlists");
            JsonObject jsonObject = new JsonObject();
            jsonObject.Add("name", JsonValue.CreateStringValue(playlist.Name));
            jsonObject.Add("description", JsonValue.CreateStringValue(playlist.Description));
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < playlist.Music.Count; i++)
            {
                jsonArray.Add(JsonHelper.MusicToJsonObject(playlist.Music[i]));
            }
            jsonObject.Add("music", jsonArray);
            string content = jsonObject.ToString();
            //Debug.WriteLine(content);
            await StorageManager.WriteFile(folder, playlist.Name + ".pmplist5", content);
        }
        
        public static async Task DeletePlaylistAsync(string PlaylistName)
        {
            StorageFolder storageFolder = await StorageManager.GetApplicationDataFolder("Playlists");
            if(await StorageManager.IsItemExsitAsync(storageFolder, PlaylistName+".pmplist5"))
            {
                await (await storageFolder.GetFileAsync(PlaylistName + ".pmplist5")).DeleteAsync();
                Playlist playlist = Library.Playlists.Find(x=>x.Name == PlaylistName);
                if(playlist!=null)
                    Library.Playlists.Remove(playlist);
                PlaylistsDataChanged.Invoke(null, null);
            }
        }


        public static async Task LoadPlaylistFromFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".pmplist4");
            picker.FileTypeFilter.Add(".pmplist5");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                if(file.FileType == ".pmplist4")
                {
                    string str = pmplist4StrTopmplist5Str(await StorageManager.ReadFile(file));
                    if(String.IsNullOrEmpty(str))
                        return;
                    await StorageManager.WriteFile(await StorageManager.GetApplicationDataFolder("Playlists"), file.DisplayName + ".pmplist5", str);
                    await LibraryManager.RefreshPlaylistsData();
                    PlaylistsDataChanged.Invoke(null, null);
                }
                else if (file.FileType == ".pmplist5")
                {
                    StorageFolder folder = await StorageManager.GetApplicationDataFolder("Playlists");
                    if (await folder.TryGetItemAsync(file.Name) == null)
                        await file.CopyAsync(folder);
                    else
                        return;
                    await LibraryManager.RefreshPlaylistsData();
                    PlaylistsDataChanged.Invoke(null, null);
                }
            }
        }

        public static async Task ExportPlaylistAsync(string PlaylistName)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("播放列表-PlanetMusicPlayer", new List<string>() { ".pmplist5" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = PlaylistName;
            String str = await StorageManager.ReadFile(await StorageManager.GetApplicationDataFolder("Playlists"),PlaylistName+".pmplist5");
            StorageFile file1 = await savePicker.PickSaveFileAsync();
            await Windows.Storage.FileIO.WriteTextAsync(file1, str);
            
        }

        public static string pmplist4StrTopmplist5Str(String pmplist4Str)
        {
            if (String.IsNullOrEmpty(pmplist4Str))
                return "";
            JsonObject pmplist4JsonObject = new JsonObject();
            if (JsonObject.TryParse(pmplist4Str, out pmplist4JsonObject) == false)
                return "";
            string playlistName = pmplist4JsonObject.GetNamedString("Name");
            string description = "";
            IJsonValue jsonValue;
            if(pmplist4JsonObject.TryGetValue("Description",out jsonValue))
            {
                if(jsonValue.ValueType == JsonValueType.String)
                {
                    description = jsonValue.GetString();
                }
            }
            JsonArray pmplist4jsonArray = pmplist4JsonObject.GetNamedArray("includeMusic");
            List<Music>musicList = new List<Music>();
            for(int i=0;i<pmplist4jsonArray.Count;i++)
            {
                string MusicName = Regex.Unescape(pmplist4jsonArray[i].GetString());
                Music music = Library.Music.Find(x=>x.DataCode.IndexOf(MusicName)!=-1);
                if (music != null)
                    musicList.Add(music);
            }

            JsonObject jsonObject = new JsonObject();
            jsonObject.Add("name", JsonValue.CreateStringValue(playlistName));
            jsonObject.Add("description", JsonValue.CreateStringValue(description));
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < musicList.Count; i++)
            {
                jsonArray.Add(JsonHelper.MusicToJsonObject(musicList[i]));
            }
            jsonObject.Add("music", jsonArray);
            return jsonObject.ToString();
        }
    }
}

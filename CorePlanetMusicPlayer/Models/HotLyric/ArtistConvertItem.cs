using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.AtomPub;

namespace CorePlanetMusicPlayer.Models.HotLyric
{
    public class ArtistConvertItem
    {
        public String Name {  get; set; }
        public String ConvertTo { get; set; }
    }

    public class ArtistConvertItemManager
    {
        public static List<ArtistConvertItem> ConvertItems { get; set; } = new List<ArtistConvertItem> ();

        public static async Task WriteItemsToJsonAsync()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = (StorageFile)await folder.TryGetItemAsync("HotLyricArtistConverts.json");
            if (file == null)
                file = await folder.CreateFileAsync("HotLyricArtistConverts.json");
            await Windows.Storage.FileIO.WriteTextAsync(file, JsonSerializer.Serialize<List<ArtistConvertItem>>(ConvertItems));
        }

        public static async Task<bool> ReadItemsFromJsonAsync()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = (StorageFile)await folder.TryGetItemAsync("HotLyricArtistConverts.json");
            if (file == null)
                return false;
            string filecontent = await Windows.Storage.FileIO.ReadTextAsync(file);

            if (String.IsNullOrEmpty(filecontent)) return false;
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.IncludeFields = false;
            try
            {
                ConvertItems = JsonSerializer.Deserialize<List<ArtistConvertItem>>(filecontent, jsonSerializerOptions);
            }
            catch (Exception ex)
            {

            }
            return true;
        }
    }
}

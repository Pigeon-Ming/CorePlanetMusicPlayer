using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace CorePlanetMusicPlayer.Models.OnlineMessages
{
    public class OnlineMusicManager
    {
        public static async Task<JsonArray> GetMusicJsonArrayAsync(String Title)
        {
            String MusicListJson = await ResponseManager.PostString("http://music.163.com/api/search/pc", new Dictionary<string, string> { ["s"] = Title, ["type"] = "1" });
            Debug.WriteLine(MusicListJson);

            JsonObject jsonObject = JsonObject.Parse(MusicListJson);
            jsonObject = jsonObject["result"].GetObject();
            JsonArray jsonArray = jsonObject["songs"].GetArray();
            return jsonArray;
        }
    }
}

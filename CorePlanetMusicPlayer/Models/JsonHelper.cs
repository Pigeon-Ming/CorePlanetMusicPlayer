using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace CorePlanetMusicPlayer.Models
{
    public class JsonHelper
    {
        public static JsonObject MusicToJsonObject(Music music)
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject.Add("type", JsonValue.CreateNumberValue((int)music.MusicType));
            jsonObject.Add("dataCode", JsonValue.CreateStringValue(music.DataCode));
            return jsonObject;
        }

        public static JsonArray StringToJsonArray(string str)
        {
            JsonArray array = new JsonArray();
            if (JsonArray.TryParse(str, out array))
            {
                return array;
            }
            else
            {
                return null;
            }
        }

        public static List<Music> JsonArrayToMusic(JsonArray jsonArray)
        {
            List<Music> musicList = new List<Music>();
            for (int i = 0; i < jsonArray.Count; i++)
            {
                musicList.Add(JsonObjectToMusic(jsonArray.GetObjectAt((uint)i)));
            }
            return musicList;
        }

        public static Music JsonObjectToMusic(JsonObject jsonObject)
        {
            Music music = new Music();
            music.MusicType = (MusicType)jsonObject.GetNamedNumber("type");
            music.DataCode = jsonObject.GetNamedString("dataCode");
            return music;
        }
    }
}

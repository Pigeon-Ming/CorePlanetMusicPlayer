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
            jsonObject.Add("title",JsonValue.CreateStringValue(music.Title));
            jsonObject.Add("artist",JsonValue.CreateStringValue(music.Artist));
            jsonObject.Add("album",JsonValue.CreateStringValue(music.Album));
            jsonObject.Add("bitrate",JsonValue.CreateNumberValue(music.Bitrate));
            jsonObject.Add("year",JsonValue.CreateNumberValue(music.Year));
            jsonObject.Add("trackNumber",JsonValue.CreateNumberValue(music.TrackNumber));
            jsonObject.Add("discNumber",JsonValue.CreateNumberValue(music.DiscNumber));
            jsonObject.Add("dataCode",JsonValue.CreateStringValue(music.DataCode));
            jsonObject.Add("duration",JsonValue.CreateStringValue(music.Duration));
            jsonObject.Add("musicType",JsonValue.CreateStringValue(music.MusicType.ToString()));
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
            List<Music>musicList = new List<Music>();
            for (int i = 0; i < jsonArray.Count; i++)
            {
                musicList.Add(JsonObjectToMusic(jsonArray.GetObjectAt((uint)i)));
            }
            return musicList;
        }

        public static Music JsonObjectToMusic(JsonObject jsonObject)
        {
            Music music = new Music();
            music.Title = jsonObject.GetNamedString("title");
            music.Artist = jsonObject.GetNamedString("artist");
            music.Album = jsonObject.GetNamedString("album");
            music.Bitrate = (uint)jsonObject.GetNamedNumber("bitrate");
            music.Year = (uint)jsonObject.GetNamedNumber("year");
            music.TrackNumber = (uint)jsonObject.GetNamedNumber("trackNumber");
            music.DiscNumber = (uint)jsonObject.GetNamedNumber("discNumber");
            music.Duration = jsonObject.GetNamedString("duration");
            return music;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;

namespace CorePlanetMusicPlayer.Models.OnlineMessages
{
    public class OnlineLyric
    {
        public String Title { get; set; }
        public String Artist {  get; set; }
        public String LyricsString { get; set; }
        public class OnlineLyricManager
        {
            public static async Task<List<OnlineLyric>> SearchLyricAsync(String MusicTitle, String ArtistName)
            {
                List<OnlineLyric> lyricsList = new List<OnlineLyric>();
                JsonArray jsonArray = await OnlineMusicManager.GetMusicJsonArrayAsync(MusicTitle);

                if (jsonArray.Count <= 0) return new List<OnlineLyric>();

                for (int i = 0; i < jsonArray.Count; i++)
                {
                    JsonObject obj = jsonArray[i].GetObject();
                    String JTitle = obj["name"].GetString();
                    String JArtist = "";
                    JsonArray array = obj["artists"].GetArray();
                    for (int j = 0; j < array.Count; j++)
                    {
                        JsonObject artistObj = array[j].GetObject();
                        JArtist += artistObj["name"].GetString();
                    }
                    if (!String.IsNullOrEmpty(ArtistName))
                        if (JArtist.IndexOf(ArtistName) == -1)
                            break;

                    String Album = "";
                    JsonObject albumObj = obj["album"].GetObject();
                    Album = albumObj["name"].GetString();
                    
                    lyricsList.Add(new OnlineLyric { Title = JTitle, Artist = JArtist, LyricsString = await GetLyricByMusicID(Convert.ToInt32(obj["id"].GetNumber()))});
                }

                return lyricsList;
            }
            public static async Task<String> GetLyricByMusicID(int ID)
            {
                String LyricJsonString = await ResponseManager.GetString("http://music.163.com/api/song/media?id=" + ID);
                JsonObject LyricJson = JsonObject.Parse(LyricJsonString);
                if (LyricJson.Keys.ToList().IndexOf("lyric") > -1)
                {
                    return LyricJson["lyric"].GetString();
                }
                else
                {
                    return "";
                }
            }
        }
    }
}

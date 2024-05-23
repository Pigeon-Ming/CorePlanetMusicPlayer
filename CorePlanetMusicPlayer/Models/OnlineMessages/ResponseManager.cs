using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models.OnlineMessages
{
    public class ResponseManager
    {
        private static readonly HttpClient client = new HttpClient();
        public static async Task<String> GetString(String URL)
        {
            return await client.GetStringAsync(URL);
        }

        public static async Task<String> PostString(String URL, Dictionary<String, String> values)
        {
            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.37.0");
            client.DefaultRequestHeaders.Add("Cookie", "NMTID=00OsHfy0b89h1MpNkxepN8TyGhEPX4AAAGPcT0GmA");
            var response = await client.PostAsync(URL, content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}

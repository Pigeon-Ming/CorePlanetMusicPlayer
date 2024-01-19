using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetMusicPlayer.Models
{
    public class Lyric
    {
        public String Content { get; set; }
        public String Time {  get; set; }
    }

    public class LyricManager
    {
        public static List<Lyric>CurrentLyrics = new List<Lyric>();

        public static List<Lyric> ProcessLyrics(String TextContent)
        {
            List<Lyric> lyrics = new List<Lyric>();

            int TextContent_DoseBracket_Index = 0;
            int TextContent_LineFeed_Index = 0;

            while (!String.IsNullOrEmpty(TextContent))
            {
                TextContent_DoseBracket_Index = TextContent.IndexOf("]");
                if (TextContent_DoseBracket_Index == -1) break;
                Lyric lyric = new Lyric();
                lyric.Time = TextContent.Substring(1,TextContent_DoseBracket_Index-1);
                if(TextContent.IndexOf("\r") - TextContent.IndexOf("]") - 1 <= 0)
                    lyric.Content = "";
                else
                    lyric.Content = TextContent.Substring(TextContent_DoseBracket_Index + 1, TextContent.IndexOf("\r") - TextContent.IndexOf("]") - 1);
                lyrics.Add(lyric);
                TextContent_LineFeed_Index = TextContent.IndexOf("\r");
                
                Debug.WriteLine(TextContent_DoseBracket_Index + "|" + TextContent_LineFeed_Index + "\n" + TextContent);
                if (TextContent_LineFeed_Index == -1&& TextContent_LineFeed_Index + 1<TextContent.Length-1) break;
                //Debug.WriteLine(TextContent_DoseBracket_Index+"|"+TextContent_LineFeed_Index+"\n"+TextContent);
                TextContent = TextContent.Substring(TextContent_LineFeed_Index+1);
            }
            return lyrics;
        }

        public static async Task<List<Lyric>> LoadFromLRCFileAndProcessAsync()
        {
            Windows.Storage.StorageFolder storageFolder =
                 Windows.Storage.ApplicationData.Current.LocalFolder;

            Windows.Storage.StorageFile file1;
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".lrc");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            if (string.IsNullOrEmpty(file.Path))
            {
                return new List<Lyric>();
            }

            String fileContent = await Windows.Storage.FileIO.ReadTextAsync(file);
            
            return ProcessLyrics(fileContent); 
        }

        public static int GetCurrentLyricIndex(List<Lyric>lyrics)
        {
            //Debug.WriteLine(lyrics[0].Time);
            int currentLyricIndex = -1;
            if (lyrics.Count > 0)
            {

                for (int i = 0; i < lyrics.Count; i++)
                {
                    Debug.WriteLine("|"+lyrics[i].Time.Substring(1, 5) + "|" + PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Substring(3, 5));
                    if (lyrics[i].Time.Substring(1, 5) == PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Substring(3, 5) /*&& CurrentLyricIndex != i*/)
                    {
                        //Debug.WriteLine(lyrics[i].Time.Substring(6, 1) + "|" + PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Substring(9, 1));
                        if (PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Length < 10)
                        {

                            currentLyricIndex = i;
                            continue;
                        }
                        if (Convert.ToInt32(lyrics[i].Time.Substring(7, 1)) >= Convert.ToInt32(PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Substring(9, 1)) /*&& isanimationover == true*/)
                        {
                            currentLyricIndex = i;

                        }
                    }
                }
            }
            return currentLyricIndex;
        }
    }
}

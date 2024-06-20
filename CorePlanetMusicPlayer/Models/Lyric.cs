using CorePlanetMusicPlayer.Models.Taglib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.UI.Xaml;

namespace CorePlanetMusicPlayer.Models
{
    public class Lyric
    {
        public String Content { get; set; }
        public String Time {  get; set; }
    }

    public class LyricManager
    {
        public static List<Lyric>CurrentLyrics = new List<Lyric>();
        public static String CurrentLyric = "";
        public static DispatcherTimer LyricServiceTimer = new DispatcherTimer();
        public static Music CurrentLyricMusic {  get; set; }

        public static int CurrentLyricIndex { get; set; } = -1;

        public static void StartLyricService()
        {
            LyricServiceTimer.Tick += LyricServiceTimer_Tick;
            LyricServiceTimer.Start();
        }

        private static void LyricServiceTimer_Tick(object sender, object e)
        {
            if (CurrentLyricMusic != PlayCore.CurrentMusic)
            {
                CurrentLyricMusic = PlayCore.CurrentMusic;
                TryAutoLoadLyricAsync();
            }

            CurrentLyricIndex = LyricManager.GetCurrentLyricIndex();
            if (CurrentLyricIndex < CurrentLyrics.Count && CurrentLyricIndex > -1)
                CurrentLyric = CurrentLyrics[CurrentLyricIndex].Content;
            else
                CurrentLyric = "没有找到歌词";
        }

        public static async Task TryAutoLoadLyricAsync()
        {
            LyricManager.CurrentLyrics = await LyricManager.AutoLoadFromLRCFileAndProcessAsync();
            if (LyricManager.CurrentLyrics.Count == 0)
                LyricManager.CurrentLyrics = LyricManager.LoadFromMusicFile(PlayCore.CurrentMusic);

        }

        public static List<Lyric> ProcessLyrics(String TextContent)
        {
            List<Lyric> lyrics = new List<Lyric>();

            int TextContent_DoseBracket_Index = 0;
            int TextContent_LineFeed_Index = 0;

            if (String.IsNullOrEmpty(TextContent)) return new List<Lyric> { };
            TextContent = TextContent.Replace("\n", "\r");
            TextContent = TextContent.Replace("\r\r", "\r");
            


            while (!String.IsNullOrEmpty(TextContent))
            {
                TextContent_DoseBracket_Index = TextContent.IndexOf("]");
                if (TextContent_DoseBracket_Index == -1) break;
                Lyric lyric = new Lyric();
                //String temp = TextContent.Substring(1, TextContent_DoseBracket_Index - 1);
                //if (Regex.IsMatch(temp, @"^([0-5]?:[0-5]?\d.[0-9]?\d)$"))
                //    
                //else
                //{
                //    TextContent_LineFeed_Index = TextContent.IndexOf("\r");
                //    TextContent = TextContent.Substring(TextContent_LineFeed_Index + 1);
                //    continue;
                //}

                TextContent_LineFeed_Index = TextContent.IndexOf("\r");
                if (TextContent.IndexOf("[0") == -1 || TextContent.IndexOf("[0") > 0)
                {
                    TextContent = TextContent.Substring(TextContent_LineFeed_Index + 1);
                    continue;
                }
                lyric.Time = TextContent.Substring(1, TextContent_DoseBracket_Index - 1);
                

                if (TextContent.IndexOf("\r") - TextContent.IndexOf("]") - 1 <= 0)
                    lyric.Content = "";
                else
                {
                    lyric.Content = TextContent.Substring(TextContent_DoseBracket_Index + 1, TextContent.IndexOf("\r") - TextContent.IndexOf("]") - 1);
                    lyric.Content = lyric.Content.Replace("「", "\r");
                    lyric.Content = lyric.Content.Replace("」", "");
                }

                
                lyrics.Add(lyric);


                //Debug.WriteLine(TextContent_DoseBracket_Index + "|" + TextContent_LineFeed_Index + "\n" + TextContent);
                if (TextContent_LineFeed_Index == -1 && TextContent_LineFeed_Index + 1 < TextContent.Length - 1)
                {
                    TextContent_DoseBracket_Index = TextContent.IndexOf("]");
                    if (TextContent_DoseBracket_Index == -1) break;
                    lyric = new Lyric();
                    lyric.Time = TextContent.Substring(1, TextContent_DoseBracket_Index - 1);
                    if (TextContent.Length - TextContent.IndexOf("]") - 1 <= 0)
                        lyric.Content = "";
                    else
                    {
                        lyric.Content = TextContent.Substring(TextContent_DoseBracket_Index + 1, TextContent.Length - TextContent.IndexOf("]") - 1);
                        lyric.Content = lyric.Content.Replace("「", "\r");
                        lyric.Content = lyric.Content.Replace("」", "");
                    }
                    if (lyrics.Count > 1 && lyrics[lyrics.Count - 1].Time == lyric.Time)
                    {
                        lyrics[lyrics.Count - 1].Content = lyrics[lyrics.Count - 1].Content + "\n" + lyric.Content;
                    }
                    else
                    {
                        lyrics.Add(lyric);
                    }
                    break;
                }

                //Debug.WriteLine(TextContent_DoseBracket_Index+"|"+TextContent_LineFeed_Index+"\n"+TextContent);
                TextContent = TextContent.Substring(TextContent_LineFeed_Index+1);
            }
            CurrentLyrics = lyrics;

            for(int i = lyrics.Count - 1; i >= 0; i--)
            {
                for(int j = 0;j< i; j++)
                {
                    if (lyrics[i].Time == lyrics[j].Time)
                    {
                        lyrics[j].Content += "\n"+lyrics[i].Content;
                        lyrics.Remove(lyrics[i]);
                        break;
                    }
                }
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

            if (file == null || string.IsNullOrEmpty(file.Path))
            {
                return new List<Lyric>();
            }

            String fileContent = await Windows.Storage.FileIO.ReadTextAsync(file);
            
            return ProcessLyrics(fileContent); 
        }

        private static async Task<bool> isFilePresentAsync(string fileName)
        {
            var item = await KnownFolders.MusicLibrary.TryGetItemAsync(fileName);
            return item != null;
        }

        public static async Task<List<Lyric>> AutoLoadFromLRCFileAndProcessAsync()
        {
            String LRCFileName = PlayCore.CurrentMusic.file.Name.Substring(0, PlayCore.CurrentMusic.file.Name.LastIndexOf(".")) + ".lrc";
            if (!await isFilePresentAsync(LRCFileName))
            {
                 return new List<Lyric>(); 
            }

            Windows.Storage.StorageFile file = await KnownFolders.MusicLibrary.GetFileAsync(LRCFileName);

            if (string.IsNullOrEmpty(file.Path))
            {
                return new List<Lyric>();
            }

            String fileContent = await Windows.Storage.FileIO.ReadTextAsync(file);

            return ProcessLyrics(fileContent);
        }

        public static List<Lyric> LoadFromMusicFile(Music music)
        {
            if (PlayCore.CurrentMusic == null) return new List<Lyric>();
            if (music.file.FileType == ".ac3" || music.file.FileType == ".m4a") return new List<Lyric>();
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(music.file);
            File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
            TagLib.File file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
            string lyricContent = file.Tag.Lyrics;
            //string lyricContent = file.Tag.Description;
            return ProcessLyrics(lyricContent);
        }

        public static int GetCurrentLyricIndex()
        {
            if (CurrentLyrics.Count > 0)
            {

                for (int i = 0; i < CurrentLyrics.Count; i++)
                {
                    //Debug.WriteLine("|"+CurrentLyrics[i].Time.Substring(0, 5) + "|" + PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Substring(3, 5));
                    if (CurrentLyrics[i].Time.Substring(0, 5) == PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Substring(3, 5) /*&& CurrentLyricIndex != i*/)
                    {
                        CurrentLyricIndex = i;
                        if (Convert.ToInt32(CurrentLyrics[i].Time.Substring(6, 1)) >= Convert.ToInt32(PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Substring(6, 1)))
                        {
                            CurrentLyricIndex = i;
                        }
                        break;
                    }
                }
            }
            if(CurrentLyricIndex!=-1 && CurrentLyricIndex< CurrentLyrics.Count)
            CurrentLyric = CurrentLyrics[CurrentLyricIndex].Content;
            return CurrentLyricIndex;
        }

        
    }
}

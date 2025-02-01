using CorePlanetMusicPlayer.Models.TagLibHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;

namespace CorePlanetMusicPlayer.Models
{
    public class Lyric
    {
        public string Time { get; set; } = "";
        public string Content { get; set; } = "";
        public string Translation { get; set; } = "";
    }

    public class LyricManager
    {
        public static List<Lyric> CurrentLyrics { get; set; } = new List<Lyric>();
        public static int CurrentLyricIndex { get; set; } = -1;
        public static string CurrentLyricContent { get; set; } = "";
        public static string CurrentLyricTranslation { get; set; } = "";

        public static bool ExchangeLyricsAndTranslations { get; set; } = false;

        public static DispatcherTimer LyricServiceTimer { get; set; } = new DispatcherTimer();

        public static EventHandler CurrentLyricIndexChanged;

        public static void InitLyricService()
        {
            LyricServiceTimer.Interval = TimeSpan.FromMilliseconds(50);
            LyricServiceTimer.Tick += (a, b) =>
            {
                if (CurrentLyrics == null)
                {
                    CurrentLyricContent = "";
                    CurrentLyricTranslation = "";
                    LyricServiceTimer.Stop();
                    return;
                }
                GetCurrentLyric();
            };
        }

        static String CurrentPosition = "";

        static void GetCurrentLyric()
        {
            if (PlayCore.MainMediaPlayer.MediaPlayer == null) return;
            //Debug.WriteLine(PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString().Substring(3, 7));
            CurrentPosition = PlayCore.MainMediaPlayer.MediaPlayer.Position.ToString();
            if (CurrentPosition.Length < 11)
                return;
            int newIndex = CurrentLyrics.FindIndex(x => x.Time == CurrentPosition.Substring(3, 7));
            if (newIndex == -1 || newIndex == CurrentLyricIndex || newIndex >= CurrentLyrics.Count) return;
            CurrentLyricIndex = newIndex;
            CurrentLyricContent = CurrentLyrics[newIndex].Content;
            CurrentLyricTranslation = CurrentLyrics[newIndex].Translation;
            CurrentLyricIndexChanged?.Invoke(null, null);
        }

        public static async Task<List<Lyric>> LoadLyricsFromStorageAsync(Music music)
        {
            if (KnownFolders.MusicLibrary == null)
                return null;
            StorageFolder LyricFolder = await StorageManager.GetApplicationDataFolder("Lyrics");
            if (await StorageManager.IsItemExsitAsync(LyricFolder, StorageManager.RemoveIllegalCharacter(music.Artist + " - " + music.Title + " - " + music.Album + ".lrc")))
            {
                return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(await LyricFolder.GetFileAsync(StorageManager.RemoveIllegalCharacter(music.Artist + " - " + music.Title + " - " + music.Album + ".lrc"))));
            }
            //if (await StorageManager.IsItemExsitAsync(LyricFolder, music.Title + " - " + music.Album + ".lrc"))
            //{
            //    return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(await LyricFolder.GetFileAsync(music.Title + " - " + music.Album + ".lrc")));
            //}
            //if (await StorageManager.IsItemExsitAsync(LyricFolder, music.Title + " - " + music.Album + " - " + music.Artist + ".lrc"))
            //{
            //    return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(await LyricFolder.GetFileAsync(music.Title + " - " + music.Album + " - " + music.Artist + ".lrc")));
            //}
            //if (await StorageManager.IsItemExsitAsync(LyricFolder, music.Title + " - " + music.Artist + ".lrc"))
            //{
            //    return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(await LyricFolder.GetFileAsync(music.Title + " - " + music.Artist + ".lrc")));
            //}
            //if (await StorageManager.IsItemExsitAsync(LyricFolder, music.Title +  " - " + music.Artist+ " - " + music.Album + ".lrc"))
            //{
            //    return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(await LyricFolder.GetFileAsync(music.Title + " - " + music.Artist + " - " + music.Album + ".lrc")));
            //}
            //Debug.WriteLine(music.Artist + " - " + music.Title + " - " + music.Album + ".lrc");
            //if (await StorageManager.IsItemExsitAsync(LyricFolder, music.Artist + " - " + music.Title + " - " + music.Album + ".lrc"))
            //{
            //    return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(await LyricFolder.GetFileAsync(music.Artist + " - " + music.Title + " - " + music.Album + ".lrc")));
            //}

            //if (await StorageManager.IsItemExsitAsync(LyricFolder, music.Artist + " - " + music.Album + " - " + music.Title + ".lrc"))
            //{
            //    return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(await LyricFolder.GetFileAsync(music.Artist + " - " + music.Album + " - " + music.Title + ".lrc")));
            //}

            //if (await StorageManager.IsItemExsitAsync(LyricFolder, music.Artist + " - " + music.Title + ".lrc"))
            //{
            //    return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(await LyricFolder.GetFileAsync(music.Artist + " - " + music.Title + ".lrc")));
            //}

            //if (await StorageManager.IsItemExsitAsync(LyricFolder, music.Title + ".lrc"))
            //{
            //    return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(await LyricFolder.GetFileAsync(music.Title + ".lrc")));
            //}
            return null;
        }

        public static async Task<bool> RemoveLyricAsync(Music music)
        {
            StorageFolder LyricFolder = await StorageManager.GetApplicationDataFolder("Lyrics");
            Debug.WriteLine("删除歌词："+music.Artist + " - " + music.Title + " - " + music.Album + ".lrc");
            IStorageItem item = await LyricFolder.TryGetItemAsync(StorageManager.RemoveIllegalCharacter(music.Artist + " - " + music.Title + " - " + music.Album + ".lrc"));
            if(item == null)
                return false;
            await ((StorageFile)item).DeleteAsync();
            return true;
        }
        public static async Task<List<Lyric>> LoadLyricsFromFilePikerAsync(Music music,bool? CopyToLyricsFolder = false)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".lrc");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return null;
            if (CopyToLyricsFolder == true)
            {
                StorageFolder LyricFolder = await StorageManager.GetApplicationDataFolder("Lyrics");
                await RemoveLyricAsync(music);
                file = await file.CopyAsync(LyricFolder,StorageManager.RemoveIllegalCharacter(music.Artist + " - " + music.Title + " - " + music.Album + ".lrc"));
            }
            if (file.FileType == ".lrc")
                return ProcessLyricsFromLRCFileString(await StorageManager.ReadFile(file));
            return null;
        }

        public static async Task<List<Lyric>> LoadLyricsFromMusicFile(Music music)
        {
            if (music == null)
                return null;
            if (music.MusicType == MusicType.Online)
                return null;
            StorageFile storageFile = LibraryManager.GetLocalMusicFile(music);
            if (storageFile == null)
                return null;
            if (storageFile.FileType == ".ac3" || storageFile.FileType == ".m4a") return new List<Lyric>();
            UwpStorageFileAbstraction uwpStorageFileAbstraction = new UwpStorageFileAbstraction(storageFile);
            File.IFileAbstraction fileAbstraction = uwpStorageFileAbstraction;
            try
            {
                TagLib.File file = TagLib.File.Create(fileAbstraction, ReadStyle.Average);
                string lyricContent = file.Tag.Lyrics;
                //string lyricContent = file.Tag.Description;
                return ProcessLyricsFromLRCFileString(lyricContent);
            }
            catch
            {

            }
            return null;
        }
        public static List<Lyric> ProcessLyricsFromLRCFileString(String str)
        {
            List<Lyric> lyrics = new List<Lyric>();

            int str_DoseBracket_Index = 0;
            int str_LineFeed_Index = 0;

            if (String.IsNullOrEmpty(str)) return new List<Lyric> { };
            str = str.Replace("\n", "\r");
            str = str.Replace("\r\r", "\r");



            while (!String.IsNullOrEmpty(str))
            {
                str_DoseBracket_Index = str.IndexOf("]");
                if (str_DoseBracket_Index == -1) break;
                Lyric lyric = new Lyric();
                str_LineFeed_Index = str.IndexOf("\r");
                if (str.IndexOf("[0") == -1 || str.IndexOf("[0") > 0)
                {
                    str = str.Substring(str_LineFeed_Index + 1);
                    continue;
                }
                lyric.Time = str.Substring(1, str_DoseBracket_Index - 1);


                if (str.IndexOf("\r") - str.IndexOf("]") - 1 <= 0)
                    lyric.Content = "";
                else
                {
                    lyric.Content = str.Substring(str_DoseBracket_Index + 1, str.IndexOf("\r") - str.IndexOf("]") - 1);
                    if (String.IsNullOrEmpty(lyric.Content) == false)
                    {
                        int translationLeft = lyric.Content.IndexOf("「");
                        int translationLength = lyric.Content.IndexOf("」") - translationLeft;
                        if (translationLeft >= 0 && translationLength > 0)
                        {
                            lyric.Translation = lyric.Content.Substring(translationLeft + 1, translationLength - 1);
                            lyric.Content = lyric.Content.Substring(0, translationLeft);
                        }
                    }
                }
                lyrics.Add(lyric);
                if (str_LineFeed_Index == -1 && str_LineFeed_Index + 1 < str.Length - 1)
                {
                    str_DoseBracket_Index = str.IndexOf("]");
                    if (str_DoseBracket_Index == -1) break;
                    lyric = new Lyric();
                    lyric.Time = str.Substring(1, str_DoseBracket_Index - 1);
                    if (str.Length - str.IndexOf("]") - 1 <= 0)
                        lyric.Content = "";
                    else
                    {
                        lyric.Content = str.Substring(str_DoseBracket_Index + 1, str.Length - str.IndexOf("]") - 1);
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

                //Debug.WriteLine(str_DoseBracket_Index+"|"+str_LineFeed_Index+"\n"+str);
                str = str.Substring(str_LineFeed_Index + 1);
            }
            CurrentLyrics = lyrics;

            for (int i = lyrics.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < i; j++)
                {
                    if (lyrics[i].Time == lyrics[j].Time)
                    {
                        lyrics[j].Translation = lyrics[i].Content;
                        lyrics.Remove(lyrics[i]);
                        break;
                    }
                }
            }
            for (int i = 0; i < lyrics.Count; i++)
            {
                if (lyrics[i].Time.Length > 6)
                    lyrics[i].Time = lyrics[i].Time.Substring(0, 7);
                else
                {
                    return null;
                }
            }
            if (ExchangeLyricsAndTranslations)
                ExchangeCurrentLyricsAndTranslations();
            return lyrics;
        }

        public static void ExchangeCurrentLyricsAndTranslations()
        {
            if (CurrentLyrics == null)
                return;
            String temp;
            for (int i=0;i<CurrentLyrics.Count;i++)
            {
                if (String.IsNullOrEmpty(CurrentLyrics[i].Content) || String.IsNullOrEmpty(CurrentLyrics[i].Translation))
                    continue;
                temp = CurrentLyrics[i].Content;
                CurrentLyrics[i].Content = CurrentLyrics[i].Translation;
                CurrentLyrics[i].Translation = temp;
            }
        }
    }
}

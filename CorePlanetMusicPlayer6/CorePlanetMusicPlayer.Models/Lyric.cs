using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Storage;
using Windows.UI.Xaml;

namespace CorePlanetMusicPlayer.Models
{
    public class Lyric
    {
        public TimeSpan Time { get; set; }
        public string Content { get; set; } = "";
        public string Translation { get; set; } = "";
    }

    public class LyricHelper
    {
        public static StorageFolder LyricFolder;

        /// <summary>
        /// 通过IMusic对象获取歌词
        /// </summary>
        /// <param name="music"></param>
        /// <returns></returns>
        public static async Task<List<Lyric>> GetLyricByMusicAsync(IMusic music)
        {
            if (music is null) return null;
            List<Lyric> lyricList = new List<Lyric>();
            if (music is LocalMusic)
            {
                lyricList = await GetLyricFromLocalMusicAsync(music as LocalMusic);
                if (lyricList != null)
                    return lyricList;
            }
            // 从缓存文件中获取
            if(LyricFolder is null)
            {
                await GetLyricFolderAsync();
            }
            IReadOnlyCollection<StorageFile> files = await StorageHelper.GetFilesInFolder(LyricFolder);
            StorageFile lyricFile = files.ToList().Find(x => x.Name.Equals(GetLyricFileName(music)));
            if (lyricFile is null) return null;
            string lrcContent = await StorageHelper.ReadFileAsStringAsync(lyricFile);
            return GetLyricsFromLRCContent(lrcContent);
        }

        /// <summary>
        /// 使用文件选择器为某一首音乐选取一个歌词文件
        /// </summary>
        /// <param name="music">选取歌词的音乐项</param>
        /// <param name="saveToData">是否保存到应用内部文件夹</param>
        /// <returns></returns>
        public static async Task<List<Lyric>> PickLyricFileAsync(IMusic music, bool? saveToData = true)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".lrc");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return null;
            // 校验选取的文件是否为合法的LRC文件
            string lrcContent = await StorageHelper.ReadFileAsStringAsync(file);
            List<Lyric> lyrics = GetLyricsFromLRCContent(lrcContent);
            if (lyrics is null || lyrics.Count == 0) return null;
            if (saveToData == true)
            {
                // 将LRC文件拷贝到内部文件夹，以便下次使用
                await DeleteLyricAsync(music);
                file = await file.CopyAsync(LyricFolder, GetLyricFileName(music));
                if (file is null)
                    return null;
            }
            return lyrics;
        }

        public static async Task<bool> DeleteLyricAsync(IMusic music)
        {
            if (LyricFolder is null)
            {
                await GetLyricFolderAsync();
            }
            IStorageItem item = await LyricFolder.TryGetItemAsync(GetLyricFileName(music));
            if (item == null)
                return false;
            await ((StorageFile)item).DeleteAsync();
            return true;
        }

        private static string GetLyricFileName(IMusic music)
        {
            return StorageHelper.RemoveIllegalCharacter(music.Artist + " - " + music.Title + " - " + music.Album + ".lrc");
        }

        private static async Task GetLyricFolderAsync()
        {
            StorageFolder folder = await StorageHelper.GetApplicationDataFolderAsync("Data");
            LyricFolder = await StorageHelper.GetStorageFolderFromStorageFolderAsync(folder, "Lyrics");
            // IReadOnlyCollection<StorageFile> files = await StorageHelper.GetFilesInFolder(folder);
        }

        public static async Task<List<Lyric>> GetLyricFromLocalMusicAsync(LocalMusic localMusic)
        {
            // 尝试读取内嵌歌词
            string str = await LocalMusicManager.GetLyricStrFromFileAsync(localMusic);
            if (String.IsNullOrEmpty(str) == false)
            {
                return GetLyricsFromLRCContent(str);
            }
            return null;
        }

        /// <summary>
        /// 从LRC文本中获取歌词
        /// </summary>
        /// <param name="str">LRC文本</param>
        /// <returns></returns>
        public static List<Lyric> GetLyricsFromLRCContent(string str)
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
                lyric.Time = StringHelper.ConvertLRCTimeToTimeSpan(str.Substring(1, str_DoseBracket_Index - 1));//待优化


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
                //if (IgnoreEmptyLine == false || !String.IsNullOrEmpty(lyric.Content) || !String.IsNullOrEmpty(lyric.Translation))
                //    lyrics.Add(lyric);
                if (str_LineFeed_Index == -1 && str_LineFeed_Index + 1 < str.Length - 1)
                {
                    str_DoseBracket_Index = str.IndexOf("]");
                    if (str_DoseBracket_Index == -1) break;
                    lyric = new Lyric();
                    lyric.Time = lyric.Time = StringHelper.ConvertLRCTimeToTimeSpan(str.Substring(1, str_DoseBracket_Index - 1));//待优化
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
                    //else
                    //{
                    //    if (IgnoreEmptyLine == false || !String.IsNullOrEmpty(lyric.Content) || !String.IsNullOrEmpty(lyric.Translation))
                    //        lyrics.Add(lyric);
                    //}
                    lyrics.Add(lyric);
                    break;
                }
                
                //Debug.WriteLine(str_DoseBracket_Index+"|"+str_LineFeed_Index+"\n"+str);
                str = str.Substring(str_LineFeed_Index + 1);
            }

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
            return lyrics;
        }


        /// <summary>
        /// 通过TimeSpan获取当前歌词
        /// </summary>
        /// <param name="lyrics"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static int GetCurrentLyricIndex(List<Lyric>lyrics, TimeSpan currentTime)
        {
            // 验证输入参数
            if (lyrics == null || lyrics.Count == 0)
                return -1;

            // 查找最后一个时间小于等于当前播放时间的歌词
            var matchedLyric = lyrics.LastOrDefault(l => l.Time <= currentTime);

            // 返回找到的歌词索引，未找到则返回-1
            return matchedLyric != null ? lyrics.IndexOf(matchedLyric) : -1;
        }
    }
}

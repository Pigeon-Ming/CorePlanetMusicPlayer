using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;

namespace CorePlanetMusicPlayer.Models
{
    public class Lyric
    {
        public TimeSpan Time { get; set; }
        public string Content { get; set; } = "";
        public string Translation { get; set; } = "";
    }

    public class LyricManager
    {

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

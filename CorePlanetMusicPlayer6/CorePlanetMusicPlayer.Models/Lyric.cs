using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<Lyric> GetLyricListFromLRCContent(string str)
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
                //if (IgnoreEmptyLine == false || !String.IsNullOrEmpty(lyric.Content) || !String.IsNullOrEmpty(lyric.Translation))
                //    lyrics.Add(lyric);
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
                    //else
                    //{
                    //    if (IgnoreEmptyLine == false || !String.IsNullOrEmpty(lyric.Content) || !String.IsNullOrEmpty(lyric.Translation))
                    //        lyrics.Add(lyric);
                    //}
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
    }
}

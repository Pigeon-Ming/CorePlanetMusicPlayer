using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace CorePlanetMusicPlayer.Models
{
    /// <summary>
    /// 歌曲的年份列表
    /// </summary>
    public class Year
    {
        public Year(uint year)
        {
            ReleaseYear = year;
        }
        public uint ReleaseYear { get; private set; }

        public List<IMusic> Music {  get; set; } = new List<IMusic>();

        /// <summary>
        /// 获取该年份里包含的专辑
        /// </summary>
        /// <returns>该年份里包含的专辑</returns>
        public List<Album> GetAlbums()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取该年份里包含的艺术家
        /// </summary>
        /// <returns>该年份里包含的艺术家</returns>
        public List<Artist> GetArtists()
        {
            throw new NotImplementedException();
        }
    }

    public class YearManager
    {
        public static ObservableCollection<Year> Years { get; set; } = new ObservableCollection<Year>();

        public static void RefreshYearsList(List<IMusic> musicList)
        {
            Years.Clear();
            AddMusicToYears(musicList);
        }

        public static void AddMusicToYears(List<IMusic> musicList)
        {
            // 1. 构建现有年份的字典
            var yearDict = Years.ToDictionary(y => y.ReleaseYear);

            // 2. 批量处理
            foreach (IMusic music in musicList)
            {
                Year year;
                if (!yearDict.TryGetValue(music.Year, out year))
                {
                    year = new Year(music.Year);
                    Years.Add(year);
                    yearDict[music.Year] = year;
                }
                year.Music.Add(music);
            }
        }

        public static void AddMusicToYears(IMusic music)
        {
            var yearList = Years.Where(x => x.ReleaseYear == music.Year).ToList();
            Year year = null;
            if(yearList != null && yearList.Count != 0)
            {
                year.Music.Add(music);
            }
            else
            {
                year = new Year(music.Year);
                year.Music.Add(music);
                Years.Add(year);
            }
        }

        public static void RemoveMusicFromYear(IMusic music)
        {
            List<Year> years = Years.ToList();
            Year year = years.Find(x => x.ReleaseYear == music.Year);
            if (year == null)
                return;
            bool isSucceed = year.Music.Remove(music);
            Debug.WriteLine($"从年份中移除音乐：{music.Title} 是否成功？ {isSucceed}");
            if (year.Music.Count == 0)
                Years.Remove(year);
        }

        public static void RemoveMusicFromYear(List<IMusic> musicList)
        {
            List<Year> years = Years.ToList();
            foreach (IMusic music in musicList)
            {
                Year year = years.Find(x => x.ReleaseYear == music.Year);
                if (year == null)
                    continue;
                bool isSucceed = year.Music.Remove(music);
                Debug.WriteLine($"从年份中移除音乐：{music.Title} 是否成功？ {isSucceed}");
                if (year.Music.Count == 0)
                    Years.Remove(year);
            }
        }
    }
}

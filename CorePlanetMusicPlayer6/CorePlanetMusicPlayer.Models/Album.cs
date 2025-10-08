using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;

namespace CorePlanetMusicPlayer.Models
{
    public class Album
    {
        public string Name { get; set; }

        public List<List<IMusic>> Discs { get; set; } = new List<List<IMusic>>(); // 第一层List为专辑的碟片，通过碟片号访问歌曲

        /// <summary>
        /// 获取专辑中歌曲的总时长
        /// </summary>
        /// <returns>专辑中歌曲的总时长</returns>
        public TimeSpan GetTotalDuration()
        {
            // To-Do: 计算总时长
            TimeSpan totalDuration = TimeSpan.Zero;
            foreach (List<IMusic>musicList in Discs)
            {
                totalDuration = totalDuration.Add(MusicHelper.GetTotalDuration(musicList));
            }
            return totalDuration;
        }

        /// <summary>
        /// 获取参与该专辑的艺术家列表
        /// </summary>
        /// <returns>参与该专辑的艺术家列表</returns>
        public List<Artist> GetArtists()
        {
            // To-Do: 获取艺术家列表
            
            return new List<Artist>();
        }

        /// <summary>
        /// 获取专辑发行的年份
        /// </summary>
        /// <returns>专辑发行年份</returns>
        public uint GetReleaseYear()
        {
            if (Discs.Count != 0)
            {
                List<IMusic> musicList = Discs.First();
                if (musicList != null && musicList.Count != 0)
                {
                    return musicList.First().Year;
                }
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// 获取专辑歌曲数量
        /// </summary>
        /// <returns>专辑歌曲数</returns>
        public int GetMusicCount()
        {
            int count = 0;
            foreach (List<IMusic> musicList in Discs)
            {
                count += musicList.Count;
            }
            return count;
        }

        /// <summary>
        /// 获取某一碟片中的音乐数
        /// </summary>
        /// <param name="discNumber">碟片号</param>
        /// <returns>指定碟片中的音乐数</returns>
        public int GetTrackCountInDisc(int discNumber)
        {
            if(Discs.Count >= discNumber && discNumber > 0)
            {
                return Discs[discNumber].Count;
            }
            return -1;
        }

        /// <summary>
        /// 获取专辑的碟片数量
        /// </summary>
        /// <returns>专辑的碟片数</returns>
        public int GetDiscCount()
        {
            return Discs.Count;
        }
    }

    public class AlbumManager
    {
        public static ObservableCollection<Album> Albums { get; private set; } = new ObservableCollection<Album>();

        public static void RefreshAlbumsList(List<IMusic> musicList)
        {
            for (int i = 0; i < musicList.Count; i++)
            {
                string albumName = musicList[i].Album;
                Album album = Albums.ToList().Find(x => x.Name == albumName);
                if (album != null)
                {
                    //album.Music.Add(music);
                }
                else
                {
                    //album = new Artist();
                    //album.Name = artistNames[i];
                    //album.Music.Add(music);
                    //Albums.Add(album);
                }

            }
            Albums.Clear();
        }

        public static void AddMusicToAlbum()
        {
            
        }
    }
}

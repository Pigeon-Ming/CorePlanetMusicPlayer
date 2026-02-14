using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;

namespace CorePlanetMusicPlayer.Models
{
    public class Disc: List<IMusic>
    {
        public uint Number { get; set; } = 0;

        public string Key { get { return Number.ToString(); } }

        public delegate string GetKeyDelegate(IMusic item);

        public string Name { get; set; } = "";
    }

    public class Album
    {
        public string Name { get; set; }

        public string Description { get; set; } = "";

        public List<Disc> Discs { get; set; } = new List<Disc>(); // 第一层List为专辑的碟片，通过碟片号访问歌曲

        public string CoverPath 
        {
            get
            {
                //TODO: 设置专辑封面图
                // 思路：查询所有的音乐，如果有此专辑中有本地音频文件，就从本地读取封面；如果全是StreamMusic默认为空，可以单独设置冯敏URL地址
                return GetCoverPath();
            }
        }

        public int MusicCount
        {
            get
            {
                // 遍历所有子列表并累加数量
                return GetMusicCount();
            }
        }

        public uint ReleaseYear
        {
            get
            {
                return GetReleaseYear();
            }
        }

        public string ArtistsString
        {
            get 
            {
                List<Artist> artists = GetArtists();
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Artist artist in artists)
                {
                    stringBuilder.Append(artist.Name);
                    stringBuilder.Append("; ");
                }
                string finalString = stringBuilder.ToString();
                if (String.IsNullOrEmpty(finalString))
                {
                    return "未知艺术家";
                }
                else
                {
                    return finalString.Substring(0, finalString.Length-2);
                }
            }
        }

        public string DurationString
        {
            get
            {
                return GetTotalDuration().ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// 获取专辑中歌曲的总时长
        /// </summary>
        /// <returns>专辑中歌曲的总时长</returns>
        public TimeSpan GetTotalDuration()
        {
            // TODO: 计算总时长
            TimeSpan totalDuration = TimeSpan.Zero;
            foreach (Disc disc in Discs)
            {
                totalDuration = totalDuration.Add(MusicHelper.GetTotalDuration(disc.ToList()));
            }
            return totalDuration;
        }

        /// <summary>
        /// 获取参与该专辑的艺术家列表
        /// </summary>
        /// <returns>参与该专辑的艺术家列表</returns>
        public List<Artist> GetArtists()
        {
            return ArtistManager.GetArtistsFromAlbum(this);
        }

        /// <summary>
        /// 获取专辑发行的年份
        /// </summary>
        /// <returns>专辑发行年份</returns>
        public uint GetReleaseYear()
        {
            if (Discs.Count != 0)
            {
                List<IMusic> musicList = Discs.First().ToList();
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
            return Discs.Sum(subList => subList?.ToList()?.Count ?? 0);
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
                return Discs[discNumber].ToList().Count;
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

        /// <summary>
        /// 获取专辑的封面Path，如果这个专辑中没有本地音乐,就返回找到的第一个StreamMusic的封面
        /// </summary>
        /// <returns></returns>
        private string GetCoverPath()
        {
            foreach (Disc disc in Discs)
            {
                foreach (IMusic music in disc)
                {
                    if(music is LocalMusic)
                    {
                        return ((LocalMusic)music).Path;
                    }
                }
            }
            return "";
        }
    }

    public class AlbumManager
    {
        public static ObservableCollection<Album> Albums { get; private set; } = new ObservableCollection<Album>();

        public static void RefreshAlbumsList(List<IMusic> musicList)
        {
            Albums.Clear();
            AddMusicToAlbum(musicList);
        }

        public static void AddMusicToAlbum(List<IMusic> musicList)
        {
            List<Album> albums = new List<Album>();

            // 1. 按专辑名分组
            var albumGroups = musicList.GroupBy(m => m.Album);

            foreach (var albumGroup in albumGroups)
            {
                var album = new Album();
                album.Name = albumGroup.Key;

                // 2. 按 DiscNumber 分组
                var discGroups = albumGroup.GroupBy(m => m.DiscNumber)
                                           .OrderBy(g => g.Key);

                // 3. 将每个碟片的曲目列表加入 Discs
                foreach (var discGroup in discGroups)
                {
                    List<IMusic> tracks = discGroup.OrderBy(m => m.TrackNumber).ToList();
                    Disc disc = new Disc { Number = discGroup.First().DiscNumber };
                    foreach (var track in tracks)
                    {
                        disc.Add(track);
                    }
                    album.Discs.Add(disc);
                }

                albums.Add(album);
            }
            Albums = new ObservableCollection<Album>(Albums.Concat<Album>(albums));
        }

        public static void AddMusicToAlbum(IMusic music)
        {
            var albums = Albums.Where(x=>x.Name == music.Album);
            if(albums != null)
            {
                Album album = albums.ToList().First();
                Disc disc = album.Discs.Find(x=> x.Number == music.DiscNumber);
                if (disc == null)
                {
                    disc = new Disc();
                    disc.Number = music.DiscNumber;
                    disc.Add(music);
                    album.Discs.Add(disc);
                }
                else
                {
                    disc.Add(music);
                    var newDisc = disc.OrderBy(x => x.DiscNumber).ToList();
                    disc.Clear();
                    disc.AddRange(newDisc);
                }
            }
            else
            {
                Album album = new Album();
                album.Name = music.Album;
                Disc disc = new Disc();
                disc.Add(music);
                album.Discs.Add(disc);
            }
        }

        public static void RemoveMusicFromAlbum(IMusic music)
        {
            List<Album> albums = Albums.ToList();
            Album album = albums.Find(x=>x.Name == music.Album);
            if (album == null)
                return;
            Disc disc = album.Discs.Find(x=>x.Number == music.DiscNumber);
            if (disc == null)
                return;
            bool isSucceed = disc.Remove(music);
            Debug.WriteLine($"从专辑中移除音乐：{music.Title} 是否成功？ {isSucceed}");
        }

        public static void RemoveMusicFromAlbum(List<IMusic> musicList)
        {
            List<Album> albums = Albums.ToList();
            foreach (IMusic music in musicList)
            {
                Album album = albums.Find(x => x.Name == music.Album);
                if (album == null)
                    return;
                Disc disc = album.Discs.Find(x => x.Number == music.DiscNumber);
                if (disc == null)
                    return;
                bool isSucceed = disc.Remove(music);
                Debug.WriteLine($"从专辑中移除音乐：{music.Title} 是否成功？ {isSucceed}");
            }
        }
    }
}

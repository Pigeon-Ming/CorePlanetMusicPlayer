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
    public class Artist
    {
        public string Name { get; set; }

        public string Description { get; set; } = "";

        public List<IMusic> Music { get; set; } = new List<IMusic>();

        public string CoverPath { get; set; } = "";

        /// <summary>
        /// 获取与该艺术家合作过的艺术家
        /// </summary>
        /// <returns>与该艺术家合作过的艺术家列表</returns>
        public List<Artist> GetCollaboratingArtists()
        {
            throw new NotImplementedException();
            //return new List<Artist>();
        }

        public List<Album> GetAlbums()
        {
            return AlbumManager.Albums.ToList().FindAll(x=> x.GetArtists().Contains(this));
        }
    }

    public class ArtistManager
    {
        public static ObservableCollection<Artist> Artists { get; private set; } = new ObservableCollection<Artist>();

        public static void RefreshArtistsList(List<IMusic> musicList)
        {
            Artists.Clear();
            AddMusicToArtist(musicList);
        }

        public static void AddMusicToArtist(List<IMusic> musicList)
        {
            List<Artist> artists = new List<Artist>();
            foreach (IMusic music in musicList)
            {
                List<string> artistNames = GetArtistNamesFromArtistString(music.Artist);

                for (int i = 0; i < artistNames.Count; i++)
                {
                    Artist artist = artists.ToList().Find(x => x.Name == artistNames[i]);
                    if (artist != null)
                    {
                        artist.Music.Add(music);
                    }
                    else
                    {
                        artist = new Artist();
                        artist.Name = artistNames[i];
                        artist.Music.Add(music);
                        artists.Add(artist);
                    }
                }
            }
            Artists = new ObservableCollection<Artist>(Artists.Concat<Artist>(artists));
        }

        public static void AddMusicToArtist(IMusic music)
        {
            List<string> artistNames = GetArtistNamesFromArtistString(music.Artist);

            for (int i = 0; i < artistNames.Count; i++)
            {
                Artist artist = Artists.ToList().Find(x => x.Name == artistNames[i]);
                if (artist != null)
                {
                    artist.Music.Add(music);
                }
                else
                {
                    artist = new Artist();
                    artist.Name = artistNames[i];
                    artist.Music.Add(music);
                    Artists.Add(artist);
                }

            }
        }

        public static List<Artist> GetArtistsFromMusicList(List<IMusic> musicList)
        {
            List<Artist> artists = new List<Artist>();
            foreach(IMusic music in musicList)
            {
                artists.AddRange(GetArtistsFromMusic(music));
            }
            return artists;
        }

        public static List<Artist> GetArtistsFromMusic(IMusic music)
        {
            List<string> names = GetArtistNamesFromArtistString(music.Artist);
            List<Artist> artists = new List<Artist>();
            foreach (string name in names)
            {
                Artist artist = GetArtistByName(name);
                if (artist != null)
                {
                    artists.Add(artist);
                }
            }
            return  artists;
        }

        public static List<Artist> GetArtistsFromAlbum(Album album)
        {
            // 1. 收集所有IMusic的Artist字符串
            HashSet<string> artistNames = new HashSet<string>();
            foreach (Disc disc in album.Discs)
            {
                foreach (IMusic music in disc)
                {
                    List<string> names = GetArtistNamesFromArtistString(music.Artist);
                    foreach (string name in names)
                    {
                        artistNames.Add(name);
                    }
                }
            }

            // 2. 在全局Artist列表中查找对应Artist对象
            List<Artist> result = new List<Artist>();
            foreach (string name in artistNames)
            {
                Artist artist = Artists.FirstOrDefault(a => a.Name == name);
                if (artist != null)
                    result.Add(artist);
            }
            return result;
        }

        private static Artist GetArtistByName(string name)
        {
            return Artists.ToList().Find(x => x.Name == name);
        }

        private static List<string> GetArtistNamesFromArtistString(string artistString)
        {
            List<string> artists = new List<string>();
            artistString = artistString.Replace("; ", ";");
            int semicolonIndex = artistString.IndexOf(';');
            if (semicolonIndex == -1)
                artists.Add(artistString);
            else
            {
                do
                {
                    artists.Add(artistString.Substring(0, semicolonIndex));
                    artistString = artistString.Substring(semicolonIndex + 1);
                    semicolonIndex = artistString.IndexOf(";");
                } while (semicolonIndex != -1 && semicolonIndex != 0);
                artists.Add(artistString);
            }
            return artists;
        }

        public static void RemoveMusicFromArtist(IMusic music)
        {
            List<string>artistNames = GetArtistNamesFromArtistString(music.Artist);
            foreach (string artistName in artistNames)
            {
                Artist artist = GetArtistByName(artistName);
                if (artist == null)
                    continue;
                artist.Music.Remove(music);
                if (artist.Music.Count == 0)
                {
                    Artists.Remove(artist);
                }
            }
        }

        public static void RemoveMusicFromArtist(List<IMusic> musicList)
        {
            foreach (IMusic music in musicList)
            {
                RemoveMusicFromArtist(music);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class Album
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<Music> Music { get; set; } = new List<Music>();
    }

    public class AlbumManager
    {
        public static void AddMusicToAlbum(Music music)
        {
            Album album = Library.Albums.Find(x=>x.Name == music.Album);
            if (album != null)
            {
                album.Music.Add(music);
            }
            else
            {
                album = new Album();
                album.Name = music.Album;
                album.Music.Add(music);
                Library.Albums.Add(album);
            }
        }
    }
}

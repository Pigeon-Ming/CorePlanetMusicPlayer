using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class Album
    {
        public String Name { get; set; }
        public List<Music> IncludeMusic { get; set; }
    }

    public class AlbumManager
    {
        public static List<Album> Albums = new List<Album>();
        public static List<Album> AlbumSuggestions = new List<Album>();

        public static async Task ClassifyAlbum()//待优化
        {
            Albums.Clear();
            List<string> Name = new List<string>();
            for (int i = 0; i < Library.LocalLibraryMusic.Count; i++)
            {
                string currentAlbumName = Library.LocalLibraryMusic[i].Album;
                if (Name.IndexOf(currentAlbumName) == -1)//如果专辑库里没有这个名字
                {
                    Name.Add(currentAlbumName);
                    Album album = new Album { Name = currentAlbumName };
                    album.IncludeMusic = new List<Music>();
                    album.IncludeMusic.Add(Library.LocalLibraryMusic[i]);


                    Albums.Add(album);

                }
                else
                {
                    Albums[Name.IndexOf(currentAlbumName)].IncludeMusic.Add(Library.LocalLibraryMusic[i]);
                }
            }
            Albums = Albums.OrderBy(x => x.Name).ToList();
            GetAlbumSuggestions();


        }//对专辑进行分类

        public static Album FindAlbumByName(String Name)
        {
            return Albums.Find(x => x.Name.Contains(Name));
        }

        public static void GetAlbumSuggestions()
        {
            Random random = new Random();
            List<int> indexes = new List<int>();
            for(int i=0;i<10;i++)
            {
                int index;
                while(indexes.IndexOf(index = random.Next(Albums.Count - 1)) != -1) { }
                indexes.Add(index);
                AlbumSuggestions.Add(Albums[index]);
            }
        }
    }
}

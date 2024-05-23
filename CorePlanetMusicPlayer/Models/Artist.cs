using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class Artist
    {
        public String Name { get; set; }
        public List<Music> IncludeMusic { get; set; }
    }

    public class ArtistManager
    {
        public static List<Artist> Artists = new List<Artist>();

        

        public static async Task ClassifyArtist()//待优化
        {
            List<string> Name = new List<string>();
            for (int i = 0; i < Library.LocalLibraryMusic.Count; i++)
            {
                Debug.WriteLine("i="+i);
                Debug.WriteLine("artist="+ Library.LocalLibraryMusic[i].Artist);
                List<string> currentArtistName = new List<string>();
                //int currentArtistName_stringindex = 0;
                string fullArtistName = Library.LocalLibraryMusic[i].Artist;
                while (true)
                {
                    if (fullArtistName.IndexOf(";") != -1)
                    {
                        currentArtistName.Add(fullArtistName.Substring(0, fullArtistName.IndexOf(";")));
                        fullArtistName = fullArtistName.Substring(fullArtistName.IndexOf(";") + 2, fullArtistName.Length - fullArtistName.IndexOf(";") - 2);
                    }
                    else
                    {
                        currentArtistName.Add(fullArtistName);
                        break;
                    }
                }

                for (int j = 0; j < currentArtistName.Count; j++)
                {

                    if (Name.IndexOf(currentArtistName[j]) == -1)//如果艺术家库里没有这个名字
                    {
                        //Debug.WriteLine("1   "+AllData.SystemLibraryMusic[i].Name);
                        Name.Add(currentArtistName[j]);
                        Artist Artist = new Artist { Name = currentArtistName[j] };
                        Artist.IncludeMusic = new List<Music>();
                        Artist.IncludeMusic.Add(Library.LocalLibraryMusic[i]);
                        Artists.Add(Artist);

                    }
                    else
                    {
                        Artists[Name.IndexOf(currentArtistName[j])].IncludeMusic.Add(Library.LocalLibraryMusic[i]);
                    }
                }
            }
            Artists = Artists.OrderBy(x => x.Name).ToList();
            //for (int i = 0; i < Artists.Count; i++)
            //{
            //    AllData.allArtistName.Add(AllData.Artists[i].Name);
            //}
        }//对艺术家进行分类

        public static Artist FindArtistByName(String Name)
        {
            if(!String.IsNullOrEmpty(Name))
                return Artists.Find(x => x.Name.Contains(Name));
            else
                return new Artist { Name="未知艺术家"};
        }

        public static List<Album> GetArtistAlbums(Artist artist)
        {
            List<String>albumNames = new List<String>();
            List<Album> albums = new List<Album>();
            for (int i=0;i<artist.IncludeMusic.Count;i++)
            {
                if (albumNames.IndexOf(artist.IncludeMusic[i].Album) == -1)
                {
                    albumNames.Add(artist.IncludeMusic[i].Album);
                    albums.Add(AlbumManager.FindAlbumByName(artist.IncludeMusic[i].Album));
                }
            }
            return albums;
        }

        public static List<Artist> DivideArtist(String fullArtistName)
        {
            List<Artist> artists = new List<Artist>();
            List<String> ArtistNames = new List<string>();
            while (true)
            {
                if (fullArtistName.IndexOf(";") != -1)
                {
                    ArtistNames.Add(fullArtistName.Substring(0, fullArtistName.IndexOf(";")));
                    if (fullArtistName.IndexOf("; ") == -1)
                    {
                        fullArtistName = fullArtistName.Substring(fullArtistName.IndexOf(";") + 1, fullArtistName.Length - fullArtistName.IndexOf(";") - 1);
                    }
                    else
                    {
                        fullArtistName = fullArtistName.Substring(fullArtistName.IndexOf(";") + 2, fullArtistName.Length - fullArtistName.IndexOf(";") - 2);
                    }
                }
                else
                {
                    ArtistNames.Add(fullArtistName);
                    break;
                }
            }
            for (int i = 0; i < ArtistNames.Count; i++)
                artists.Add(FindArtistByName(ArtistNames[i]));
            return artists;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetMusicPlayer.Models
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
                List<string> currentArtistName = new List<string>();
                //int currentArtistName_stringindex = 0;
                string fullArtistName = Library.LocalLibraryMusic[i].Artist;
                while (true)
                {
                    if (fullArtistName.IndexOf(";") != -1)
                    {
                        currentArtistName.Add(fullArtistName.Substring(0, fullArtistName.IndexOf(";")));
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
            //for (int i = 0; i < Artists.Count; i++)
            //{
            //    AllData.allArtistName.Add(AllData.Artists[i].Name);
            //}
        }//对艺术家进行分类
    }
}

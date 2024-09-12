using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class Artist
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<Music> Music { get; set; } = new List<Music>();

    }

    public class ArtistManager
    {
        

        public static void AddMusicToArtist(Music music)
        {
            List<string>artists = GetArtistNamesFromArtistString(music.Artist);

            Debug.WriteLine(music.Title);

            for(int i = 0; i < artists.Count; i++)
            {
                Artist artist = Library.Artists.Find(x => x.Name == artists[i]);
                if (artist != null)
                {
                    artist.Music.Add(music);
                }
                else
                {
                    artist = new Artist();
                    artist.Name = artists[i];
                    artist.Music.Add(music);
                    Library.Artists.Add(artist);
                }
                
            }
                
        }

        public static List<string> GetArtistNamesFromArtistString(string ArtistString)
        {
            List<string>artists = new List<string>();
            ArtistString = ArtistString.Replace("; ",";");
            int semicolonIndex = ArtistString.IndexOf(';');
            if (semicolonIndex == -1)
                artists.Add(ArtistString);
            else
            {
                do
                {
                    artists.Add(ArtistString.Substring(0, semicolonIndex));
                    ArtistString = ArtistString.Substring(semicolonIndex + 1);
                    semicolonIndex = ArtistString.IndexOf(";");
                } while (semicolonIndex != -1);
                artists.Add(ArtistString);
            }
            return artists;
        }
    }
}

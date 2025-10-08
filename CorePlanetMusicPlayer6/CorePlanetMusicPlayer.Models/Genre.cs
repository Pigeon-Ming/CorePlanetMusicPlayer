using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class Genre
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public List<IMusic> Music {  get; set; }
    }

    public class GenreManager
    {
        public static ObservableCollection<Genre> Genres { get; set; } = new ObservableCollection<Genre>();

        public static void RefreshGenresList()
        {
            Genres.Clear();
        }

        public static void AddMusicToGenre()
        {

        }
    }
}

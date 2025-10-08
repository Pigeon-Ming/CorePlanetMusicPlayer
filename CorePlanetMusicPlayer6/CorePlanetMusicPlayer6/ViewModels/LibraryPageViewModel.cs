using CommunityToolkit.Mvvm.ComponentModel;
using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer6.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer6.ViewModels
{
    public class LibraryPageViewModel : ObservableObject
    {
        private ObservableCollection<Music>musicCollection = new ObservableCollection<Music>();

        public ObservableCollection<Music> MusicCollection
        {
            get { return musicCollection; }
            set 
            {
                musicCollection = value;
                OnPropertyChanged();
            }
        }

        
    }
}

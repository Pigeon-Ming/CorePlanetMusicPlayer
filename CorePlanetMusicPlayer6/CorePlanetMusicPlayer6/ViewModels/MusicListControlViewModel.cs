using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer6.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CorePlanetMusicPlayer6.ViewModels
{
    public class MusicListControlViewModel : Notify
    {
        private ObservableCollection<Music> musicCollection = new ObservableCollection<Music>();

        public ObservableCollection<Music> MusicCollection
        {
            get { return musicCollection; }
            set
            {
                musicCollection = value;
                OnPropertyChanged();
            }
        }

        public MusicListControlViewModel()
        {
            
        }

        private ICommand _playSelectedItem;

        public ICommand PlaySelectedItem
        {
            get { return _playSelectedItem; }
            set{ _playSelectedItem = value; OnPropertyChanged(); }
        }

        private ICommand CopyMusicTitle
        {

        }
    }
}

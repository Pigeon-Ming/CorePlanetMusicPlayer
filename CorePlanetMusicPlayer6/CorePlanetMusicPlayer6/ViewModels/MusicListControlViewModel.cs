using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using CorePlanetMusicPlayer.App;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CorePlanetMusicPlayer6.ViewModels
{
    public class MusicListControlViewModel : ObservableObject
    {
        private IPlayEngine playEngine;
        public IPlayEngine PlayEngine
        {
            get => playEngine;
            set
            {
                //playEngine = value;
                SetProperty(ref playEngine, value);
                //OnPropertyChanged();
            }
        }


        private ObservableCollection<IMusic> musicCollection = new ObservableCollection<IMusic>();

        public ObservableCollection<IMusic> MusicCollection
        {
            get => musicCollection;
            set
            {
                //musicCollection = value;
                SetProperty(ref musicCollection, value);
                //OnPropertyChanged();
            }
        }

        private MusicMenuViewModel musicMenuViewModel = new MusicMenuViewModel();

        public MusicMenuViewModel MusicMenuViewModel
        {
            get => musicMenuViewModel;
            set
            {
                SetProperty(ref musicMenuViewModel, value);
                //musicMenuViewModel = value;
                //OnPropertyChanged();
            }
        }

        public MusicListControlViewModel()
        {
            Play = new RelayCommand(OnPlayClick);
        }

        private ICommand _play;
        public ICommand Play
        {
            get => _play;
            set { _play = value; OnPropertyChanged(); }
        }

        private void OnPlayClick()
        {
            Debug.WriteLine("Play");
        }

        private ICommand _playSelectedItem;

        public ICommand PlaySelectedItem
        {
            get => _playSelectedItem;
            set
            {
                SetProperty(ref _playSelectedItem, value);
            }
        }

        
    }
}

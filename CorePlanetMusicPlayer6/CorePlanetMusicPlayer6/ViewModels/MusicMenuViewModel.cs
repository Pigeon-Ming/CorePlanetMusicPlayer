using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
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
    public class MusicMenuViewModel : Notify
    {
        private IMusic _menuMusic;

        private IPlayEngine _playEngine;

        private ObservableCollection<IMusic> _musicCollection = new ObservableCollection<IMusic>();

        public IMusic MenuMusic
        {
            get { return _menuMusic; }
            set { _menuMusic = value; OnPropertyChanged(); }
        }

        private IPlayEngine PlayEngine
        {
            get { return _playEngine; }
            set { _playEngine = value; OnPropertyChanged(); }
        }

        public ObservableCollection<IMusic> MusicCollection
        {
            get { return _musicCollection; }
            set { _musicCollection = value; OnPropertyChanged(); }
        }



        public MusicMenuViewModel()
        {
            Play = new ActionCommand(OnPlayClick);
        }


        private ICommand _play;
        public ICommand Play
        {
            get { return _play; }
            set { _play = value; OnPropertyChanged(); }
        }

        private ICommand _playNext;
        public ICommand PlayNext
        {
            get { return _playNext; }
            set { _playNext = value; OnPropertyChanged(); }
        }

        private ICommand _addToPlayQueue;
        public ICommand AddToPlayQueue
        {
            get { return _addToPlayQueue; }
            set { _addToPlayQueue = value; OnPropertyChanged(); }
        }

        private ICommand _viewAlbum;
        public ICommand ViewAlbum
        {
            get { return _viewAlbum; }
            set { _viewAlbum = value; OnPropertyChanged(); }
        }

        private ICommand _viewArtist;
        public ICommand ViewArtist
        {
            get { return _viewArtist; }
            set { _viewArtist = value; OnPropertyChanged(); }
        }

        private ICommand _viewInfo;
        public ICommand ViewInfo
        {
            get { return _viewInfo; }
            set { _viewInfo = value; OnPropertyChanged(); }
        }

        private ICommand _saveToPlayList;
        public ICommand SaveToPlayList
        {
            get { return _saveToPlayList; }
            set { _saveToPlayList = value; OnPropertyChanged(); }
        }



        private void OnPlayClick()
        {
            PlayEngine.PlayMusic(MenuMusic, MusicCollection.ToList(), MusicCollection.IndexOf(MenuMusic));
        }
    }
}

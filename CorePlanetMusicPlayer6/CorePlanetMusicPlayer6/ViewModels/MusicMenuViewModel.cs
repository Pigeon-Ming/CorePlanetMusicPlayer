using CommunityToolkit.Mvvm.ComponentModel;
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
    public class MusicMenuViewModel : ObservableObject
    {
        private IMusic _menuMusic;

        private IPlayEngine _playEngine;

        private ObservableCollection<IMusic> _musicCollection = new ObservableCollection<IMusic>();

        public IMusic MenuMusic
        {
            get { return _menuMusic; }
            set { _menuMusic = value; OnPropertyChanged(); }
        }

        public IPlayEngine PlayEngine
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
            //Play = new ActionCommand(OnPlayClick);
            //PlayNext = new ActionCommand(OnPlayNextClick);
            //AddToPlayQueue = new ActionCommand(OnAddToPlayQueueClick);
            //ViewAlbum = new ActionCommand(OnViewAlbumClick);
            //ViewArtist = new ActionCommand(OnViewArtistClick);
            //ViewInfo = new ActionCommand(OnViewInfoClick);
            //SaveToPlayList = new ActionCommand(OnSaveToPlayListClick);
        }


        private ICommand _play;
        public ICommand Play
        {
            get { return _play; }
            set { _play = value; OnPropertyChanged(); }
        }

        private void OnPlayClick()
        {
            PlayEngine.PlayMusic(MenuMusic, MusicCollection.ToList(), MusicCollection.IndexOf(MenuMusic));
        }

        private ICommand _playNext;
        public ICommand PlayNext
        {
            get { return _playNext; }
            set { _playNext = value; OnPropertyChanged(); }
        }

        private void OnPlayNextClick()
        {
            PlayQueue playQueue = PlayEngine.GetPlayQueue();
            playQueue.AddNextMusic(MenuMusic);
        }

        private ICommand _addToPlayQueue;
        public ICommand AddToPlayQueue
        {
            get { return _addToPlayQueue; }
            set { _addToPlayQueue = value; OnPropertyChanged(); }
        }
        private void OnAddToPlayQueueClick()
        {
            PlayQueue playQueue = PlayEngine.GetPlayQueue();
            playQueue.AddMusic(MenuMusic);
        }


        private ICommand _viewAlbum;
        public ICommand ViewAlbum
        {
            get { return _viewAlbum; }
            set { _viewAlbum = value; OnPropertyChanged(); }
        }
        private void OnViewAlbumClick()
        {
            //To-Do: View Album
        }

        private ICommand _viewArtist;
        public ICommand ViewArtist
        {
            get { return _viewArtist; }
            set { _viewArtist = value; OnPropertyChanged(); }
        }
        private void OnViewArtistClick()
        {
            //To-Do: View Artist
        }

        private ICommand _viewInfo;
        public ICommand ViewInfo
        {
            get { return _viewInfo; }
            set { _viewInfo = value; OnPropertyChanged(); }
        }
        private void OnViewInfoClick()
        {
            //To-Do: View Info
            Debug.WriteLine("ViewInfo");
        }

        private ICommand _saveToPlayList;
        public ICommand SaveToPlayList
        {
            get { return _saveToPlayList; }
            set { _saveToPlayList = value; OnPropertyChanged(); }
        }
        private void OnSaveToPlayListClick()
        {
            //To-Do: Save To PlayList
        }

        private ICommand _copyMusicTitle;

        public ICommand CopyMusicTitle
        {
            get { return _copyMusicTitle; }
            set { _copyMusicTitle = value; OnPropertyChanged(); }
        }

        private void OnCopyMusicTitleClick()
        {
            //To-Do: Copy Music Title
        }
    }
}

using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PlanetMusicPlayer.Controls;
using System.Diagnostics;

namespace PlanetMusicPlayer.ViewModels
{
    public class MusicListViewModel
    {
        public List<Music> currentList { get; set; } = new List<Music> { };
        public Music rightClickItem { get; set; }
        public Music selectedItem { get; set; }
        public ListView musicListView { get; set; }

        public MusicListViewModel(List<Music> _currentList,ListView listView)
        {
            this.rightClickItem = new Music { Artist = "", Title = "" };
            this.currentList = _currentList;
            this.musicListView = listView;
        }

        public void Menu_Play()
        {
            PlayCore.PlayMusic(rightClickItem, EventList<Music>.ListToEventList(this.currentList), musicListView.SelectedIndex);
        }

        public void Menu_PlayNext()
        {
            PlayQueueManager.AddMusicPlayNext(rightClickItem);
        }

        public void Menu_AddToPlayQueue()
        {
            PlayQueueManager.AddMusicToPlayQueue(rightClickItem);
        }

        public void Menu_AddToPlaylist()
        {
            ContentDialog dialog = new ContentDialog();
            dialog = new ContentDialog();
            dialog.Content = new SaveToPlaylistControl(rightClickItem);
            dialog.ShowAsync();
        }

        

    }
}

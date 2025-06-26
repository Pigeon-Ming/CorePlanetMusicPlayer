using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;

namespace CorePlanetMusicPlayer6.ViewModels
{
    public class MusicMenuViewModel
    {
        public IPlayEngine PlayEngine { get; set; }

        public IMusic SelectedMusic { get; set; }
        public List<IMusic> MusicList { get; set; } = new List<IMusic>();

        public void CopyMusicTitle(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(SelectedMusic.Title);
            Clipboard.SetContent(dataPackage);
        }

        public void Play(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("播放音乐：" + SelectedMusic.Title);
            PlayEngine.PlayMusic(SelectedMusic,MusicList,MusicList.IndexOf(SelectedMusic));
        }

        public void PlayNext(object sender, RoutedEventArgs e)
        {
            PlayEngine.GetPlayQueue().AddNextMusic(SelectedMusic);
        }

        public void AddToPlayQueue(object sender, RoutedEventArgs e)
        {
            PlayEngine.GetPlayQueue().AddMusic(SelectedMusic);
        }

        public void ViewAlbum(object sender, RoutedEventArgs e)
        {

        }

        public void ViewArtist(object sender, RoutedEventArgs e)
        {

        }

        public void ViewInfo(object sender, RoutedEventArgs e)
        {

        }

        public void SaveToPlaylist(object sender, RoutedEventArgs e)
        {

        }
    }
}

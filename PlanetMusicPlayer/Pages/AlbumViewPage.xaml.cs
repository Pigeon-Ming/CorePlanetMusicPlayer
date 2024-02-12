using CorePlanetMusicPlayer.Models;
using PlanetMusicPlayer.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace PlanetMusicPlayer.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumViewPage : Page
    {
        //public static Album ViewingAlbum{ get; set; }
        public Album ViewingAlbum { get; set; }
        public BasicMusicListControl basicMusicListControl;

        public AlbumViewPage(Album album)
        {
            this.InitializeComponent();
            if(album!=null)
            {
                this.ViewingAlbum = album;
                AlbumName.Text = ViewingAlbum.Name;
                SetCover();
                
                
            }
        }

        private async void SetCover()
        {
            AlbumCover.Source = (await MusicManager.GetMusicCoverAsync(ViewingAlbum.IncludeMusic[0])).cover;
            for(int i = 1;i<ViewingAlbum.IncludeMusic.Count;i++)
                await MusicManager.GetMusicCoverAsync(ViewingAlbum.IncludeMusic[i]);
            ListViewGrid.Children.Clear();
            basicMusicListControl = new BasicMusicListControl(ViewingAlbum.IncludeMusic);
            ListViewGrid.Children.Add(basicMusicListControl);
            
        }

        private void CommandBar_Play_Click(object sender, RoutedEventArgs e)
        {
            PlayCore.PlayMusic(ViewingAlbum.IncludeMusic[0],EventList<Music>.ListToEventList(ViewingAlbum.IncludeMusic),0);
        }

        private void CommandBar_AddToPlayQueue_Click(object sender, RoutedEventArgs e)
        {
            PlayQueue.normalList.AddRange(EventList<Music>.ListToEventList(ViewingAlbum.IncludeMusic));
        }   

        private void CommandBar_MultiSelect_Click(object sender, RoutedEventArgs e)
        {
            if (CommandBar_MultiSelect.IsChecked == true)
                basicMusicListControl.mainListView.SelectionMode = ListViewSelectionMode.Multiple;
            else
                basicMusicListControl.mainListView.SelectionMode = ListViewSelectionMode.Single;
        }

        private void CommandBar_EditAlbumProperty_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

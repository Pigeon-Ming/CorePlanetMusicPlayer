using CorePlanetMusicPlayer.Models;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.DevControls
{
    public sealed partial class ArtistControl : UserControl
    {
        private Artist artist;

        public ArtistControl()
        {
            this.InitializeComponent();
        }

        public void SetArtist(Artist artist)
        {
            this.artist = artist;
            UpdateView();
        }

        void UpdateView()
        {
            List<Album> albums = artist.GetAlbums();

            NameTextBlock.Text = $"名称：{artist.Name}";
            DescriptionTextBox.Text = artist.Description;

            AlbumsCountTextBlock.Text = $"专辑数：{albums.Count.ToString()}";
            MusicCountTextBlock.Text = $"音乐数：{artist.Music.Count.ToString()}";

            MusicListView.ItemsSource = null;
            MusicListView.ItemsSource = artist.Music;

            AlbumsListView.ItemsSource = null;
            AlbumsListView.ItemsSource = albums;
        }
    }
}

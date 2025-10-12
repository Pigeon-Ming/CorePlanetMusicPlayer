using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
    public sealed partial class AlbumControl : UserControl
    {
        private Album album;

        public AlbumControl()
        {
            this.InitializeComponent();
        }

        public void SetAlbum(Album album)
        {
            this.album = album;

            NameTextBlock.Text = album.Name;
            DescriptionTextBox.Text = album.Description;
            MusicCountTextBlock.Text = album.MusicCount.ToString();
            DiscCountTextBlock.Text = album.Discs.Count.ToString();
            List<Artist>artists = album.GetArtists();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Artist artist in artists)
            {
                stringBuilder.Append(artist.Name);
                stringBuilder.Append("; ");
            }
            stringBuilder = stringBuilder.Remove(stringBuilder.Length - 1, 1);
            ArtistsTextBlock.Text = stringBuilder.ToString();

            GroupedItemsViewSource.Source = album.Discs;
        }
    }
}

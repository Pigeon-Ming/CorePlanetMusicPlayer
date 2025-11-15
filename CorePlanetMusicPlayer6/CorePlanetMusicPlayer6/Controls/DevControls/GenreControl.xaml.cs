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
    public sealed partial class GenreControl : UserControl
    {
        private Genre genre;
        public GenreControl()
        {
            this.InitializeComponent();
        }

        public void SetGenre(Genre genre)
        {
            this.genre = genre;
            TitleTextBlock.Text = $"流派：{genre.Name}";
            IdTextBlock.Text = $"流派id：{genre.Id}";
            MusicCountTextBlock.Text = $"歌曲数：{genre.Music.Count}首";
            MusicListView.ItemsSource = null;
            MusicListView.ItemsSource = genre.Music;
        }
    }
}

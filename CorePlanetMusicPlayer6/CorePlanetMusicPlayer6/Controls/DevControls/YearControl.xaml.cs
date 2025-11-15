using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class YearControl : UserControl
    {
        private Year year;

        public YearControl()
        {
            this.InitializeComponent();
        }


        public void SetYear(Year year)
        {
            this.year = year;
            TitleTextBlock.Text = $"{year.ReleaseYear}年";
            MusicCountTextBlock.Text = $"歌曲数：{year.Music.Count}首";
            MusicListView.ItemsSource = null;
            MusicListView.ItemsSource = year.Music;
        }
    }
}

using CorePlanetMusicPlayer.Models;
using PlanetMusicPlayer.Pages;
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

namespace PlanetMusicPlayer.Controls.DevControls.LyricControls
{
    public sealed partial class Text2LyricControl : UserControl
    {
        public Text2LyricControl()
        {
            this.InitializeComponent();
        }

        public List<Lyric>lyrics = new List<Lyric>();
        

        private void Convert_Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(TextTextBox.Text))
                return;
            lyrics = LyricManager.ProcessLyrics(TextTextBox.Text);
            ResultListView.ItemsSource = lyrics;
        }

        private void Clear_Button_Click(object sender, RoutedEventArgs e)
        {
            TextTextBox.Text = "";
            lyrics.Clear();
            ResultListView.ItemsSource = null;
        }

        private void LoadFile_Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void LoadEmbeddedLyrics_Button_Click(object sender, RoutedEventArgs e)
        {
            lyrics = LyricManager.LoadFromMusicFile(PlayCore.CurrentMusic);
            ResultListView.ItemsSource = null;
            ResultListView.ItemsSource = lyrics;
        }
    }
}

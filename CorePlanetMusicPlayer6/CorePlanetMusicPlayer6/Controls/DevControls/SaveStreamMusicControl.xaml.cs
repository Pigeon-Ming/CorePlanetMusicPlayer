using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ContentDialogManager = CorePlanetMusicPlayer.App.ContentDialogManager;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CorePlanetMusicPlayer6.Controls.DevControls
{
    public sealed partial class SaveStreamMusicControl : UserControl
    {
        public StreamMusic StreamMusic { get; private set; }
        public SaveStreamMusicControl()
        {
            this.InitializeComponent();
            StreamMusic = new StreamMusic();
            initGenreComboBox();
        }

        public SaveStreamMusicControl(StreamMusic streamMusic)
        {
            this.InitializeComponent();
            StreamMusic = streamMusic;
            URLTextBox.IsReadOnly = true;
            initGenreComboBox();
        }

        void initGenreComboBox()
        {
            List<string> genres = GenreManager.GenreMap_Chinese.Values.ToList();
            genres.Insert(0,"[未指派]");
            GenreComboBox.ItemsSource = genres;
            GenreComboBox.SelectedIndex = StreamMusic.Genre == 255 ?  0 : (int)StreamMusic.Genre;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogManager.HideContentDialog();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(URLTextBox.Text))
            {
                URLTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            await SaveAsync();
            ContentDialogManager.HideContentDialog();
        }

        async Task SaveAsync()
        {
            

            StreamMusic.Url = URLTextBox.Text;
            StreamMusic.Title = TitleTextBox.Text;
            StreamMusic.Artist = ArtistTextBox.Text;
            StreamMusic.Album = AlbumTextBox.Text;
            string temp = BitrateTextBox.Text;
            if (!String.IsNullOrEmpty(temp))
                StreamMusic.Bitrate = Convert.ToUInt32(temp);
            else
                StreamMusic.Bitrate = 0;
            temp = TrackTextBox.Text;
            if (!String.IsNullOrEmpty(temp))
            {
                StreamMusic.TrackNumber = Convert.ToUInt32(temp);
            }
            else
            {
                StreamMusic.TrackNumber = 0;
            }
            temp = DiscTextBox.Text;
            if (!String.IsNullOrEmpty(temp))
            {
                StreamMusic.DiscNumber = Convert.ToUInt32(temp);
            }
            else
            {
                StreamMusic.DiscNumber = 0;
            }
            temp = YearTextBox.Text;
            if (!String.IsNullOrEmpty(temp))
            {
                StreamMusic.Year = Convert.ToUInt32(temp);
            }
            else
            {
                StreamMusic.Year = 0;
            }

            if (GenreComboBox.SelectedIndex > 0)
            {
                StreamMusic.Genre = (uint)(GenreComboBox.SelectedIndex - 1);
            }
            else
            {
                StreamMusic.Genre = 255;
            }
            StreamMusic.CoverUrl = CoverURLTextBox.Text;
            await Library.SaveStreamMusicAsync(StreamMusic);
        }
    }
}

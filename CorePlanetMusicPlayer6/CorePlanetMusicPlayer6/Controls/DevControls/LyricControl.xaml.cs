using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using CorePlanetMusicPlayer6.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UWPTools.Models;
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
    public sealed partial class LyricControl : UserControl
    {
        private DispatcherTimer LyricTimer = new DispatcherTimer();

        private List<Lyric> Lyrics = new List<Lyric>();

        private IPlayEngine playEngine;

        public LyricControl()
        {
            this.InitializeComponent();

            playEngine = ProgramData.PlayEngine;

            LyricTimer.Interval = TimeSpan.FromMilliseconds(20);
            LyricTimer.Tick += LyricTimer_Tick;
        }

        /*
        public void SetPlayEngine(IPlayEngine playEngine)
        {
            this.playEngine = playEngine;
        }
        */

        private void LyricTimer_Tick(object sender, object e)
        {
            int index = LyricManager.GetCurrentLyricIndex(Lyrics,playEngine.GetPlayProgress());
            if (index < 0 || index >= Lyrics.Count)
                return;
            for(int i = 0; i < index; i++)
            {
                ((LyricItemControl)LyricsStackPanel.Children[i]).SetAsNotCurrent();
            }
            ((LyricItemControl)LyricsStackPanel.Children[index]).SetAsCurrrent();
            for (int i = index + 1; i < Lyrics.Count; i++)
            {
                ((LyricItemControl)LyricsStackPanel.Children[i]).SetAsNotCurrent();
            }
        }

        private async void Menu_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            await OpenFileAsync();
        }

        public async Task OpenFileAsync()
        {
            LyricTimer.Stop();
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".lrc");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null)
                return;
            string lrcString = await StorageHelper.ReadFileAsStringAsync(file);
            Lyrics = LyricManager.GetLyricsFromLRCContent(lrcString);
            UpdateLyrics();
            LyricTimer.Start();
        }

        public void UpdateLyrics()
        {
            LyricsStackPanel.Children.Clear();
            for (int i = 0; i < Lyrics.Count; i++)
            {
                LyricsStackPanel.Children.Add(new LyricItemControl(Lyrics[i]));
            }
        }
    }
}

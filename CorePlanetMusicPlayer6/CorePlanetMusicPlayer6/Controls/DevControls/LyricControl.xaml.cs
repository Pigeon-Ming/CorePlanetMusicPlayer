 using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
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

        private List<Lyric> Lyrics = new List<Lyric>();

        private IPlayEngine playEngine;

        public LyricControl()
        {
            this.InitializeComponent();

            playEngine = ProgramData.PlayEngine;

            LyricService.CurrentLyricChanged += LyricService_CurrentLyricChanged;
            LyricService.LyricsChanged += LyricService_LyricsChanged;
        }

        private void LyricService_LyricsChanged(object sender, EventArgs e)
        {
            Lyrics = LyricService.Lyrics;
            UpdateLyrics();
        }

        private void LyricService_CurrentLyricChanged(object sender, int e)
        {
            UpdateView();
        }

        void UpdateView()
        {
            int index = LyricService.CurrentIndex;
            if (index < 0 || index >= Lyrics.Count)
                return;
            for (int i = 0; i < index; i++)
            {
                ((LyricItemControl)LyricsStackPanel.Children[i]).SetAsNotCurrent();
            }
            ((LyricItemControl)LyricsStackPanel.Children[index]).SetAsCurrrent();
            for (int i = index + 1; i < Lyrics.Count; i++)
            {
                ((LyricItemControl)LyricsStackPanel.Children[i]).SetAsNotCurrent();
            }
        }

        /*
        public void SetPlayEngine(IPlayEngine playEngine)
        {
            this.playEngine = playEngine;
        }
        */

        private async void Menu_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            await OpenFileAsync();
        }

        public async Task OpenFileAsync()
        {
            IMusic music = playEngine.GetPlayQueue().GetCurrentMusic();
            if (music != null) 
                await LyricService.PickLyricFileForCurrentMusicAsync();
        }

        public void UpdateLyrics()
        {
            LyricsStackPanel.Children.Clear();
            if (Lyrics is null) return;
            for (int i = 0; i < Lyrics.Count; i++)
            {
                LyricsStackPanel.Children.Add(new LyricItemControl(Lyrics[i]));
            }
        }
    }
}

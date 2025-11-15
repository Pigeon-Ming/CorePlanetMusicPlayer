using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using CorePlanetMusicPlayer.App;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
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
    public sealed partial class PlayingControl : UserControl
    {
        IPlayEngine playEngine = ProgramData.PlayEngine;

        DispatcherTimer progressTimer = new DispatcherTimer();

        public PlayingControl()
        {
            this.InitializeComponent();
            playEngine.StateChanged += PlayEngine_StateChanged;
            playEngine.PlayingChanged += PlayEngine_PlayingChanging;
            initProgressTimer();
        }

        void initProgressTimer() 
        {
            progressTimer.Interval = TimeSpan.FromSeconds(1);
            progressTimer.Tick += ProgressTimer_Tick;
            progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, object e)
        {
            TimeSpan playProgress = playEngine.GetPlayProgress();
            ProgressSlider.Value = playProgress.TotalSeconds;
            ProgressTextBlock.Text = playProgress.ToString(@"mm\:ss");
        }

        private async void PlayEngine_PlayingChanging(object sender, EventArgs e)
        {
            IMusic music = playEngine.GetPlayQueue().GetCurrentMusic();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TitleTextBlock.Text = music.Title;
                MessagesTextBlock.Text = music.Artist + " · " + music.Album;
                TimeSpan durationTimeSpan = MusicHelper.GetDurationTimeSpan(music);
                ProgressSlider.Maximum = durationTimeSpan.TotalSeconds;
                DurationTextBlock.Text = music.Duration;
            });
        }

        private async void PlayEngine_StateChanged(object sender, EventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (playEngine.PlayState == PlayState.Playing)
                {
                    PlayAndPauseIcon.Glyph = "\uE769";
                }
                else
                {
                    PlayAndPauseIcon.Glyph = "\uE768";
                }
            });
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            playEngine.Previous();
        }

        private void PlayAndPauseButton_Click(object sender, RoutedEventArgs e)
        {
            playEngine.PlayPause();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            playEngine.Next();
        }

        private void PlayModeButton_Click(object sender, RoutedEventArgs e)
        {
            playEngine.Previous();
        }

        private void ProgressSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            
        }

        private void ProgressSlider_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ((SystemMediaPlayer)playEngine).GetMediaPlayer().Position = TimeSpan.FromMinutes(3);
        }

        private void NextButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ((SystemMediaPlayer)playEngine).GetMediaPlayer().Position = ((SystemMediaPlayer)playEngine).GetMediaPlayer().Position + TimeSpan.FromSeconds(5);
        }
    }
}

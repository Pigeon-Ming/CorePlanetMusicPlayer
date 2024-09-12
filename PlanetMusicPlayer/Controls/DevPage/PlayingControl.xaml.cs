using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

namespace PlanetMusicPlayer.Controls.DevPage
{
    public sealed partial class PlayingControl : UserControl
    {
        DispatcherTimer timer = new DispatcherTimer();
        bool init = true;
        public PlayingControl()
        {
            this.InitializeComponent();
            PlayQueue.PlayQueueChanged += PlayQueue_PlayQueueChangedAsync;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            
            ProgressSlider.Maximum = PlayCore.MainMediaPlayer.MediaPlayer.NaturalDuration.TotalSeconds;
            ProgressSlider.Value = PlayCore.MainMediaPlayer.MediaPlayer.Position.TotalSeconds;
        }


        private async void MediaPlayer_CurrentStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                
                if (PlayCore.MainMediaPlayer.MediaPlayer.CurrentState == Windows.Media.Playback.MediaPlayerState.Playing)
                    PlayAndPauseButtonIcon.Symbol = Symbol.Pause;
                else
                    PlayAndPauseButtonIcon.Symbol = Symbol.Play;
            });
            
        }

        private async void PlayQueue_PlayQueueChangedAsync(object sender, EventArgs e)
        {

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                timer.Start();
                if (init)
                    PlayCore.MainMediaPlayer.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                Music music = PlayCore.GetPlayingMusic();
                MusicNameTextBlock.Text = music.Title;
                MessageTextBlock.Text = music.Artist + "-" + music.Album;
            });
        }

        private void PlayModeButton_Click(object sender, RoutedEventArgs e)
        {
            switch (PlayCore.playMode)
            {
                case PlayCore.PlayMode.LoopAll:
                    PlayCore.SetPlayMode(PlayCore.PlayMode.Shuffle);
                    PlayModeButton_Icon.Symbol = Symbol.Shuffle;
                    break;
                case PlayCore.PlayMode.Shuffle:
                    PlayCore.SetPlayMode(PlayCore.PlayMode.Single);
                    PlayModeButton_Icon.Symbol = Symbol.RepeatOne;
                    break;
                case PlayCore.PlayMode.Single:
                    PlayCore.SetPlayMode(PlayCore.PlayMode.Reverse);
                    PlayModeButton_Icon.Symbol = Symbol.Up;
                    break;
                case PlayCore.PlayMode.Reverse:
                    PlayCore.SetPlayMode(PlayCore.PlayMode.LoopAll);
                    PlayModeButton_Icon.Symbol = Symbol.RepeatAll;
                    break;
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            PlayCore.PreviousMusic();
        }

        private void PlayAndPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if(PlayCore.MainMediaPlayer.MediaPlayer == null)
                return;
            if (PlayCore.MainMediaPlayer.MediaPlayer.CurrentState == Windows.Media.Playback.MediaPlayerState.Playing)
                PlayCore.Pause();
            else
                PlayCore.Play();

        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            PlayCore.NextMusic();
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

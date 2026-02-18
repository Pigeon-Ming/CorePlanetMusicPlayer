using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Models;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace CorePlanetMusicPlayer.App
{
    public class LyricService
    {
        static readonly DispatcherTimer ServiceTimer = new DispatcherTimer();

        public static List<Lyric> Lyrics = new List<Lyric>();

        public static event EventHandler<int> CurrentLyricChanged;
        public static event EventHandler LyricsChanged;

        private static int currentIndex;
        public static int CurrentIndex 
        {
            get
            {
                return currentIndex;
            }
            private set 
            {
                if (currentIndex != value)
                {
                    currentIndex = value;
                    CurrentLyricChanged?.Invoke(null , value);
                }
            } 
        }

        static IPlayEngine PlayEngine;

        /// <summary>
        /// 初始化歌词服务
        /// </summary>
        /// <param name="playEngine"></param>
        public static void InitService(IPlayEngine playEngine)
        {
            PlayEngine = playEngine;
            PlayEngine.PlayingChanging += PlayEngine_PlayingChanging;
            PlayEngine.PlayingChanged += PlayEngine_PlayingChanged;

            ServiceTimer.Interval = TimeSpan.FromMilliseconds(500);
            ServiceTimer.Tick += ServiceTimer_Tick;
            
        }

        private static async void PlayEngine_PlayingChanging(object sender, Windows.Media.Playback.CurrentMediaPlaybackItemChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                ServiceTimer.Stop();
                Lyrics?.Clear();
                CurrentIndex = -1;
            });
        }

        private static async void PlayEngine_PlayingChanged(object sender, Windows.Media.Playback.CurrentMediaPlaybackItemChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await GetLyricAsync();
            });
        }

        /// <summary>
        /// 加载当前歌曲的歌词
        /// </summary>
        /// <returns></returns>
        private static async Task GetLyricAsync()
        {
            IMusic music = PlayEngine.GetCurrentMusic();
            if (music is null) return;
            Lyrics = await LyricHelper.GetLyricByMusicAsync(music);
            LyricsChanged?.Invoke(null, new EventArgs());
            ServiceTimer.Start();
        }

        /// <summary>
        /// 使用文件选择器为某一首音乐选取一个歌词文件
        /// </summary>
        /// <param name="music">选取歌词的音乐项</param>
        /// <param name="saveToData">是否保存到应用内部文件夹</param>
        /// <returns></returns>
        public static async Task<bool> PickLyricFileForCurrentMusicAsync(bool? saveToData = true)
        {
            IMusic music = PlayEngine.GetCurrentMusic();
            if (music is null) return false;
            List<Lyric> lyrics = await LyricHelper.PickLyricFileAsync(music, saveToData);
            if (lyrics is null)
            {
                return false;
            }
            else
            {
                Lyrics = lyrics;
                LyricsChanged?.Invoke(null, new EventArgs());
                return true;
            }
            
        }

        /// <summary>
        /// 歌词服务主Timer事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ServiceTimer_Tick(object sender, object e)
        {
            CurrentIndex = LyricHelper.GetCurrentLyricIndex(Lyrics, PlayEngine.GetPlayProgress());
        }
    }
}

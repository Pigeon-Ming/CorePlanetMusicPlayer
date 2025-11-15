using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml;

namespace CorePlanetMusicPlayer.App
{
    public class PlayRecordManager
    {
        private static IPlayEngine playEngine { get; set; }

        public static bool PlayRecordEnabled { get; private set; }

        public static void StartPlayRecord()
        {
            Debug.WriteLine("播放记录已启动。");
            if (PlayRecordEnabled)
                StopPlayRecord();
            playEngine.PlayingChanging += PlayEngine_PlayingChanging;
            PlayRecordEnabled = true;
        }

        private static async void PlayEngine_PlayingChanging(object sender, EventArgs e)
        {
            
            if (sender is MediaPlaybackItemChangedReason && (MediaPlaybackItemChangedReason)sender == MediaPlaybackItemChangedReason.EndOfStream)
            {
                await InsertPlayRecordAsync();
            }
        }

        private static async Task InsertPlayRecordAsync()
        {
            IMusic music = playEngine.GetPlayQueue().GetCurrentMusic();
            if (music == null)
                return;
            Debug.WriteLine("插入播放记录");
            await PlayRecordHelper.InsertDataAsync(new PlayRecord(music));
        }

        public static void StopPlayRecord()
        {
            Debug.WriteLine("播放记录已停止。");
            playEngine.PlayingChanging -= PlayEngine_PlayingChanging;
            PlayRecordEnabled = false;
        }

        public static async Task InitAsync(IPlayEngine p)
        {
            playEngine = p;
            await PlayRecordHelper.InitAsync();
        }
    }
}

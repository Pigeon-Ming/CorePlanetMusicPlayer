using CorePlanetMusicPlayer.Models;
using CorePlanetMusicPlayer.PlayCore;
using CorePlanetMusicPlayer6.Models;
using CorePlanetMusicPlayer6.ViewModels.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer6.ViewModels
{
    public class PlayingControlViewModel
    {
        public IPlayEngine PlayEngine { get; set; }

        public string PlayStateIconGlyph = "\uE768";
        public string CurrentPlayModeIconGlyph = "\uE8EE";

        public BitmapImage CurrentMusicCover { get; set; }

        public IMusic CurrentMusic { get; set; } = new Music { Title = "未在播放",Artist = "",Album = ""};

        public event EventHandler StateChanged;

        public PlayingControlViewModel(IPlayEngine playEngine)
        {
            PlayEngine = playEngine;

            PlayEngine.GetPlayQueue().CurrentIndexChanged += PlayQueue_CurrentIndexChanged;
            PlayEngine.GetPlayQueue().CurrentPlayModeChanged += PlayQueue_CurrentPlayModeChanged;
            PlayEngine.StateChanged += PlayEngine_StateChanged;
        }

        private void PlayEngine_StateChanged(object sender, EventArgs e)
        {
            PlayStateIconGlyph = GetPlayStateIconGlyph(PlayEngine.PlayState);
            StateChanged?.Invoke(this, e);
        }

        string GetPlayStateIconGlyph(PlayEngine.PlayState state)
        {
            switch(state)
            {
                case CorePlanetMusicPlayer.PlayCore.PlayEngine.PlayState.Playing:
                    return "\uE769";
                case CorePlanetMusicPlayer.PlayCore.PlayEngine.PlayState.Paused:
                    return "\uE768";
                case CorePlanetMusicPlayer.PlayCore.PlayEngine.PlayState.Buffering:
                    return "\uF16A";
                default:
                    return "";
            }
        }

        string GetCurrentPlayModeIconGlyph()
        {
            switch (PlayEngine.GetPlayQueue().CurrentPlayModeEnum)
            {
                case PlayQueue.PlayModeEnum.RepeatAll:
                    return "\uE8EE";
                case PlayQueue.PlayModeEnum.Shuffle:
                    return "\uE8B1";
                case PlayQueue.PlayModeEnum.RepeatOne:
                    return "\uE8ED";
                case PlayQueue.PlayModeEnum.Reverse:
                    return "\uE752";
                default:
                    return "\uE9CE";
            }

        }

        async Task GetCoverAsync()
        {
            ViewMusic viewMusic = await ViewMusicManager.FindViewMusicInViewMusicListAsync(ProgramData.ViewMusic,CurrentMusic,true);
            CurrentMusicCover = viewMusic.Cover;
            StateChanged?.Invoke(this, null);
        }

        private void PlayQueue_CurrentPlayModeChanged(object sender, EventArgs e)
        {
            CurrentPlayModeIconGlyph = GetCurrentPlayModeIconGlyph();
            StateChanged?.Invoke(this,e);
        }

        private void PlayQueue_CurrentIndexChanged(object sender, EventArgs e)
        {
            IMusic music = PlayEngine.GetPlayQueue().GetCurrentMusic();
            if (music == null)
                CurrentMusic = new Music { Title = "未在播放", Artist = "", Album = "" };
            else
                CurrentMusic = music;

            GetCoverAsync();

            StateChanged?.Invoke(this, e);
        }
    }
}

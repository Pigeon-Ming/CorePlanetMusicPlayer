using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.PlayCore
{
    public enum PlayState { Playing, Paused, Stopped, Buffering }

    public interface IPlayEngine
    {
        void Play();
        void Pause();

        void PlayPause();

        void Stop();
        void Next();
        void Previous();

        void PlayMusic(IMusic music,List<IMusic> newPlayQueue,int currentMusicIndex);

        double GetVolume();
        void SetVolume(double volume);

        TimeSpan GetPlayProgress();
        

        PlayState PlayState { get; set; }

        PlayQueue GetPlayQueue();

        event EventHandler PlayingEnded;

        event EventHandler StateChanged;

        event EventHandler PlayingChanging;

        event EventHandler PlayingChanged;
    }
}

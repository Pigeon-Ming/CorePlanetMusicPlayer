using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Playback;

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


        DeviceInformation GetSoundOutputDevice();
        void SetSoundOutputDevice(DeviceInformation deviceInformation);

        
        MediaPlaybackList SetMediaSource(int index, List<IMusic> newPlayQueue);

        TimeSpan GetPlayProgress();

        TimeSpan GetMediaDuration();
        PlayState PlayState { get; set; }

        PlayQueue GetPlayQueue();

        event EventHandler PlayingEnded;

        event EventHandler StateChanged;

        event EventHandler PlayingChanging;

        event EventHandler PlayingChanged;

        event EventHandler VolumeChanged;
    }
}

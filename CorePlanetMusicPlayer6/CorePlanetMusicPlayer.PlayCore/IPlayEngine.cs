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

        void SetPlayProgress(TimeSpan newProgress);

        TimeSpan GetMediaDuration();
        PlayState PlayState { get; set; }

        PlayQueue GetPlayQueue();

        IMusic GetCurrentMusic();

        event EventHandler PlayingEnded;

        event EventHandler StateChanged;

        event EventHandler<CurrentMediaPlaybackItemChangedEventArgs> PlayingChanging;

        event EventHandler<CurrentMediaPlaybackItemChangedEventArgs> PlayingChanged;

        event EventHandler VolumeChanged;

        /// <summary>
        /// 均衡器
        /// </summary>
        

        bool IsEqualizerSupported { get; }
        bool IsEqualizerEnabled { get; set; }
        int EqualizerBandCount { get; }
        double GetEqualizerBandFrequency(int bandIndex);
        float GetEqualizerGain(int bandIndex);
        void SetEqualizerGain(int bandIndex, float gain);
        void ResetEqualizer();
        float[] GetAllEqualizerGains();
        void SetAllEqualizerGains(IEnumerable<float> gains);
    }
}

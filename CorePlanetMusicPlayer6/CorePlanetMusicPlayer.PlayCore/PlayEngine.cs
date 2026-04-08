using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.Devices.Enumeration;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.Core;
using Windows.Media.Effects;
using Windows.Media.Playback;
using Windows.Media.Render;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer.PlayCore
{

    //public class SystemMediaPlayerElement : IPlayEngine
    //{
    //    MediaPlayerElement MediaPlayerElement;

    //    public event EventHandler PlayingEnded;
    //    public event EventHandler StateChanged;
    //    public event EventHandler PlayingChanging;
    //    public event EventHandler PlayingChanged;

    //    public void Next()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Pause()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Play()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void PlayMusic()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Previous()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void Stop()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class SystemMediaPlayer : IPlayEngine
    {
        private MediaPlayer MediaPlayer { get; }

        private AudioGraph _audioGraph;
        private readonly SemaphoreSlim _audioGraphSemaphore = new SemaphoreSlim(1, 1);

        SystemMediaTransportControls SMTCControls { get; set; }

        public PlayState PlayState { get; set; }

        public PlayQueue PlayQueue { get; set; }

        public event EventHandler PlayingEnded;
        public event EventHandler StateChanged;

        public event EventHandler<CurrentMediaPlaybackItemChangedEventArgs> PlayingChanging;
        public event EventHandler<CurrentMediaPlaybackItemChangedEventArgs> PlayingChanged;

        public event EventHandler VolumeChanged;

        private readonly float[] _equalizerGains = new float[10];
        private bool _isEqualizerEnabled;
        private bool _isEqualizerSupported = true;
        private const float MaxEqualizerBoostDb = 9f;

        public SystemMediaPlayer()
        {
            MediaPlayer = new MediaPlayer();
            MediaPlayer.SystemMediaTransportControls.IsEnabled = false;
            SMTCControls = MediaPlayer.SystemMediaTransportControls;//SystemMediaTransportControls.GetForCurrentView(); ;

            PlayQueue = new PlayQueue(this);

            //SMTCConrtols.DisplayUpdater.Type = MediaPlaybackType.Music;

            MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            MediaPlayer.BufferingStarted += MediaPlayer_BufferingStarted;
        }



        private void MediaPlayer_BufferingStarted(MediaPlayer sender, object args)
        {
            Debug.WriteLine("BufferingStarted!");
        }

        public MediaPlayer GetMediaPlayer()
        {
            Debug.WriteLine("GetMediaPlayer方法仅供开发使用！");
            return MediaPlayer;
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            Debug.WriteLine("MediaOpened");
            var mediaPlaybackList = MediaPlayer.Source as MediaPlaybackList;
            if (mediaPlaybackList != null)
            {
                int newIndex = (int)mediaPlaybackList.CurrentItemIndex;
                if (PlayQueue.CurrentIndex != newIndex)
                    PlayQueue.SetCurrentIndex(newIndex);
            }
            PlayingChanged?.Invoke(this, null);
        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (MediaPlayer.CurrentState)
            {
                case MediaPlayerState.Playing:
                    PlayState = PlayState.Playing;
                    break;
                case MediaPlayerState.Paused:
                    PlayState = PlayState.Paused;
                    break;
                case MediaPlayerState.Stopped:
                    PlayState = PlayState.Stopped;
                    break;
                case MediaPlayerState.Buffering:
                    PlayState = PlayState.Buffering;
                    break;
            }
            if (IsEqualizerEnabled)
                _ = EnsureAudioGraphPlaybackStateAsync();
            StateChanged?.Invoke(this, null);
        }

        public PlayQueue GetPlayQueue()
        {
            return PlayQueue;
        }

        public void Next()
        {
            PlayQueue.Next();
            playMusic(PlayQueue.CurrentIndex);
            SMTCManager.UpdateSMTC(((MediaPlaybackList)MediaPlayer.Source).CurrentItem, PlayQueue.GetCurrentMusic());
            //SMTCManager.UpdateSMTC(SMTCConrtols, PlayQueue.GetCurrentMusic());
        }

        public void Pause()
        {
            MediaPlayer.Pause();
            if (IsEqualizerEnabled && _mediaInputNode != null)
                _mediaInputNode.Stop();
        }

        public void Play()
        {
            MediaPlayer.Play();
            if (IsEqualizerEnabled)
                _ = EnsureAudioGraphPlaybackStateAsync();
        }

        public void PlayPause()
        {
            if (PlayState == PlayState.Playing)
            {
                MediaPlayer.Pause();
                if (IsEqualizerEnabled && _mediaInputNode != null)
                    _mediaInputNode.Stop();
            }
            else
            {
                MediaPlayer.Play();
                if (IsEqualizerEnabled)
                    _ = EnsureAudioGraphPlaybackStateAsync();
            }
        }

        //private void playMusic(MediaPlaybackList mediaPlaybackList,int index)
        //{

        //    if (mediaPlaybackList != null)
        //    {
        //        mediaPlaybackList.StartingItem = mediaPlaybackList.Items[index];
        //        MediaPlayer.Play();
        //    }

        //}

        private void playMusic(int index)
        {
            MediaPlaybackList mediaPlaybackList = (MediaPlaybackList)MediaPlayer.Source;
            if (mediaPlaybackList is null)
                return;
            mediaPlaybackList.MoveTo((uint)index);
            MediaPlayer.Play();
            if (IsEqualizerEnabled)
                _ = EnsureAudioGraphPlaybackStateAsync();
            PlayingChanged?.Invoke(this, null);
        }

        public void PlayMusic(IMusic music, List<IMusic> newPlayQueue, int currentMusicIndex)
        {

            MediaPlaybackList mediaPlaybackList = SetMediaSource(currentMusicIndex, newPlayQueue);
            PlayQueue.SetQueue(newPlayQueue);
            PlayQueue.SetCurrentIndex(currentMusicIndex);
            playMusic(currentMusicIndex);
            SMTCManager.UpdateSMTC(mediaPlaybackList.Items[currentMusicIndex], PlayQueue.GetCurrentMusic());
            //SMTCManager.UpdateSMTC(SMTCConrtols, PlayQueue.GetCurrentMusic());
        }

        private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            Debug.WriteLine("Reason: " + args.Reason);
            PlayingChanging?.Invoke(this, args);

            if ((int)sender.CurrentItemIndex >= PlayQueue.NormalQueue.Count)
                return;
            if (PlayQueue.CurrentIndex != (int)sender.CurrentItemIndex)
                PlayQueue.SetCurrentIndex((int)sender.CurrentItemIndex);
            MediaPlaybackList mediaPlaybackList = (MediaPlaybackList)MediaPlayer.Source;
            if (mediaPlaybackList.Items.Count <= sender.CurrentItemIndex)
                return;
            SMTCManager.UpdateSMTC(mediaPlaybackList.Items[(int)sender.CurrentItemIndex], PlayQueue.GetCurrentMusic());
            if (IsEqualizerEnabled)
                _ = RebuildAudioGraphInputNodeForCurrentItemAsync();
            Debug.WriteLine($"CurrentItemChanged:{PlayQueue.CurrentIndex}");
            PlayingChanged?.Invoke(this, args);
        }

        public MediaPlaybackList SetMediaSource(int index, List<IMusic> newPlayQueue)
        {
            if (MediaPlayer.Source != null)
            {
                ((MediaPlaybackList)MediaPlayer.Source).CurrentItemChanged -= MediaPlaybackList_CurrentItemChanged;
            }
            MediaPlaybackList mediaPlaybackList = GetMediaPlayBackListFromIMusicList(newPlayQueue);
            mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;

            if (mediaPlaybackList == null)
                return null;
            mediaPlaybackList.StartingItem = mediaPlaybackList.Items[index];
            MediaPlayer.Source = mediaPlaybackList;
            if (IsEqualizerEnabled)
                _ = RebuildAudioGraphInputNodeForCurrentItemAsync();
            return mediaPlaybackList;
        }

        private MediaPlaybackList GetMediaPlayBackListFromIMusicList(List<IMusic> musicList)
        {
            MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
            //mediaPlaybackList.Items.Clear();
            //mediaPlaybackList.CurrentItemChanged
            //            mediaPlaybackList.Items.Clear
            foreach (IMusic music in musicList)
            {
                MediaPlaybackItem mediaPlaybackItem = GetMediaPlayBackItemFromIMusic(music);
                if (mediaPlaybackItem != null)
                    mediaPlaybackList.Items.Add(mediaPlaybackItem);
            }
            return mediaPlaybackList;
        }

        private List<MediaPlaybackItem> GetMediaPlayBackItemListFromIMusicList(List<IMusic> musicList)
        {
            //MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
            List<MediaPlaybackItem> items = new List<MediaPlaybackItem>();
            //mediaPlaybackList.CurrentItemChanged
            //            mediaPlaybackList.Items.Clear
            foreach (IMusic music in musicList)
            {
                MediaPlaybackItem mediaPlaybackItem = GetMediaPlayBackItemFromIMusic(music);
                if (mediaPlaybackItem != null)
                    items.Add(mediaPlaybackItem);
            }
            return items;
        }

        private MediaPlaybackItem GetMediaPlayBackItemFromIMusic(IMusic music)
        {
            var mediaSource = CreateMediaSourceFromIMusic(music);
            if (mediaSource == null)
                return null;
            return new MediaPlaybackItem(mediaSource);
        }

        private MediaSource CreateMediaSourceFromIMusic(IMusic music)
        {
            if (music == null)
                return null;
            if (music is LocalMusic)
                return MediaSource.CreateFromStorageFile(((LocalMusic)music).StorageFile);
            if (music is StreamMusic)
                return MediaSource.CreateFromUri(new Uri(((StreamMusic)music).Url));
            if (music is RemovableMusic)
                return MediaSource.CreateFromStorageFile(((RemovableMusic)music).StorageFile);
            return null;
        }

        public void Previous()
        {
            PlayQueue.Previous();
            playMusic(PlayQueue.CurrentIndex);
            SMTCManager.UpdateSMTC(((MediaPlaybackList)MediaPlayer.Source).CurrentItem, PlayQueue.GetCurrentMusic());
            // SMTCManager.UpdateSMTC(SMTCControls, PlayQueue.GetCurrentMusic());
        }

        public void Stop()
        {
            MediaPlayer.Pause();
            if (_mediaInputNode != null)
            {
                _mediaInputNode.Stop();
                try
                {
                    _mediaInputNode.Seek(TimeSpan.Zero);
                }
                catch
                {
                }
            }
            PlayQueue.ClearPlayQueue();
        }

        public double GetVolume()
        {
            //VolumeChanged?.Invoke(this,null);
            return MediaPlayer.Volume;
        }

        public void SetVolume(double volume)
        {
            MediaPlayer.Volume = volume;
            UpdateAudioGraphOutputGain();
            VolumeChanged?.Invoke(this, null);
        }

        public DeviceInformation GetSoundOutputDevice()
        {
            return MediaPlayer.AudioDevice;
        }

        public void SetSoundOutputDevice(DeviceInformation deviceInformation)
        {
            MediaPlayer.AudioDevice = deviceInformation;
            if (IsEqualizerEnabled)
                _ = RebuildAudioGraphInputNodeForCurrentItemAsync();
        }

        public TimeSpan GetPlayProgress()
        {
            //MediaPlayer.PlaybackSession.BufferingProgress
            return MediaPlayer.Position;
        }

        public void SetPlayProgress(TimeSpan newProgress)
        {
            MediaPlayer.Position = newProgress;
            if (_mediaInputNode != null)
            {
                try
                {
                    _mediaInputNode.Seek(newProgress);
                }
                catch
                {
                }
            }
        }

        public TimeSpan GetMediaDuration()
        {
            return MediaPlayer.NaturalDuration;
        }

        public IMusic GetCurrentMusic()
        {
            if (PlayQueue is null)
                return null;
            return PlayQueue.GetCurrentMusic();
        }

        /// <summary>
        /// 均衡器
        /// </summary>
        //private EqualizerEffectDefinition _equalizer;

        private AudioDeviceOutputNode _deviceOutputNode;

        private MediaSourceAudioInputNode _mediaInputNode;

        private List<EqualizerEffectDefinition> _eqDefs = new List<EqualizerEffectDefinition>();

        private LimiterEffectDefinition _limiter;

        private List<EqualizerBand> _bands = new List<EqualizerBand>(10);

        private double[] _freqCenters = new double[] { 32, 64, 125, 250, 500, 1000, 2000, 4000, 8000, 16000 };

        private async Task EnsureAudioGraphAsync()
        {
            if (_audioGraph != null && _deviceOutputNode != null)
                return;

            var settings = new AudioGraphSettings(AudioRenderCategory.Media);
            var graphResult = await AudioGraph.CreateAsync(settings);
            if (graphResult.Status != AudioGraphCreationStatus.Success)
            {
                _isEqualizerSupported = false;
                return;
            }

            _audioGraph = graphResult.Graph;
            var outResult = await _audioGraph.CreateDeviceOutputNodeAsync();
            if (outResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                _isEqualizerSupported = false;
                _audioGraph.Dispose();
                _audioGraph = null;
                return;
            }

            _deviceOutputNode = outResult.DeviceOutputNode;
            _audioGraph.Start();
        }

        private static double DbToGain(double db)
        {
            return Math.Pow(10.0, db / 20.0);
        }

        public bool IsEqualizerSupported => _isEqualizerSupported;

        public bool IsEqualizerEnabled
        {
            get => _isEqualizerEnabled;
            set
            {
                if (_isEqualizerEnabled == value)
                    return;

                _isEqualizerEnabled = value;
                _ = ApplyEqualizerModeAsync();
            }
        }

        public int EqualizerBandCount => 10;

        public double GetEqualizerBandFrequency(int bandIndex)
        {
            if (bandIndex < 0 || bandIndex >= EqualizerBandCount)
                throw new ArgumentOutOfRangeException(nameof(bandIndex));
            return _freqCenters[bandIndex];
        }

        public float GetEqualizerGain(int bandIndex)
        {
            if (bandIndex < 0 || bandIndex >= EqualizerBandCount)
                throw new ArgumentOutOfRangeException(nameof(bandIndex));
            return _equalizerGains[bandIndex];
        }

        public void SetEqualizerGain(int bandIndex, float gain)
        {
            if (bandIndex < 0 || bandIndex >= EqualizerBandCount)
                throw new ArgumentOutOfRangeException(nameof(bandIndex));
            _equalizerGains[bandIndex] = NormalizeGain(gain);
            if (IsEqualizerEnabled && _bands.Count > bandIndex)
                ApplyBandGain(bandIndex);
        }

        public void ResetEqualizer()
        {
            for (int i = 0; i < EqualizerBandCount; i++)
                _equalizerGains[i] = 0f;
            ApplyAllBandGains();
        }

        public float[] GetAllEqualizerGains()
        {
            return _equalizerGains.ToArray();
        }

        public void SetAllEqualizerGains(IEnumerable<float> gains)
        {
            if (gains == null)
                return;

            int i = 0;
            foreach (var gain in gains)
            {
                if (i >= EqualizerBandCount)
                    break;
                _equalizerGains[i] = NormalizeGain(gain);
                i++;
            }

            ApplyAllBandGains();
        }

        private float NormalizeGain(float gain)
        {
            if (gain > MaxEqualizerBoostDb)
                return MaxEqualizerBoostDb;
            if (gain < -12f)
                return -12f;
            return gain;
        }

        private void ApplyBandGain(int bandIndex)
        {
            if (_bands == null || bandIndex < 0 || bandIndex >= _bands.Count)
                return;

            try
            {
                _bands[bandIndex].Gain = DbToGain(_equalizerGains[bandIndex]);
            }
            catch
            {
            }
        }

        private void ApplyAllBandGains()
        {
            if (_bands == null || _bands.Count == 0)
                return;

            int count = Math.Min(_bands.Count, EqualizerBandCount);
            for (int i = 0; i < count; i++)
                ApplyBandGain(i);
            UpdateAudioGraphOutputGain();
        }

        private void UpdateAudioGraphOutputGain()
        {
            if (_mediaInputNode == null)
                return;

            if (!IsEqualizerEnabled)
            {
                _mediaInputNode.OutgoingGain = MediaPlayer.Volume;
                return;
            }

            float maxPositiveGainDb = 0f;
            for (int i = 0; i < EqualizerBandCount; i++)
            {
                if (_equalizerGains[i] > maxPositiveGainDb)
                    maxPositiveGainDb = _equalizerGains[i];
            }

            var headroomGain = DbToGain(-maxPositiveGainDb);
            var compensatedGain = MediaPlayer.Volume * headroomGain;
            if (compensatedGain < 0)
                compensatedGain = 0;
            if (compensatedGain > 1)
                compensatedGain = 1;
            _mediaInputNode.OutgoingGain = compensatedGain;
        }

        private async Task ApplyEqualizerModeAsync()
        {
            if (!IsEqualizerEnabled)
            {
                MediaPlayer.IsMuted = false;
                await _audioGraphSemaphore.WaitAsync();
                try
                {
                    if (_mediaInputNode != null)
                    {
                        _mediaInputNode.Stop();
                        _mediaInputNode.Dispose();
                        _mediaInputNode = null;
                    }
                    _eqDefs.Clear();
                    _bands.Clear();
                }
                finally
                {
                    _audioGraphSemaphore.Release();
                }
                return;
            }

            await RebuildAudioGraphInputNodeForCurrentItemAsync();
            await EnsureAudioGraphPlaybackStateAsync();
        }

        private async Task EnsureAudioGraphPlaybackStateAsync()
        {
            if (!IsEqualizerEnabled)
                return;

            if (_mediaInputNode == null)
                await RebuildAudioGraphInputNodeForCurrentItemAsync();

            if (_mediaInputNode == null)
                return;

            MediaPlayer.IsMuted = true;
            UpdateAudioGraphOutputGain();

            if (MediaPlayer.CurrentState == MediaPlayerState.Playing || MediaPlayer.CurrentState == MediaPlayerState.Buffering)
                _mediaInputNode.Start();
            else
                _mediaInputNode.Stop();
        }

        private MediaSource GetCurrentMediaSource()
        {
            var currentMusic = PlayQueue?.GetCurrentMusic();
            return CreateMediaSourceFromIMusic(currentMusic);
        }

        private async Task RebuildAudioGraphInputNodeForCurrentItemAsync()
        {
            if (!IsEqualizerEnabled)
                return;

            var mediaSource = GetCurrentMediaSource();
            if (mediaSource == null)
                return;

            await _audioGraphSemaphore.WaitAsync();
            try
            {
                await EnsureAudioGraphAsync();
                if (_audioGraph == null || _deviceOutputNode == null)
                    return;

                if (_mediaInputNode != null)
                {
                    _mediaInputNode.Stop();
                    _mediaInputNode.Dispose();
                    _mediaInputNode = null;
                }

                var result = await _audioGraph.CreateMediaSourceAudioInputNodeAsync(mediaSource);
                if (result.Status != MediaSourceAudioInputNodeCreationStatus.Success)
                {
                    _isEqualizerSupported = false;
                    return;
                }

                _mediaInputNode = result.Node;
                _mediaInputNode.AddOutgoingConnection(_deviceOutputNode);
                _eqDefs.Clear();
                _bands.Clear();

                int total = 0;
                while (total < EqualizerBandCount)
                {
                    var eq = new EqualizerEffectDefinition(_audioGraph);
                    _eqDefs.Add(eq);
                    _mediaInputNode.EffectDefinitions.Add(eq);
                    _mediaInputNode.EnableEffectsByDefinition(eq);
                    total += eq.Bands.Count;
                }

                foreach (var eq in _eqDefs)
                {
                    foreach (var band in eq.Bands)
                    {
                        _bands.Add(band);
                        if (_bands.Count >= EqualizerBandCount)
                            break;
                    }
                    if (_bands.Count >= EqualizerBandCount)
                        break;
                }

                for (int i = 0; i < _bands.Count && i < _freqCenters.Length; i++)
                {
                    try
                    {
                        _bands[i].FrequencyCenter = _freqCenters[i];
                    }
                    catch
                    {
                    }
                }

                ApplyAllBandGains();
                UpdateAudioGraphOutputGain();

                if (_limiter != null && _mediaInputNode.EffectDefinitions.Contains(_limiter))
                    _mediaInputNode.EffectDefinitions.Remove(_limiter);
                _limiter = new LimiterEffectDefinition(_audioGraph);
                _mediaInputNode.EffectDefinitions.Add(_limiter);
                _mediaInputNode.EnableEffectsByDefinition(_limiter);

                MediaPlayer.IsMuted = true;
            }
            finally
            {
                _audioGraphSemaphore.Release();
            }
        }
    }
}

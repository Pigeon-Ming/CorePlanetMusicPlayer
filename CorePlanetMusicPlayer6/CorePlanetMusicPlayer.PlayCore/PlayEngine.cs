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
using Windows.Devices.Enumeration;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
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
        MediaPlayer MediaPlayer { get; }

        SystemMediaTransportControls SMTCControls { get; set; }

        public PlayState PlayState { get; set; }

        public PlayQueue PlayQueue { get; set; } 

        public event EventHandler PlayingEnded;
        public event EventHandler StateChanged;

        public event EventHandler PlayingChanging;
        public event EventHandler PlayingChanged;

        public event EventHandler VolumeChanged;

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
            int newIndex = (int)((MediaPlaybackList)MediaPlayer.Source).CurrentItemIndex;
            if (PlayQueue.CurrentIndex != newIndex)
                PlayQueue.SetCurrentIndex(newIndex);
            PlayingChanged?.Invoke(this, null);
        }

        private void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch(MediaPlayer.CurrentState)
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
            StateChanged?.Invoke(this,null);
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
        }

        public void Play()
        {
            MediaPlayer.Play();
        }

        public void PlayPause()
        {
            if (PlayState == PlayState.Playing)
                MediaPlayer.Pause();
            else
                MediaPlayer.Play();
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
            mediaPlaybackList.MoveTo((uint)index);
            MediaPlayer.Play();
            PlayingChanged?.Invoke(this,null);
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
            Debug.WriteLine("Reason: "+ args.Reason);
            PlayingChanging?.Invoke(this, new EventArgs());
            
            if ((int)sender.CurrentItemIndex >= PlayQueue.NormalQueue.Count)
                return;
            if (PlayQueue.CurrentIndex != (int)sender.CurrentItemIndex)
                PlayQueue.SetCurrentIndex((int)sender.CurrentItemIndex);
            MediaPlaybackList mediaPlaybackList = (MediaPlaybackList)MediaPlayer.Source;
            if (mediaPlaybackList.Items.Count <= sender.CurrentItemIndex)
                return;
            SMTCManager.UpdateSMTC(mediaPlaybackList.Items[(int)sender.CurrentItemIndex], PlayQueue.GetCurrentMusic());
            Debug.WriteLine($"CurrentItemChanged:{PlayQueue.CurrentIndex}");
            PlayingChanged?.Invoke(this, new EventArgs());
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
            return mediaPlaybackList;
        }

        private MediaPlaybackList GetMediaPlayBackListFromIMusicList(List<IMusic>musicList)
        {
            MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
            //mediaPlaybackList.Items.Clear();
            //mediaPlaybackList.CurrentItemChanged
//            mediaPlaybackList.Items.Clear
            foreach(IMusic music in musicList)
            {
                MediaPlaybackItem mediaPlaybackItem = GetMediaPlayBackItemFromIMusic(music);
                if(mediaPlaybackItem != null)
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
            if (music == null)
                return null;
            MediaPlaybackItem mediaPlaybackItem;
            if (music is LocalMusic)
            {
                MediaSource mediaSource = MediaSource.CreateFromStorageFile(((LocalMusic)music).StorageFile);
                mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                return mediaPlaybackItem;
            }
            else if(music is StreamMusic)
            {
                MediaSource mediaSource = MediaSource.CreateFromUri(new Uri(((StreamMusic)music).Url));
                mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                return mediaPlaybackItem;
            } else if (music is RemovableMusic)
            {
                MediaSource mediaSource = MediaSource.CreateFromStorageFile(((RemovableMusic)music).StorageFile);
                mediaPlaybackItem = new MediaPlaybackItem(mediaSource);
                return mediaPlaybackItem;
            }
            else return null;
        }

        public void Previous()
        {
            PlayQueue.Previous();
            playMusic(PlayQueue.CurrentIndex);
            SMTCManager.UpdateSMTC(((MediaPlaybackList)MediaPlayer.Source).CurrentItem, PlayQueue.GetCurrentMusic());
            // SMTCManager.UpdateSMTC(SMTCConrtols, PlayQueue.GetCurrentMusic());
        }

        public void Stop()
        {
            MediaPlayer.Pause();
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
            VolumeChanged?.Invoke(this,null);
        }

        public DeviceInformation GetSoundOutputDevice()
        {
            return MediaPlayer.AudioDevice;
        }

        public void SetSoundOutputDevice(DeviceInformation deviceInformation)
        {
            MediaPlayer.AudioDevice = deviceInformation;
        }

        public TimeSpan GetPlayProgress()
        {
            //MediaPlayer.PlaybackSession.BufferingProgress
            return MediaPlayer.Position;
        }

        public TimeSpan GetMediaDuration()
        {
            return MediaPlayer.NaturalDuration;
        }
    }
}

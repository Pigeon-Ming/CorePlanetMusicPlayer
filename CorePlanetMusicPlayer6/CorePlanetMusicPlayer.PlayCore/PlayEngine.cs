using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        SystemMediaTransportControls SMTCConrtols { get; set; }

        public PlayState PlayState { get; set; }

        public PlayQueue PlayQueue { get; set; } = new PlayQueue();

        public event EventHandler PlayingEnded;
        public event EventHandler StateChanged;

        /// <summary>
        /// （未实现)
        /// </summary>
        public event EventHandler PlayingChanging;
        public event EventHandler PlayingChanged;

        public event EventHandler VolumeChanged;

        public SystemMediaPlayer()
        {
            MediaPlayer = new MediaPlayer();
            MediaPlayer.SystemMediaTransportControls.IsEnabled = false;
            SMTCConrtols = MediaPlayer.SystemMediaTransportControls;//SystemMediaTransportControls.GetForCurrentView(); ;
            
            //SMTCConrtols.DisplayUpdater.Type = MediaPlaybackType.Music;
            
            MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
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

        private void playMusic(MediaPlaybackList mediaPlaybackList,int index)
        {
            
            if (mediaPlaybackList != null)
            {
                mediaPlaybackList.StartingItem = mediaPlaybackList.Items[index];
                MediaPlayer.Source = mediaPlaybackList;
                MediaPlayer.Play();
            }
            
        }

        private void playMusic(int index)
        {
            MediaPlaybackList mediaPlaybackList = (MediaPlaybackList)MediaPlayer.Source;
            mediaPlaybackList.MoveTo((uint)index);
            PlayingChanged?.Invoke(this,null);
        }

        public void PlayMusic(IMusic music, List<IMusic> newPlayQueue, int currentMusicIndex)
        {
            if(MediaPlayer.Source != null)
            {
                ((MediaPlaybackList)MediaPlayer.Source).CurrentItemChanged -= MediaPlaybackList_CurrentItemChanged;
            }
            MediaPlaybackList mediaPlaybackList = GetMediaPlayBackListFromIMusicList(newPlayQueue);
            mediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;

            if (mediaPlaybackList == null)
                return;
            
            PlayQueue.SetQueue(newPlayQueue);
            PlayQueue.SetCurrentIndex(currentMusicIndex);
            playMusic(mediaPlaybackList,currentMusicIndex);
            SMTCManager.UpdateSMTC(mediaPlaybackList.Items[currentMusicIndex], PlayQueue.GetCurrentMusic());
            //SMTCManager.UpdateSMTC(SMTCConrtols, PlayQueue.GetCurrentMusic());
        }

        private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            PlayQueue.SetCurrentIndex((int)sender.CurrentItemIndex);
            SMTCManager.UpdateSMTC(((MediaPlaybackList)MediaPlayer.Source).Items[(int)sender.CurrentItemIndex], PlayQueue.GetCurrentMusic());
            Debug.WriteLine($"CurrentItemChanged:{PlayQueue.CurrentIndex}");
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
                //To-Do:OnlineMusic的播放
                return null;
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
            VolumeChanged?.Invoke(this,null);
            return MediaPlayer.Volume;
        }

        public void SetVolume(double volume)
        {
            MediaPlayer.Volume = volume;
            VolumeChanged?.Invoke(this,null);
        }

        public TimeSpan GetPlayProgress()
        {
            //MediaPlayer.PlaybackSession.BufferingProgress
            return MediaPlayer.Position;
        }
    }
}

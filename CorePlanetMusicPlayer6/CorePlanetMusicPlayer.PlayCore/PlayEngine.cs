using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
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

    public class PlayEngine
    {
        public enum PlayState { Playing,Paused,Stoped, Buffering }
    }

    public class SystemMediaPlayer : IPlayEngine
    {
        MediaPlayer MediaPlayer = new MediaPlayer();

        SystemMediaTransportControls SMTCConrtols { get; set; }

        public PlayEngine.PlayState PlayState { get; set; }

        public PlayQueue PlayQueue { get; set; } = new PlayQueue();

        public event EventHandler PlayingEnded;
        public event EventHandler StateChanged;
        public event EventHandler PlayingChanging;
        public event EventHandler PlayingChanged;

        public event EventHandler VolumeChanged;

        public SystemMediaPlayer()
        {
            SMTCConrtols = MediaPlayer.SystemMediaTransportControls;
            SMTCConrtols.DisplayUpdater.Type = MediaPlaybackType.Music;
            MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
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
                    PlayState = PlayEngine.PlayState.Playing;
                    break;
                case MediaPlayerState.Paused:
                    PlayState = PlayEngine.PlayState.Paused;
                    break;
                case MediaPlayerState.Stopped:
                    PlayState = PlayEngine.PlayState.Stoped;
                    break;
                case MediaPlayerState.Buffering:
                    PlayState = PlayEngine.PlayState.Buffering;
                    break;
            }
            StateChanged.Invoke(this,null);
        }

        public PlayQueue GetPlayQueue()
        {
            return PlayQueue;
        }

        public void Next()
        {
            PlayQueue.Next();
            playMusic(PlayQueue.CurrentIndex);
            SMTCManager.UpdateSMTC(SMTCConrtols, PlayQueue.GetCurrentMusic());
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
            if (PlayState == PlayEngine.PlayState.Playing)
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
        }

        public void PlayMusic(IMusic music, List<IMusic> newPlayQueue, int currentMusicIndex)
        {
            
            MediaPlaybackList mediaPlaybackList = GetMediaPlayBackListFromIMusicList(newPlayQueue);
            if (mediaPlaybackList == null)
                return;
            
            PlayQueue.SetQueue(newPlayQueue);
            PlayQueue.SetCurrentIndex(currentMusicIndex);
            playMusic(mediaPlaybackList,currentMusicIndex);
            SMTCManager.UpdateSMTC(SMTCConrtols, PlayQueue.GetCurrentMusic());
        }

        private MediaPlaybackList GetMediaPlayBackListFromIMusicList(List<IMusic>musicList)
        {
            MediaPlaybackList mediaPlaybackList = new MediaPlaybackList();
            foreach(IMusic music in musicList)
            {
                MediaPlaybackItem mediaPlaybackItem = GetMediaPlayBackItemFromIMusic(music);
                if(mediaPlaybackItem != null)
                    mediaPlaybackList.Items.Add(mediaPlaybackItem);
            }
            return mediaPlaybackList;
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
            else if(music is OnlineMusic)
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
            SMTCManager.UpdateSMTC(SMTCConrtols, PlayQueue.GetCurrentMusic());
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
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using static CorePlanetMusicPlayer.Models.Library;

namespace CorePlanetMusicPlayer.Models
{
    public class PlayCore
    {
        public static MediaPlayerElement MainMediaPlayer { get; set; } = new MediaPlayerElement();

        public enum PlayMode { LoopAll, Shuffle, Single, Reverse};
        public static PlayMode playMode { get; private set; } = PlayMode.LoopAll;
        public static void InitPlayCore()
        {
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsPauseEnabled = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsPlayEnabled = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsPreviousEnabled = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsNextEnabled = true;
            MainMediaPlayer.MediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            MainMediaPlayer.MediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            MainMediaPlayer.MediaPlayer.CommandManager.NextReceived += (a, b)=> { NextMusic(); };
            MainMediaPlayer.MediaPlayer.CommandManager.PreviousReceived += (a, b) => { PreviousMusic(); };
            MainMediaPlayer.MediaPlayer.MediaEnded += (a, b) => { NextMusic(); };
        }

        public static void Pause()
        {
            if(MainMediaPlayer.MediaPlayer != null)
            {
                MainMediaPlayer.MediaPlayer.Pause();
            }
        }
        public static void Play()
        {
            if (MainMediaPlayer.MediaPlayer != null)
            {
                MainMediaPlayer.MediaPlayer.Play();
            }
        }
        public static void PreviousMusic()
        {
            switch (playMode)
            {
                case PlayMode.LoopAll:
                    if (PlayQueue.playingMusicIndex > 0)
                        PlayQueue.playingMusicIndex--;
                    else
                        PlayQueue.playingMusicIndex = PlayQueue.normalList.Count - 1;
                    break;
                case PlayMode.Shuffle:
                    if (PlayQueue.playingMusicIndex > 0)
                        PlayQueue.playingMusicIndex--;
                    else
                        PlayQueue.playingMusicIndex = PlayQueue.shuffleList.Count - 1;
                    break;
                case PlayMode.Single:

                    break;
                case PlayMode.Reverse:
                    if (PlayQueue.playingMusicIndex < PlayQueue.normalList.Count)
                        PlayQueue.playingMusicIndex++;
                    else
                        PlayQueue.playingMusicIndex = 0;
                    break;
            }
            if (playMode != PlayMode.Shuffle)
            {
                if (PlayQueue.playingMusicIndex < PlayQueue.normalList.Count && PlayQueue.playingMusicIndex >= 0)
                    PlayMusic(PlayQueue.normalList[PlayQueue.playingMusicIndex], PlayQueue.normalList, PlayQueue.playingMusicIndex);
            }
            else
            {
                if (PlayQueue.playingMusicIndex < PlayQueue.shuffleList.Count && PlayQueue.playingMusicIndex >= 0)
                    PlayMusic(PlayQueue.shuffleList[PlayQueue.playingMusicIndex], PlayQueue.shuffleList, PlayQueue.playingMusicIndex);
            }
        }
        public static void NextMusic()
        {
            switch (playMode)
            {
                case PlayMode.LoopAll:
                    if (PlayQueue.playingMusicIndex < PlayQueue.normalList.Count)
                        PlayQueue.playingMusicIndex++;
                    else
                        PlayQueue.playingMusicIndex = 0;
                    break;
                case PlayMode.Shuffle:
                    if (PlayQueue.playingMusicIndex < PlayQueue.shuffleList.Count)
                        PlayQueue.playingMusicIndex++;
                    else
                        PlayQueue.playingMusicIndex = 0;
                    break;
                case PlayMode.Single:
                    break;
                case PlayMode.Reverse:
                    if (PlayQueue.playingMusicIndex > 0)
                        PlayQueue.playingMusicIndex--;
                    else
                        PlayQueue.playingMusicIndex = PlayQueue.normalList.Count - 1;
                    break;
            }
            if (playMode != PlayMode.Shuffle)
            { 
                if (PlayQueue.playingMusicIndex < PlayQueue.normalList.Count && PlayQueue.playingMusicIndex >= 0)
                    PlayMusic(PlayQueue.normalList[PlayQueue.playingMusicIndex],PlayQueue.normalList,PlayQueue.playingMusicIndex);
            }
            else
            {
                if (PlayQueue.playingMusicIndex < PlayQueue.shuffleList.Count && PlayQueue.playingMusicIndex >= 0)
                    PlayMusic(PlayQueue.shuffleList[PlayQueue.playingMusicIndex], PlayQueue.shuffleList, PlayQueue.playingMusicIndex);
            }
        }

        public static void PlayMusic(Music music,List<Music>playQueue,int playingMusicIndex)
        {
            PlayQueue.SetList(playQueue);
            PlayQueue.playingMusicIndex = playingMusicIndex;
            switch (music.MusicType)
            {
                case MusicType.Local:
                    PlayLocalMusic(music);
                    break;
                case MusicType.External:
                    PlayExternalMusic(music);
                    break;
                case MusicType.Online:
                    PlayOnlineMusic(music);
                    break;

            }
        }

        private static async void PlayExternalMusic(Music music)
        {
            if (music.MusicType != MusicType.External) return;
            object obj = MusicManager.FindMusic(music);
            if (obj == null || obj.GetType() != typeof(ExternalMusic)) return;
            MediaPlaybackItem playbackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(await MusicManager.GetExternalMusicByExternalMusicKeyAsync(((ExternalMusic)obj).Key)));
            MainMediaPlayer.Source = playbackItem;
            PlayCore.MainMediaPlayer.MediaPlayer.Play();
            PlayCore.InitPlayCore();
            RefreshSMTC(playbackItem, music);
        }

        private static void PlayLocalMusic(Music music)
        {
            if (music.MusicType != MusicType.Local) return;
            object obj = MusicManager.FindMusic(music);
            if (obj == null || obj.GetType() != typeof(LocalMusic)) return;
            LocalMusic localMusic = (LocalMusic)obj;
            MediaPlaybackItem playbackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(localMusic.StorageFile));
            MainMediaPlayer.Source = playbackItem;
            PlayCore.MainMediaPlayer.MediaPlayer.Play();
            PlayCore.InitPlayCore();
            RefreshSMTC(playbackItem,music);
        }

        private static void PlayOnlineMusic(Music music)
        {
            if (music.MusicType != MusicType.Online) return;
            object obj = MusicManager.FindMusic(music);
            if (obj == null || obj.GetType() != typeof(OnlineMusic)) return;
            OnlineMusic onlineMusic = (OnlineMusic)obj;
            MediaPlaybackItem playbackItem = new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(onlineMusic.URL)));
            MainMediaPlayer.Source = playbackItem;
            PlayCore.MainMediaPlayer.MediaPlayer.Play();
            PlayCore.InitPlayCore();
            RefreshSMTC(playbackItem, music);
        }

        public static Music GetPlayingMusic()
        {
            
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                if (PlayQueue.normalList.Count <= PlayQueue.playingMusicIndex || PlayQueue.playingMusicIndex == -1)
                    return null;
                else
                    return PlayQueue.normalList[PlayQueue.playingMusicIndex];
            }
            else
            {
                if (PlayQueue.shuffleList.Count <= PlayQueue.playingMusicIndex || PlayQueue.playingMusicIndex == -1)
                    return null;
                else
                    return PlayQueue.shuffleList[PlayQueue.playingMusicIndex];
            }
        }

        public static void RefreshSMTC(MediaPlaybackItem playbackItem, Music music)
        {
            MediaItemDisplayProperties props = playbackItem.GetDisplayProperties();
            props.Type = Windows.Media.MediaPlaybackType.Music;
            props.MusicProperties.Title = music.Title;

            //List<Artist> artistsList = ArtistManager.DivideArtist(CurrentMusic.Artist);


            //String artistString = CurrentMusic.Artist;
            //for (int i = 0; i < artistsList.Count; i++)
            //{
            //    if (artistsList[i] == null) break;
            //    ArtistConvertItem artistConvertItem = ArtistConvertItemManager.ConvertItems.Find(x => x.Name == artistsList[i].Name);
            //    if (artistConvertItem != null)
            //        artistString = artistString.Replace(artistConvertItem.Name, artistConvertItem.ConvertTo);
            //}

            //props.MusicProperties.Artist = artistString.Replace(";", " /").Replace("/ ", "/");
            props.MusicProperties.AlbumTitle = music.Album;
            props.MusicProperties.TrackNumber = music.TrackNumber;

            //StorageItemThumbnail thumbnail = await music.file.GetThumbnailAsync(ThumbnailMode.SingleItem);
            //props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnail);
            playbackItem.ApplyDisplayProperties(props);


        }

        public static event EventHandler PlayModeChanged;
        public static void SetPlayMode(PlayMode newPlayMode)
        {
            playMode = newPlayMode;
            if (playMode == PlayMode.Shuffle)
            {
                PlayQueue.CreateShuffleList();
                Music music = GetPlayingMusic();
                if (music != null)
                {
                    PlayQueue.playingMusicIndex =  PlayQueue.shuffleList.IndexOf(music);
                }
            }
        }
    }

    public class PlayQueue
    {
        public static event EventHandler PlayQueueChanged;
        public static List<Music> normalList { get; private set; } = new List<Music>();
        public static List<Music> shuffleList { get; private set; } = new List<Music>();
        public static int playingMusicIndex { get; set; } = -1;

        public static void SetList(List<Music>musicList)
        {
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                normalList = musicList;
            }
            else
            {
                shuffleList = musicList;
            }
            PlayQueueChanged(null,new EventArgs());
        }
        public static void AddMusic(Music music)
        {
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                normalList.Add(music);
            }
            else
            {
                shuffleList.Add(music);
            }
            PlayQueueChanged(null, new EventArgs());
        }

        public static void AddMusic(List<Music> musicList)
        {
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                normalList = normalList.Concat(musicList).ToList();
            }
            else
            {
                shuffleList = shuffleList.Concat(musicList).ToList();
            }
            PlayQueueChanged(null, new EventArgs());
        }

        public static void AddPlayNextMusic(Music music)
        {
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                normalList.Insert(playingMusicIndex, music);
            }
            else
            {
                shuffleList.Insert(playingMusicIndex, music);
            }
            PlayQueueChanged(null, new EventArgs());
        }

        public static void CreateShuffleList()
        {
            if(normalList.Count < 2)
            {
                shuffleList = normalList;
            }
            else
            {
                List<Music> list = normalList.ToList();
                for (int i = 0; i < normalList.Count;i++)
                {
                    Random random = new Random();
                    int index = random.Next(0,list.Count);
                    shuffleList.Add(list[index]);
                    list.RemoveAt(index);
                }
            }
        }
    }
}

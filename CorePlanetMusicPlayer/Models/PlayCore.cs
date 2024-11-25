using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static CorePlanetMusicPlayer.Models.Library;

namespace CorePlanetMusicPlayer.Models
{
    public class PlayCore
    {
        public static MediaPlayerElement MainMediaPlayer { get; set; } = new MediaPlayerElement();

        public static Music CurrentMusic { get; set; } = new Music();
        public enum PlayMode { LoopAll, Shuffle, Single, Reverse};
        public static PlayMode playMode { get; private set; } = PlayMode.LoopAll;
        public static event EventHandler CurrentMusicChanging;
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

        public static async void PlayMusic(Music music,List<Music>playQueue,int playingMusicIndex)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                CurrentMusicChanging.Invoke(null,null);
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
                CurrentMusic = music;
                PlayQueue.SetList(playQueue);
            });
            
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
            Debug.WriteLine(onlineMusic.URL);
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
        public static event EventHandler PlayQueueChanged = delegate { };
        public static List<Music> normalList { get; private set; } = new List<Music>();
        public static List<Music> shuffleList { get; private set; } = new List<Music>();
        public static int playingMusicIndex { get; set; } = -1;
        public static bool SavePlayQueue { get; set; } = false;
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
            if (SavePlayQueue)
                SavePlayQueueAsync();
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
            if(SavePlayQueue)
                SavePlayQueueAsync();
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
            if (SavePlayQueue)
                SavePlayQueueAsync();
            PlayQueueChanged(null, new EventArgs());
        }

        public static void AddPlayNextMusic(Music music)
        {
            if (playingMusicIndex == -1 || playingMusicIndex >= normalList.Count)
                return;
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                normalList.Insert(playingMusicIndex, music);
            }
            else
            {
                shuffleList.Insert(playingMusicIndex, music);
            }
            if (SavePlayQueue)
                SavePlayQueueAsync();
            PlayQueueChanged(null, new EventArgs());
        }

        public static void RemoveItemFromPlayQueue(Music music)
        {
            if(PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                normalList.Remove(music);
            }
            else
            {
                shuffleList.Remove(music);
            }
            if (SavePlayQueue)
                SavePlayQueueAsync();
        }

        public static void RemoveItemFromPlayQueue(List<Music> musicList)
        {
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                for(int i = 0; i < musicList.Count; i++)
                {
                    normalList.Remove(musicList[i]);
                }
            }
            else
            {
                for (int i = 0; i < musicList.Count; i++)
                {
                    shuffleList.Remove(musicList[i]);
                }
            }
            if (SavePlayQueue)
                SavePlayQueueAsync();
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
            if (SavePlayQueue)
                SavePlayQueueAsync();
        }

        public static async Task SavePlayQueueAsync()
        {
            JsonArray jsonValues = new JsonArray();
            if(PlayCore.playMode == PlayCore.PlayMode.Shuffle)
            {
                for (int i = 0; i < shuffleList.Count; i++)
                    jsonValues.Add(JsonHelper.MusicToJsonObject(shuffleList[i]));
            }
            else
            {
                for (int i = 0; i < normalList.Count; i++)
                    jsonValues.Add(JsonHelper.MusicToJsonObject(normalList[i]));
            }
            await StorageHelper.WriteFile(ApplicationData.Current.LocalFolder, "LastPlayQueue.json", jsonValues.ToString());
            SavePlayQueueIndex();
        }

        public static void SavePlayQueueIndex()
        {
            ApplicationData.Current.LocalSettings.Values["PlayCore-LastPlayQueueIndex"] = playingMusicIndex.ToString();
        }

        public static async Task ReadLastPlayQueueAsync()
        {
            string fileStr = await StorageHelper.ReadFile(ApplicationData.Current.LocalFolder, "LastPlayQueue.json");
            if (String.IsNullOrEmpty(fileStr))
                return;
            JsonArray jsonArray;
            JsonArray.TryParse(fileStr,out jsonArray);
            if (jsonArray == null)
                return;
            List<Music> list = new List<Music>();
            for(int i=0;i<jsonArray.Count;i++)
            {
                list.Add(MusicManager.GetMusicFromJsonObject(jsonArray[i].GetObject()));
            }
            String str = ApplicationData.Current.LocalSettings.Values["PlayCore-LastPlayQueueIndex"].ToString();
            if(String.IsNullOrEmpty(str))
                return;
            int index = -1;
            try
            {
                index = Convert.ToInt32(str);
            }catch(FormatException)
            { 

            }
            playingMusicIndex = index;
            SetList(list);
            return;
        }
    }
}

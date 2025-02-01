using CorePlanetMusicPlayer.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer.Models
{
    public class PlayCore
    {
        public static MediaPlayerElement MainMediaPlayer { get; set; } = new MediaPlayerElement();

        public static Music CurrentMusic { get; set; } = new Music();
        public enum PlayMode { LoopAll, Shuffle, Single, Reverse };
        public static PlayMode playMode { get; private set; } = PlayMode.LoopAll;
        public static event EventHandler CurrentMusicChanging;

        private static bool MediaPlayerInited { get; set; } = false;
        public static void InitPlayCore()
        {
            MediaPlayerInited = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsPauseEnabled = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsPlayEnabled = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsPreviousEnabled = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsNextEnabled = true;
            MainMediaPlayer.MediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            MainMediaPlayer.MediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            MainMediaPlayer.MediaPlayer.CommandManager.NextReceived += (a, b) => {NextMusic();};
            MainMediaPlayer.MediaPlayer.CommandManager.PreviousReceived += (a, b) => {PreviousMusic();};
            MainMediaPlayer.MediaPlayer.MediaEnded += (a, b) => { NextMusic(); };
        }

        public static void Pause()
        {
            if (MainMediaPlayer.MediaPlayer != null)
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
                    if (PlayQueue.playingMusicIndex < PlayQueue.normalList.Count - 1)
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
                    if (PlayQueue.playingMusicIndex < PlayQueue.normalList.Count-1)
                        PlayQueue.playingMusicIndex++;
                    else
                        PlayQueue.playingMusicIndex = 0;
                    break;
                case PlayMode.Shuffle:
                    if (PlayQueue.playingMusicIndex < PlayQueue.shuffleList.Count-1)
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
                    PlayMusic(PlayQueue.normalList[PlayQueue.playingMusicIndex], PlayQueue.normalList, PlayQueue.playingMusicIndex);
            }
            else
            {
                if (PlayQueue.playingMusicIndex < PlayQueue.shuffleList.Count && PlayQueue.playingMusicIndex >= 0)
                    PlayMusic(PlayQueue.shuffleList[PlayQueue.playingMusicIndex], PlayQueue.shuffleList, PlayQueue.playingMusicIndex);
            }
        }

        public static async void PlayMusic(Music music, List<Music> playQueue, int playingMusicIndex)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                CurrentMusicChanging.Invoke(null, null);
                PlayQueue.playingMusicIndex = playingMusicIndex;
                if (music == null)
                    return;
                if (music.Available == false)
                    return;
                switch (music.MusicType)
                {
                    case MusicType.Local:
                    case MusicType.ExternalLocal:
                        PlayLocalMusic(music);
                        break;
                    case MusicType.Online:
                        PlayOnlineMusic(music);
                        break;
                    case MusicType.Removable:
                        PlayRemovableMusic(music);
                        break;
                }
                CurrentMusic = music;
                PlayQueue.SetList(playQueue);
            });

        }

        //private static async void PlayExternalMusic(Music music)
        //{
        //    if (music.MusicType != MusicType.External) return;
        //    object obj = MusicManager.FindMusic(music);
        //    if (obj == null || obj.GetType() != typeof(ExternalMusic)) return;
        //    MediaPlaybackItem playbackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(await MusicManager.GetExternalMusicByExternalMusicKeyAsync(((ExternalMusic)obj).Key)));
        //    MainMediaPlayer.Source = playbackItem;
        //    PlayCore.MainMediaPlayer.MediaPlayer.Play();
        //    PlayCore.InitPlayCore();
        //    RefreshSMTC(playbackItem, music);
        //}

        private static async void PlayLocalMusic(Music music)
        {
            if (music.MusicType != MusicType.Local && music.MusicType != MusicType.ExternalLocal) return;
            StorageFile storageFile;
            //if (music.MusicType == MusicType.ExternalLocal && music.DataCode.IndexOf("pmptemp-") != -1)
            //    storageFile = Library.TempMusicFiles.Find(x=>x.Path == music.DataCode.Substring(8));
            //else
                storageFile = LibraryManager.GetLocalMusicFile(music);
            if (storageFile == null)
                return;
            
            MediaPlaybackItem playbackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(storageFile));
            MainMediaPlayer.Source = playbackItem;
            MainMediaPlayer.MediaPlayer.Play();
            if (MediaPlayerInited == false)
                InitPlayCore();
            await RefreshSMTCAsync(playbackItem, music);
        }

        private static async void PlayOnlineMusic(Music music)
        {
            if (music.MusicType != MusicType.Online) return;
            
            MediaPlaybackItem playbackItem = new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri(music.DataCode)));
            MainMediaPlayer.Source = playbackItem;
            MainMediaPlayer.MediaPlayer.Play();
            await RefreshSMTCAsync(playbackItem, music);
            if (MediaPlayerInited == false)
                InitPlayCore();
            //Debug.WriteLine(music.DataCode);
        }

        private static async void PlayRemovableMusic(Music music)
        {
            if (music.MusicType != MusicType.Removable) return;

            StorageFile storageFile = RemovableDeviceManager.GetMusicFile(music);
            if (storageFile == null)
                return;
            MediaPlaybackItem playbackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(storageFile));
            MainMediaPlayer.Source = playbackItem;
            MainMediaPlayer.MediaPlayer.Play();
            await RefreshSMTCAsync(playbackItem, music);
            if (MediaPlayerInited == false)
                InitPlayCore();
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

        public static async Task RefreshSMTCAsync(MediaPlaybackItem playbackItem, Music music)
        {
            MediaItemDisplayProperties props = playbackItem.GetDisplayProperties();
            props.Type = Windows.Media.MediaPlaybackType.Music;
            props.MusicProperties.Title = music.Title;
            props.MusicProperties.Artist = music.Artist;
            props.MusicProperties.AlbumTitle = music.Album;
            props.MusicProperties.TrackNumber = music.TrackNumber;
            StorageFile storageFile = LibraryManager.GetLocalMusicFile(music);
            if (storageFile != null)
            {
                StorageItemThumbnail thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
                props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnail);
            }
            
            playbackItem.ApplyDisplayProperties(props);


        }

        public static event EventHandler PlayModeChanged;
        public static void SetPlayMode(PlayMode newPlayMode)
        {
            playMode = newPlayMode;
            if (playMode == PlayMode.Shuffle)
            {
                PlayQueue.CreateShuffleList();
                PlayQueue.playingMusicIndex = PlayQueue.shuffleList.IndexOf(CurrentMusic);
                if(PlayQueue.playingMusicIndex == -1 && PlayQueue.shuffleList.Count > 0)
                {
                    PlayQueue.playingMusicIndex = 0;
                }
            }
            else
            {
                if(PlayCore.playMode == PlayMode.Single && PlayQueue.shuffleList.Count > 0)
                {
                    PlayQueue.playingMusicIndex = PlayQueue.normalList.IndexOf(CurrentMusic);
                }/*else*/
                //{
                //    Music music = PlayQueue.normalList[PlayQueue.playingMusicIndex];
                //    PlayQueue.playingMusicIndex = PlayQueue.normalList.IndexOf(music);
                //}
                if ( PlayQueue.playingMusicIndex == -1&&PlayQueue.normalList.Count > 0 )
                    PlayQueue.playingMusicIndex = 0;
            }
            PlayModeChanged.Invoke(null,null);
        }
    }

    public class PlayQueue
    {
        public static event EventHandler PlayQueueChanged = delegate { };
        public static List<Music> normalList { get; private set; } = new List<Music>();
        public static List<Music> shuffleList { get; private set; } = new List<Music>();
        public static int playingMusicIndex { get; set; } = -1;
        public static bool SavePlayQueue { get; set; } = false;
        public static void SetList(List<Music> musicList)
        {
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
            {
                if (normalList.Equals(musicList))
                {
                    Debug.WriteLine("相同");
                    PlayQueueChanged(null, new EventArgs());
                    return;
                }
                normalList = musicList;
            }
            else
            {
                if (shuffleList.Equals(musicList))
                {
                    PlayQueueChanged(null, new EventArgs());
                    return;
                }
                shuffleList = musicList;
            }
            if (SavePlayQueue)
                SavePlayQueueAsync();
            PlayQueueChanged(null, new EventArgs());
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
            if (SavePlayQueue)
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
                normalList.Insert(playingMusicIndex+1, music);
            }
            else
            {
                shuffleList.Insert(playingMusicIndex+1, music);
            }
            if (SavePlayQueue)
                SavePlayQueueAsync();
            PlayQueueChanged(null, new EventArgs());
        }

        public static void RemoveItemFromPlayQueue(Music music)
        {
            if (PlayCore.playMode != PlayCore.PlayMode.Shuffle)
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
                for (int i = 0; i < musicList.Count; i++)
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
            if (normalList.Count < 2)
            {
                shuffleList = normalList;
            }
            else
            {
                shuffleList.Clear();
                List<Music> list = normalList.ToList();
                Random random = new Random();
                int index;
                for (int i = 0; i < normalList.Count; i++)
                {
                    
                    index = random.Next(0, list.Count);
                    shuffleList.Add(list[index]);
                    list.RemoveAt(index);
                }
            }
            if (SavePlayQueue)
                SavePlayQueueAsync();
        }

        public static async void SavePlayQueueAsync()
        {
            SavePlayQueueIndex();
            StorageFolder folder = await StorageManager.GetApplicationDataFolder("Cache");
            await SQLiteManager.MusicSimpleDataBasesHelper.CreateTableAsync(folder,"LastPlayQueue","latest");
            SQLiteManager.MusicSimpleDataBasesHelper.ClearTableData(folder.Path+ "\\LastPlayQueue.db","latest");
            //JsonArray jsonValues = new JsonArray();
            if (PlayCore.playMode == PlayCore.PlayMode.Shuffle)
            {
                SQLiteManager.MusicSimpleDataBasesHelper.SetTableData(folder.Path + "\\LastPlayQueue.db", "latest",shuffleList);
                //for (int i = 0; i < shuffleList.Count; i++) ;
                //jsonValues.Add(JsonHelper.MusicToJsonObject(shuffleList[i]));
            }
            else
            {
                SQLiteManager.MusicSimpleDataBasesHelper.SetTableData(folder.Path + "\\LastPlayQueue.db", "latest",normalList);
                //for (int i = 0; i < normalList.Count; i++) ;
                //jsonValues.Add(JsonHelper.MusicToJsonObject(normalList[i]));
            }
            //await StorageManager.WriteFile(ApplicationData.Current.LocalFolder, "LastPlayQueue.json", jsonValues.ToString());
            
        }

        public static void SavePlayQueueIndex()
        {
            ApplicationData.Current.LocalSettings.Values["PlayCore-LastPlayQueueIndex"] = playingMusicIndex.ToString();
        }

        public static async Task ReadLastPlayQueueAsync()
        {
            StorageFolder folder = await StorageManager.GetApplicationDataFolder("Cache");
            List<PlayHistoryItem>playHistoryItems = SQLiteManager.MusicSimpleDataBasesHelper.GetTableData(folder.Path+"\\LastPlayQueue.db","latest");
            List<Music> musicList = new List<Music>();
            for(int i=0;i<playHistoryItems.Count;i++)
            {
                Music music = MusicManager.GetMusicFromMusicTypeAndDataCode((MusicType)playHistoryItems[i].Type, playHistoryItems[i].DataCode);
                if (music == null || music.MusicType == MusicType.Removable || music.MusicType == MusicType.ExternalLocal)
                    return;
                musicList.Add(music);
            }
            //PlayQueue.normalList = musicList;
            String str = ApplicationData.Current.LocalSettings.Values["PlayCore-LastPlayQueueIndex"].ToString();
            if (String.IsNullOrEmpty(str))
                return;
            int index = -1;
            try
            {
                index = Convert.ToInt32(str);
            }
            catch (FormatException)
            {

            }
            playingMusicIndex = index;
            SetList(musicList);
            
            //return;
            //string fileStr = await StorageManager.ReadFile(ApplicationData.Current.LocalFolder, "LastPlayQueue.json");
            //if (String.IsNullOrEmpty(fileStr))
            //    return;
            //JsonArray jsonArray;
            //JsonArray.TryParse(fileStr, out jsonArray);
            //if (jsonArray == null)
            //    return;
            //List<Music> list = new List<Music>();
            //for (int i = 0; i < jsonArray.Count; i++)
            //{
            //    list.Add(MusicManager.GetMusicFromJsonObject(jsonArray[i].GetObject()));
            //}
            //String str = ApplicationData.Current.LocalSettings.Values["PlayCore-LastPlayQueueIndex"].ToString();
            //if (String.IsNullOrEmpty(str))
            //    return;
            //int index = -1;
            //try
            //{
            //    index = Convert.ToInt32(str);
            //}
            //catch (FormatException)
            //{

            //}
            //playingMusicIndex = index;
            //SetList(list);
            //return;
        }
    }
}

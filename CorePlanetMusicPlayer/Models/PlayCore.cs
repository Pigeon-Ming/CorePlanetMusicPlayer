﻿
using CorePlanetMusicPlayer.Models.HotLyric;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Audio;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer.Models
{
    public class PlayCore
    {
        public static MediaPlayerElement MainMediaPlayer { get; set; } = new MediaPlayerElement();
        public static AudioGraph MainAudioGraph;
        static MediaSourceAudioInputNode mediaSourceInputNode;
        
        public static Music CurrentMusic { get; set; }
        public enum LoopPlayModeEnum {None,All, Reverse, Single};
        public enum ShufflePlayModeEnum {None,All,NoRepeat};
        public static LoopPlayModeEnum LoopPlayMode = LoopPlayModeEnum.All;
        public static ShufflePlayModeEnum ShufflePlayMode = ShufflePlayModeEnum.None;
        public static bool firstPlay = true;
        
        /*----------
         数据处理
         ----------*/


        public static void PlayMusic(Music music,EventList<Music>newPlayQueue,int musicIndexInPlayQueue)
        {
            if (music == null) return;
            if (music.file == null) return;


            PlayQueue.currentMusicIndex = musicIndexInPlayQueue;
            if (ShufflePlayMode==ShufflePlayModeEnum.None)
            {
                PlayQueue.normalList = newPlayQueue;
                PlayQueue.normalList.Invoke();
            }
            else
            {
                PlayQueue.shuffleList = newPlayQueue;
                PlayQueue.shuffleList.Invoke();
            }
            

            
            //PlayQueue.normalList = newPlayQueue;
            
            if (true)
                PlayMusic_MediaPlayerElement(music);
            else
                PlayMusic_AudioGraph(music);
        }

        public static void PlayMusic(Music music, List<Music> newPlayQueue, int musicIndexInPlayQueue)
        {
            if (music == null) return;
            if (music.file == null) return;

            

            PlayQueue.currentMusicIndex = musicIndexInPlayQueue;
            if (ShufflePlayMode == ShufflePlayModeEnum.None)
                PlayQueue.normalList.SetItems(EventList<Music>.ListToEventList(newPlayQueue));
            else
                PlayQueue.shuffleList.SetItems(EventList<Music>.ListToEventList(newPlayQueue));
            //PlayQueue.normalList = newPlayQueue;

            if (true)
                PlayMusic_MediaPlayerElement(music);
            else
                PlayMusic_AudioGraph(music);
        }

        public static void PlayMusic()
        {
            if (MainMediaPlayer.MediaPlayer != null)
                MainMediaPlayer.MediaPlayer.Play();
        }

        public static void PauseMusic()
        {
            if (MainMediaPlayer.MediaPlayer != null)
                MainMediaPlayer.MediaPlayer.Pause();
        }

        public static void NextMusic()
        {
            
            int index = 0;

            Debug.WriteLine("当前播放模式："+ShufflePlayMode.ToString()+" - "+LoopPlayMode.ToString());


            if (ShufflePlayMode == ShufflePlayModeEnum.None)
            {
                
                if (PlayQueue.normalList.ToList<Music>().Count == 0) return;
                
                switch (LoopPlayMode)
                {
                    case LoopPlayModeEnum.None:
                        if (PlayQueue.currentMusicIndex != PlayQueue.normalList.Count - 1)
                            index = PlayQueue.currentMusicIndex + 1;
                        break;
                    case LoopPlayModeEnum.All:
                        if (PlayQueue.currentMusicIndex != PlayQueue.normalList.Count - 1)
                            index = PlayQueue.currentMusicIndex + 1;
                        else
                            index = 0;
                        break;
                    case LoopPlayModeEnum.Reverse:
                        if (PlayQueue.currentMusicIndex == 0)
                            index = PlayQueue.normalList.Count - 1;
                        else
                            index = PlayQueue.currentMusicIndex - 1;
                        break;
                    case LoopPlayModeEnum.Single:
                        index = PlayQueue.currentMusicIndex;
                        Debug.WriteLine("到这里了");
                        break;
                }
                Debug.WriteLine("Here:"+index.ToString());
                PlayMusic(PlayQueue.normalList[index], PlayQueue.normalList, index);
            }
            else
            {   
                switch (LoopPlayMode)
                {
                    case LoopPlayModeEnum.None:
                        if (PlayQueue.currentMusicIndex != PlayQueue.shuffleList.Count - 1)
                            index = PlayQueue.currentMusicIndex + 1;
                        break;
                    case LoopPlayModeEnum.All:
                        if (PlayQueue.currentMusicIndex == PlayQueue.shuffleList.Count - 1)
                            index = 0;
                        else
                            index = PlayQueue.currentMusicIndex + 1;
                        break;
                    case LoopPlayModeEnum.Reverse:
                        if (PlayQueue.currentMusicIndex == 0)
                            index = PlayQueue.shuffleList.Count - 1;
                        else
                            index = PlayQueue.currentMusicIndex - 1;
                        break;
                    case LoopPlayModeEnum.Single:
                        index = PlayQueue.currentMusicIndex;
                        break;
                }
                PlayMusic(PlayQueue.shuffleList[index], PlayQueue.shuffleList, index);
            }
            
        }
        public static void PreviousMusic()
        {
            if (PlayQueue.normalList.Count == 0) return;
            int index = 0;
            if (ShufflePlayMode == ShufflePlayModeEnum.None)
            {
                switch (LoopPlayMode)
                {
                    case LoopPlayModeEnum.None:
                        if (PlayQueue.currentMusicIndex != 0)
                            index = PlayQueue.currentMusicIndex - 1;
                        break;
                    case LoopPlayModeEnum.All:
                        if (PlayQueue.currentMusicIndex != 0)
                            index = PlayQueue.currentMusicIndex - 1;
                        else
                            index = PlayQueue.normalList.Count-1;
                        break;
                    case LoopPlayModeEnum.Reverse:
                        if (PlayQueue.currentMusicIndex == PlayQueue.normalList.Count - 1)
                            index = 0;
                        else
                            index = PlayQueue.normalList.Count + 1;
                        break;
                    case LoopPlayModeEnum.Single:
                        index = PlayQueue.currentMusicIndex;
                        break;
                }
                PlayMusic(PlayQueue.normalList[index], PlayQueue.normalList, index);
            }
            else
            {
                switch (LoopPlayMode)
                {
                    case LoopPlayModeEnum.None:
                        if (PlayQueue.currentMusicIndex != PlayQueue.shuffleList.Count - 1)
                            index = PlayQueue.currentMusicIndex - 1;
                        break;
                    case LoopPlayModeEnum.All:
                        if (PlayQueue.currentMusicIndex == 0)
                            index = PlayQueue.shuffleList.Count - 1;
                        else
                            index = PlayQueue.currentMusicIndex - 1;
                        break;
                    case LoopPlayModeEnum.Reverse:
                        if (PlayQueue.currentMusicIndex == PlayQueue.shuffleList.Count - 1)
                            index = 0;
                        else
                            index = PlayQueue.currentMusicIndex + 1;
                        break;
                    case LoopPlayModeEnum.Single:
                        index = PlayQueue.currentMusicIndex;
                        break;
                }
                PlayMusic(PlayQueue.shuffleList[index],PlayQueue.shuffleList,index);
            }
            
        }

        public static async Task RefreshSMTC(MediaPlaybackItem playbackItem,Music music)
        {
            MediaItemDisplayProperties props = playbackItem.GetDisplayProperties();
            props.Type = Windows.Media.MediaPlaybackType.Music;
            props.MusicProperties.Title = CurrentMusic.Title;

            List<Artist> artistsList = ArtistManager.DivideArtist(CurrentMusic.Artist);


            String artistString = CurrentMusic.Artist;
            for(int i = 0; i < artistsList.Count; i++)
            {
                if (artistsList[i] == null) break;
                ArtistConvertItem artistConvertItem = ArtistConvertItemManager.ConvertItems.Find(x => x.Name == artistsList[i].Name);
                if (artistConvertItem != null)
                    artistString = artistString.Replace(artistConvertItem.Name,artistConvertItem.ConvertTo);
            }
            
            props.MusicProperties.Artist = artistString.Replace(";"," /").Replace("/ ","/");

            Debug.WriteLine(props.MusicProperties.Artist);
            props.MusicProperties.AlbumTitle = CurrentMusic.Album;
            props.MusicProperties.TrackNumber = CurrentMusic.TrackNumber;

            StorageItemThumbnail thumbnail = await music.file.GetThumbnailAsync(ThumbnailMode.SingleItem);
            props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnail);
            playbackItem.ApplyDisplayProperties(props);

            
        }
        /*----------
         MediaPlayerElement
         ----------*/
        public static void InitMediaPlayerElement()
        {
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsPauseEnabled = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsPlayEnabled = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsPreviousEnabled = true;
            MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.IsNextEnabled = true;
            MainMediaPlayer.MediaPlayer.CommandManager.NextBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            MainMediaPlayer.MediaPlayer.CommandManager.PreviousBehavior.EnablingRule = MediaCommandEnablingRule.Always;
            MainMediaPlayer.MediaPlayer.CommandManager.NextReceived += CommandManager_NextReceived;
            MainMediaPlayer.MediaPlayer.CommandManager.PreviousReceived += CommandManager_PreviousReceived;
            MainMediaPlayer.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            
        }

        private static void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            Debug.WriteLine("MeidaEnded");
            NextMusic();
        }

        private static void CommandManager_PreviousReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPreviousReceivedEventArgs args)
        {
            PreviousMusic();
        }

        private static void CommandManager_NextReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerNextReceivedEventArgs args)
        {
            NextMusic();
        }

        private static async Task PlayMusic_MediaPlayerElement(Music music)
        {
            //await MusicManager.GetMusicHDCoverAsync(music);
            MediaPlaybackItem playbackItem;
            StorageFile file;
            if (!String.IsNullOrEmpty(music.Token))
            {
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(music.Token))
                {
                    file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(music.Token);
                    Debug.WriteLine("从token获取文件");
                }
                else
                    file = music.file;
            }

            else
                file = music.file;
            //if (music.source.State == MediaSourceState.Failed) return;
            //Debug.WriteLine("MusicSourceState:"+music.source.State.ToString());
            playbackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));
            music = await MusicManager.GetMusicPropertiesAsync_Single(music);
            
            CurrentMusic = music;
            
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                CurrentMusic.cover = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                CurrentMusic = await MusicManager.GetMusicHDCoverAsync_Taglib(CurrentMusic);
                MainMediaPlayer.Source = playbackItem;
                PlayCore.MainMediaPlayer.MediaPlayer.Play();
            
            
                RefreshSMTC(playbackItem, music);
                if (firstPlay)
                {
                    firstPlay = false;
                    
                   InitMediaPlayerElement();
                }
            });


            
        }
        /*----------
         AudioGraph
         ----------*/
        public static async Task InitAudioGraph()
        {

            AudioGraphSettings settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                //ShowErrorMessage("AudioGraph creation error: " + result.Status.ToString());
            }

            MainAudioGraph = result.Graph;
            mediaSourceInputNode.MediaSourceCompleted += MediaSourceInputNode_MediaSourceCompleted;
        }

        private static void MediaSourceInputNode_MediaSourceCompleted(MediaSourceAudioInputNode sender, object args)
        {
            MainAudioGraph.Stop();
        }

        private async Task CreateMediaSourceInputNode(MediaSource mediaSource)
        {
            if (MainAudioGraph == null)
                return;

            CreateMediaSourceAudioInputNodeResult mediaSourceAudioInputNodeResult =
                await MainAudioGraph.CreateMediaSourceAudioInputNodeAsync(mediaSource);

            if (mediaSourceAudioInputNodeResult.Status != MediaSourceAudioInputNodeCreationStatus.Success)
            {
                switch (mediaSourceAudioInputNodeResult.Status)
                {
                    case MediaSourceAudioInputNodeCreationStatus.FormatNotSupported:
                        Debug.WriteLine("The MediaSource uses an unsupported format");
                        break;
                    case MediaSourceAudioInputNodeCreationStatus.NetworkError:
                        Debug.WriteLine("The MediaSource requires a network connection and a network-related error occurred");
                        break;
                    case MediaSourceAudioInputNodeCreationStatus.UnknownFailure:
                    default:
                        Debug.WriteLine("An unknown error occurred while opening the MediaSource");
                        break;
                }
                return;
            }

            mediaSourceInputNode = mediaSourceAudioInputNodeResult.Node;
        }

        private static void PlayMusic_AudioGraph(Music music)
        {

        }
    }
}

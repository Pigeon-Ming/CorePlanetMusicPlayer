
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
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer.Models
{
    public class PlayCore
    {
        public static MediaPlayerElement MainMediaPlayer = new MediaPlayerElement();
        public static AudioGraph MainAudioGraph;
        static MediaSourceAudioInputNode mediaSourceInputNode;
        
        public static Music CurrentMusic { get; set; }
        public enum LoopPlayModeEnum {None,All, Reverse, Single};
        public enum ShufflePlayModeEnum {None,All,NoRepeat};
        public static LoopPlayModeEnum LoopPlayMode = LoopPlayModeEnum.None;
        public static ShufflePlayModeEnum ShufflePlayMode = ShufflePlayModeEnum.None;
        public static bool firstPlay = true;
        
        /*----------
         数据处理
         ----------*/


        public static void PlayMusic(Music music,EventList<Music>newPlayQueue,int musicIndexInPlayQueue)
        {
            PlayQueue.currentMusicIndex = musicIndexInPlayQueue;
            PlayQueue.normalList.SetItems(newPlayQueue);
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
            Debug.WriteLine("Here?");
            if (ShufflePlayMode == ShufflePlayModeEnum.None)
            {
                Debug.WriteLine("Here??"+ LoopPlayMode.ToString());
                Debug.WriteLine("PlayQueueCount" + PlayQueue.normalList.Count);
                if (PlayQueue.normalList.ToList<Music>().Count == 0) return;
                
                switch (LoopPlayMode)
                {
                    case LoopPlayModeEnum.None:
                        if (PlayQueue.currentMusicIndex != PlayQueue.normalList.Count - 1)
                            index = PlayQueue.currentMusicIndex + 1;
                        break;
                    case LoopPlayModeEnum.All:

                        break;
                    case LoopPlayModeEnum.Reverse:
                        if (PlayQueue.currentMusicIndex == 0)
                            index = PlayQueue.normalList.Count - 1;
                        else
                            index = PlayQueue.currentMusicIndex - 1;
                        break;
                    case LoopPlayModeEnum.Single:
                        index = PlayQueue.currentMusicIndex;
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
                            index = PlayQueue.shuffleList.Count + 1;
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
                        if (PlayQueue.currentMusicIndex == PlayQueue.shuffleList.Count - 1)
                            index = 0;
                        else
                            index = PlayQueue.shuffleList.Count + 1;
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
            props.MusicProperties.Artist = CurrentMusic.Artist;
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
            
            MediaPlaybackItem playbackItem;
            StorageFile file = music.file;
            //Debug.WriteLine("Here!||"+music.file.Path);
            playbackItem = new MediaPlaybackItem(music.source);
            //Debug.WriteLine("Here!!");
            music = await MusicManager.GetMusicPropertiesAsync(music);
            //Debug.WriteLine("Here!!!");
            CurrentMusic = music;
            //Debug.WriteLine("Here!!!!");
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MainMediaPlayer.Source = playbackItem;
                PlayCore.MainMediaPlayer.MediaPlayer.Play();
                //Debug.WriteLine("Here!!!!！");
            
            
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

using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Media;
using System.Diagnostics;
using Windows.Media.Playback;

namespace CorePlanetMusicPlayer.PlayCore
{
    public class SMTCManager
    {
        public static async void UpdateSMTC(SystemMediaTransportControls SMTCControl, IMusic music)//该方法未正确实现
        {
            //To-Do:添加封面

            //StorageFile storageFile = LibraryManager.GetLocalMusicFile(music);
            //if (storageFile != null)
            //{
            //    StorageItemThumbnail thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
            //    MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnail);
            //}

            //playbackItem.ApplyDisplayProperties(props);

            await Task.Delay(500);
            Debug.WriteLine("更新SMTC");
            SystemMediaTransportControlsDisplayUpdater updater = SMTCControl.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            Debug.WriteLine("SMTC-"+music.Title);
            updater.MusicProperties.Title = music.Title;
            updater.MusicProperties.Artist = music.Artist;
            updater.MusicProperties.AlbumTitle = music.Album;
            updater.MusicProperties.TrackNumber = music.TrackNumber;
            //LocalMusic localMusic = (LocalMusic)music;
            //await updater.CopyFromFileAsync(MediaPlaybackType.Music,localMusic.StorageFile);
            updater.Update();
        }

        public static async void UpdateSMTC(MediaPlaybackItem item, IMusic music)
        {
            if (item == null)
                return;
            MediaItemDisplayProperties props = item.GetDisplayProperties();
            props.Type = Windows.Media.MediaPlaybackType.Music;
            props.MusicProperties.Title = music.Title;
            props.MusicProperties.Artist = music.Artist;
            props.MusicProperties.AlbumTitle = music.Album;
            props.MusicProperties.TrackNumber = music.TrackNumber;
            Debug.WriteLine("SMTC-方法2");
            item.ApplyDisplayProperties(props);
            if (music is LocalMusic)
            {
                LocalMusic localMusic = (LocalMusic)music;
                StorageItemThumbnail thumbnail = await localMusic.StorageFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
                props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnail);
                await Task.Delay(500);
            }
            //StorageFile storageFile = LibraryManager.GetLocalMusicFile(music);
            //if (storageFile != null)
            //{
            //    StorageItemThumbnail thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
            //    props.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnail);
            //}

            item.ApplyDisplayProperties(props);
        }
    }
}

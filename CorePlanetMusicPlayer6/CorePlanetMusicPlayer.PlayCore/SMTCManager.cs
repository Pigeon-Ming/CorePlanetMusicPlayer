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

namespace CorePlanetMusicPlayer.PlayCore
{
    public class SMTCManager
    {
        public static void UpdateSMTC(SystemMediaTransportControls SMTCControl, IMusic music)
        {
            //To-Do:添加封面
            
            //StorageFile storageFile = LibraryManager.GetLocalMusicFile(music);
            //if (storageFile != null)
            //{
            //    StorageItemThumbnail thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem);
            //    MainMediaPlayer.MediaPlayer.SystemMediaTransportControls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromStream(thumbnail);
            //}

            //playbackItem.ApplyDisplayProperties(props);

            //await Task.Delay(500);
            SMTCControl.DisplayUpdater.MusicProperties.Title = music.Title;
            SMTCControl.DisplayUpdater.MusicProperties.Artist = music.Artist;
            SMTCControl.DisplayUpdater.MusicProperties.AlbumTitle = music.Album;
            SMTCControl.DisplayUpdater.MusicProperties.TrackNumber = music.TrackNumber;
            SMTCControl.DisplayUpdater.Update();
        }
    }
}

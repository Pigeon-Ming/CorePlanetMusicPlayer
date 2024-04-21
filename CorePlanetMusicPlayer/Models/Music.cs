using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Models
{
    public class Music
    {
        public String Title { get; set; }
        public String Artist { get; set; }
        public String Album { get; set; }//专辑名称
        public uint Bitrate { get; set; }//比特率
        public uint Year { get; set; }//年份
        public uint TrackNumber { get; set; }//专辑内音乐编号
        public String Duration { get; set; }//长度

        public StorageFile file { get; set; }
        public MediaSource source { get; set; }
        public BitmapImage cover { get; set; }
    }
    
    public class MusicManager
    {
        public static async Task<Music> GetMusicPropertiesAsync(Music music)
        {
            
            //Debug.WriteLine(LibraryManager.gotPropertyCount+"||"+ Library.LocalLibraryMusic.Count);
            if (music.file == null)
            {
                if (LibraryManager.gotPropertyCount == Library.LocalLibraryMusic.Count)
                {
                    AlbumManager.ClassifyAlbum();
                    ArtistManager.ClassifyArtist();
                }
                return music;
            }
            if (music.Album!="未知专辑"|| music.Artist != "未知艺术家")
                return music;
            
            StorageFile file = music.file;
            StorageItemContentProperties storageItemContentProperties = file.Properties;
            MusicProperties musicProperties = await storageItemContentProperties.GetMusicPropertiesAsync(); // 音频属性
            if (!string.IsNullOrEmpty(musicProperties.Title))
                music.Title = musicProperties.Title;

            if (!string.IsNullOrEmpty(musicProperties.Album))
                music.Album = musicProperties.Album;
            else
                music.Album = "未知专辑";

            if (!string.IsNullOrEmpty(musicProperties.Artist))
                music.Artist = musicProperties.Artist;
            else
                music.Artist = "未知艺术家";
            music.Year = musicProperties.Year;
            music.Bitrate = musicProperties.Bitrate;
            music.Duration = musicProperties.Duration.ToString().Substring(3, 5);
            music.TrackNumber = musicProperties.TrackNumber;
            ++LibraryManager.gotPropertyCount;
            if (LibraryManager.gotPropertyCount == Library.LocalLibraryMusic.Count)
            {
                AlbumManager.ClassifyAlbum();
                ArtistManager.ClassifyArtist();
            }
            
            return music;
        }

        public static async Task<Music> GetMusicCoverAsync(Music music)
        {
            StorageFile file = music.file;
            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView);
            music.cover = new BitmapImage();
            if (thumbnail != null)
            {
                
                music.cover.SetSource(thumbnail);
                
            }
            return music;
        }

        public static Music FindMusicByFileName(String searchName)
        {
            Music music = Library.LocalLibraryMusic.Find(x => x.file.Name == searchName);
            return music;
        }

        public static async Task<Music> GetMusicHDCoverAsync(Music music)
        {
            StorageFile file = music.file;
            StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);

            try
            {
                music.cover = new BitmapImage();
                if (thumbnail != null)
                {

                    music.cover.SetSource(thumbnail);

                }
            }catch (Exception ex)
            {

            }
            
            return music;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace PlanetMusicPlayer.Models
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
        public BitmapImage cover { get; set; }
    }
    
    public class MusicManager
    {
        public static async Task<Music> GetMusicPropertiesAsync(Music music)
        {
            ++LibraryManager.gotPropertyCount;
            Debug.WriteLine(LibraryManager.gotPropertyCount+"||"+ Library.LocalLibraryMusic.Count);
            if (music.file == null)
            {
                if (LibraryManager.gotPropertyCount == Library.LocalLibraryMusic.Count)
                {
                    AlbumManager.ClassifyAlbum();
                    ArtistManager.ClassifyArtist();
                }
                return music;
            }
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
                music.Album = "未知艺术家";
            music.Year = musicProperties.Year;
            music.Bitrate = musicProperties.Bitrate;
            music.Duration = musicProperties.Duration.ToString().Substring(3, 5);
            music.TrackNumber = musicProperties.TrackNumber;

            if (LibraryManager.gotPropertyCount == Library.LocalLibraryMusic.Count)
            {
                AlbumManager.ClassifyAlbum();
                ArtistManager.ClassifyArtist();
            }
            return music;
        }
    }
}

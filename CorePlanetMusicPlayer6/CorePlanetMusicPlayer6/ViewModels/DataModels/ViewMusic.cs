using CorePlanetMusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer6.ViewModels.DataModels
{
    public class ViewMusic
    {
        public IMusic Music { get; set; }

        public BitmapImage Cover { get; set; }
    }

    public class ViewMusicManager
    {
        //public static async Task<List<ViewMusic>> CreateViewMusicListAsync(List<IMusic>musicList)
        //{
        //    List<ViewMusic> viewMusicList = new List<ViewMusic>();
        //    foreach(IMusic music in musicList)
        //    {
        //         viewMusicList.Add(await CreateViewMusicAsync(music));
        //    }
        //    return viewMusicList;
        //}
        public static async Task<ViewMusic> CreateViewMusicAsync(IMusic music)
        {
            if (music == null)
                return null;
            ViewMusic viewMusic = new ViewMusic();
            viewMusic.Music = music;
            if (music is LocalMusic)
            {
                LocalMusic localMusic = (LocalMusic)music;
                viewMusic.Cover = await LocalMusicManager.GetCover_TagLibAsync(localMusic);
                
            }
            else if(music is OnlineMusic)
            {
                //To-Do:OnlineMusic的ViewMusic生成
            }

            return viewMusic;
        }

        public static async Task<List<ViewMusic>> FindViewMusicListInViewMusicListAsync(List<ViewMusic> viewMusicList, List<IMusic> musicList, bool? CreateIfNotExsit = true)
        {
            if (viewMusicList == null)
                return null;
            List<ViewMusic> newViewMusicList = new List<ViewMusic>();
            foreach(IMusic music in musicList)
            {
                newViewMusicList.Add(await FindViewMusicInViewMusicListAsync(viewMusicList,music,CreateIfNotExsit));
            }
            return newViewMusicList;
        }

        public static async Task<ViewMusic> FindViewMusicInViewMusicListAsync(List<ViewMusic>viewMusicList, IMusic music, bool? CreateIfNotExsit = true)
        {
            if (viewMusicList == null)
                return null;
            ViewMusic viewMusic = viewMusicList.Find(x => x.Music.Equals(music));
            if (viewMusic == null)
            {
                if (CreateIfNotExsit == true)
                    return await CreateViewMusicAsync(music);
                else
                    return new ViewMusic { Cover = new BitmapImage(),Music = music };
            }
            else
            {
                return viewMusic;

            }
            
        }
    }
}

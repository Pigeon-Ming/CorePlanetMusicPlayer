using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    /// <summary>
    /// 歌曲的年份列表
    /// </summary>
    public class Year
    {
        public uint ReleaseYear { get; set; }

        public List<IMusic> Music {  get; set; } = new List<IMusic>();
    }

    public class YearManager
    {
        public ObservableCollection<Year> Years { get; set; } = new ObservableCollection<Year>();

        public 
    }
}

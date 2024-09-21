using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace CorePlanetMusicPlayer.Models
{
    public class Image
    {
        public BitmapImage BitmapImage { get; set; } = new BitmapImage();

        public string Token { get; set; } = "";
    }
}

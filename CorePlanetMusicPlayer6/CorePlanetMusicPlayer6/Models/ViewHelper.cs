using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CorePlanetMusicPlayer6.Models
{
    public class ViewHelper
    {
        public static void SetPageHeaderMargin(Panel panel,Size pageSize)
        {
            if (pageSize.Width < 640)
                panel.Margin = new Thickness(56, 0, 8, 0);
            else
                panel.Margin = new Thickness(8, 0, 8, 0);


        }
    }
}

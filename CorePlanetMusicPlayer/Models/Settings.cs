using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CorePlanetMusicPlayer.Models
{
    public class CorePMPSettings
    {
        public static List<String> Library_ScanMusicType { get; set; } = new List<String> { "Local","External","Online"/*,"Removable"*/};//要扫描音乐的位置
        public static bool Library_GetMusicCoverWhenLoad { get; set; } = true;
    }
}

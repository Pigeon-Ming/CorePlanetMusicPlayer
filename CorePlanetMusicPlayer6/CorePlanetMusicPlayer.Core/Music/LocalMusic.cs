using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Music
{
    /// <summary>
    /// 本地音乐的扩展类
    /// </summary>
    public sealed class LocalMusic
    {
        public Music Music { get; set; }

        public MusicFileInfo FileInfo { get; set; }

        //public string AccessToken { get; set; } = string.Empty;


        public bool IsAvaliable
        {
            get { return Music != null && FileInfo != null && FileInfo.HasPath; }
        }

        public static LocalMusic Create(Music music, MusicFileInfo fileInfo, string accessToken)
        {
            Guard.NotNull(music, nameof(music));
            Guard.NotNull(fileInfo, nameof(fileInfo));

            return new LocalMusic
            {
                Music = music,
                FileInfo = fileInfo,
                //AccessToken = accessToken ?? string.Empty
            };
        }
    }
}

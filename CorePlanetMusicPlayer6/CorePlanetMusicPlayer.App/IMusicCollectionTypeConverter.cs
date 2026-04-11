using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorePlanetMusicPlayer.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CorePlanetMusicPlayer.App
{
    public class IMusicCollectionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return string.Empty;

            Type valueType = value as Type ?? value.GetType();

            if (typeof(Album).IsAssignableFrom(valueType))
                return "专辑";
            if (typeof(Artist).IsAssignableFrom(valueType))
                return "艺术家";
            if (typeof(Genre).IsAssignableFrom(valueType))
                return "流派";
            if (typeof(Year).IsAssignableFrom(valueType))
                return "年份";
            if (typeof(Playlist).IsAssignableFrom(valueType))
                return "播放列表";

            return "未知集合";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

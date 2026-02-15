using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CorePlanetMusicPlayer.App
{
    public class ThumbConverter : DependencyObject, IValueConverter
    {
        public double SecondValue
        {
            get { return (double)GetValue(SecondValueProperty); }
            set { SetValue(SecondValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondValueProperty =
            DependencyProperty.Register("SecondValue", typeof(double), typeof(ThumbConverter), new PropertyMetadata(0d));


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Convert seconds (value) to "mm:ss" for slider tip display
            if (value == null)
                return "00:00";

            if (!double.TryParse(value.ToString(), out double seconds))
                return "00:00";

            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
                return "00:00";

            var ts = TimeSpan.FromSeconds(seconds);
            int totalMinutes = (int)ts.TotalMinutes;
            int secs = ts.Seconds;
            return string.Format("{0:D2}:{1:D2}", totalMinutes, secs);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            // Allow converting back from "mm:ss" or "m:ss" to seconds (double)
            if (value == null)
                return 0d;

            var s = value.ToString();
            var parts = s.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[0], out int m) && int.TryParse(parts[1], out int sec))
            {
                return (double)(m * 60 + sec);
            }

            // fallback: try parse as number
            if (double.TryParse(s, out double d))
                return d;

            return 0d;
        }
    }
}

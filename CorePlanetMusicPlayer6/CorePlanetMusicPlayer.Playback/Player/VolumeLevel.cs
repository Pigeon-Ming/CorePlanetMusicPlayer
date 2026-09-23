using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    public sealed class VolumeLevel
    {
        public double Value { get; private set; }

        public bool IsMuted 
        {
            get { return Value <= 0; }
        }

        public int Percentage
        {
            get { return (int)(Value * 100); }
        }

        private VolumeLevel(double value)
        {
            Value = Normalize(value);
        }

        public static VolumeLevel Muted()
        {
            return new VolumeLevel(0);
        }

        public static VolumeLevel Default()
        {
            return new VolumeLevel(1);
        }

        public static VolumeLevel Create(double value)
        {
            return new VolumeLevel(value);
        }

        public static VolumeLevel WithValue(double value)
        {
            return new VolumeLevel(value);
        }

        private static double Normalize(double value)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value > 1)
            {
                return 1;
            }

            return value;
        }
    }
}

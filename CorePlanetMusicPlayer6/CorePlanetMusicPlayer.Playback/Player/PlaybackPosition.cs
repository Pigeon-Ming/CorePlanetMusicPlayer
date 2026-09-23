using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Playback.Player
{
    public sealed class PlaybackPosition
    {
        public TimeSpan Position { get; private set; }

        public TimeSpan Duration { get; private set; }

        public TimeSpan Remaining { 
            get
            {
                if (Duration <= TimeSpan.Zero)
                {
                    return TimeSpan.Zero;
                }

                var remaining = Duration - Position;

                if (remaining < TimeSpan.Zero)
                {
                    return TimeSpan.Zero;
                }

                return remaining;
            } 
        }

        public double Progress
        {
            get
            {
                if (Duration <= TimeSpan.Zero)
                {
                    return 0;
                }

                return Position.TotalMilliseconds / Duration.TotalMilliseconds;
            }
        }

        public bool HasDuration
        {
            get { return Duration > TimeSpan.Zero; }
        }

        private PlaybackPosition(TimeSpan position, TimeSpan duration)
        {
            Guard.NotNegative(position, nameof(position));
            Guard.NotNegative(duration, nameof(duration));

            Duration = duration;
            Position = NormalizePosition(position, duration);
        }

        public static PlaybackPosition Empty()
        {
            return new PlaybackPosition(TimeSpan.Zero, TimeSpan.Zero);
        }

        public static PlaybackPosition Create(TimeSpan position, TimeSpan duration)
        {
            return new PlaybackPosition(position, duration);
        }

        public PlaybackPosition WithPosition(TimeSpan position)
        {
            return new PlaybackPosition(position, Duration);
        }

        public PlaybackPosition WithDuration(TimeSpan duration)
        {
            return new PlaybackPosition(Position, duration);
        }

        private static TimeSpan NormalizePosition(TimeSpan position, TimeSpan duration)
        {
            if (position < TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            if(duration > TimeSpan.Zero && position > duration)
            {
                return duration;
            }

            return position;
        }
    }
}

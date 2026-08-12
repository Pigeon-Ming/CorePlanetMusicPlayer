using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.History
{
    public struct PlaybackHistoryId : IEquatable<PlaybackHistoryId>
    {
        public string Value { get; private set; }

        public PlaybackHistoryId(string value)
        {
            Value = EntityId.Normalize(value);
        }

        public static PlaybackHistoryId NewId()
        {
            return new PlaybackHistoryId(EntityId.New());
        }

        public bool IsEmpty
        {
            get { return EntityId.IsEmpty(Value); }
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(PlaybackHistoryId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj is PlaybackHistoryId)
            {
                return Equals((PlaybackHistoryId)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        public static bool operator ==(PlaybackHistoryId left, PlaybackHistoryId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlaybackHistoryId left, PlaybackHistoryId right)
        {
            return !left.Equals(right);
        }
    }
}

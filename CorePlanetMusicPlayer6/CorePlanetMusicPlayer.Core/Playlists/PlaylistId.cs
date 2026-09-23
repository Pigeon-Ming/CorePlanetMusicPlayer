using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Playlists
{
    public struct PlaylistId : IEquatable<PlaylistId>
    {
        public string Value { get; private set; }

        public PlaylistId(string value)
        {
            Value = value;
        }

        public static PlaylistId NewId()
        {
            return new PlaylistId(EntityId.New());
        }

        public bool IsEmpty
        {
            get { return EntityId.IsEmpty(Value); }
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(PlaylistId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj is PlaylistId other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        public static bool operator ==(PlaylistId left, PlaylistId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlaylistId left, PlaylistId right)
        {
            return !left.Equals(right);
        }
    }
}

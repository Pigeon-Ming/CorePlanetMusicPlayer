using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Artists
{
    public struct ArtistId
    {
        public string Value { get; private set; }

        public ArtistId(string value)
        {
            Value = EntityId.Normalize(value);
        }

        public static ArtistId NewId()
        {
            return new ArtistId(EntityId.New());
        }

        public bool IsEmpty
        {
            get { return EntityId.IsEmpty(Value); }
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(ArtistId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is ArtistId)
                return Equals((ArtistId)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        public static bool operator ==(ArtistId left, ArtistId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ArtistId left, ArtistId right)
        {
            return !left.Equals(right);
        }
    }
}

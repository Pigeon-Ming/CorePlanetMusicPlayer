using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Albums
{
    public struct AlbumId
    {
        public string Value { get; private set; }

        public AlbumId(string value)
        {
            Value = EntityId.Normalize(value);
        }

        public static AlbumId NewId()
        {
            return new AlbumId(EntityId.New());
        }

        public bool IsEmpty
        {
            get { return EntityId.IsEmpty(Value); }
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(AlbumId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is AlbumId)
                return Equals((AlbumId)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        public static bool operator ==(AlbumId left, AlbumId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AlbumId left, AlbumId right)
        {
            return !left.Equals(right);
        }
    }
}

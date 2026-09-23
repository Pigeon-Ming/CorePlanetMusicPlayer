using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Library
{
    public class LibraryFolderId
    {
        public string Value { get; private set; }

        public LibraryFolderId(string value)
        {
            Value = EntityId.Normalize(value);
        }

        public static LibraryFolderId NewId()
        {
            return new LibraryFolderId(EntityId.New());
        }

        public bool IsEmpty
        {
            get { return EntityId.IsEmpty(Value); }
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(LibraryFolderId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj is LibraryFolderId)
            {
                return Equals((LibraryFolderId)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        public static bool operator ==(LibraryFolderId left, LibraryFolderId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LibraryFolderId left, LibraryFolderId right)
        {
            return !left.Equals(right);
        }
    }
}

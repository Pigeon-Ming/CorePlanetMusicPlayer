using CorePlanetMusicPlayer.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Core.Music
{
    /// <summary>
    /// 音乐的唯一标识
    /// </summary>
    public struct MusicId : IEquatable<MusicId>
    {
        public string Value { get; private set; }

        public MusicId(string value)
        {
            Value = EntityId.Normalize(value);
        }

        public static MusicId NewId()
        {
            return new MusicId(EntityId.New());
        }

        public bool IsEmpty
        {
            get { return EntityId.IsEmpty(Value); }
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Equals(MusicId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj is MusicId)
            {
                return Equals((MusicId)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        public static bool operator ==(MusicId left, MusicId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MusicId left, MusicId right)
        {
            return !left.Equals(right);
        }
    }
}

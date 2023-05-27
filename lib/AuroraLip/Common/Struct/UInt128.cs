using System.Globalization;

namespace AuroraLib.Common.Struct
{
    /// <summary>
    /// Represents a 16-byte, 128-bit unsigned integer. is mainly used for checksums.
    /// </summary>
    [Serializable]
    public struct UInt128 : IFormattable, IComparable<UInt128>, IEquatable<UInt128>
    {
        public readonly ulong Low, High;

        public static readonly UInt128 MaxValue = new UInt128(ulong.MaxValue, ulong.MaxValue);
        public static readonly UInt128 MinValue = new UInt128(ulong.MinValue, ulong.MinValue);

        public UInt128(ulong high, ulong low)
        {
            High = high;
            Low = low;
        }

        public UInt128(string HexString)
        {
            if (HexString.Length > 16)
            {
                Low = UInt64.Parse(HexString.Substring(0, 16), NumberStyles.HexNumber);
                High = UInt64.Parse(HexString.Substring(HexString.Length - 16), NumberStyles.HexNumber);
            }
            else
            {
                Low = UInt64.Parse(HexString, NumberStyles.HexNumber);
                High = 0;
            }
        }

        public UInt128(UInt128 value)
        {
            High = value.High;
            Low = value.Low;
        }

        public override string ToString() => ToString(null, null);

        public string ToString(IFormatProvider provider) => ToString(null, provider);

        public string ToString(string format) => ToString(format, null);

        public string ToString(string format, IFormatProvider provider)
        {
            if (High == 0)
                return Low.ToString(format, provider);
            return High.ToString(format, provider) + Low.ToString(format, provider);
        }

        public int CompareTo(UInt128 other)
        {
            if (this > other) return 1;
            return this == other ? 0 : -1;
        }

        public override bool Equals(object obj)
            => obj is UInt128 ui128 && ui128 == this;

        public bool Equals(UInt128 other)
            => this == other;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17 * 31 + Low.GetHashCode();
                return hash * 31 + High.GetHashCode();
            }
        }

        #region operators

        public static UInt128 operator +(UInt128 left, UInt128 right)
        {
            var newLo = unchecked(left.Low + right.Low);
            var newHi = left.High + right.High;
            if (newLo < left.Low) newHi++;
            return new UInt128(newHi, newLo);
        }

        public static UInt128 operator -(UInt128 left, UInt128 right)
            => new UInt128(left.High - right.High - (left.Low < right.Low ? 1UL : 0UL), unchecked(left.Low - right.Low));

        public static UInt128 operator ++(UInt128 value)
        {
            ulong newLo = unchecked(value.Low + 1);
            return new UInt128(newLo != 0 ? value.High : value.High + 1, newLo);
        }

        public static UInt128 operator --(UInt128 value)
            => new UInt128(value.Low != 0 ? value.High : value.High - 1, unchecked(value.Low - 1));

        public static bool operator ==(UInt128 left, UInt128 right)
            => left.High == right.High && left.Low == right.Low;

        public static bool operator !=(UInt128 left, UInt128 right)
            => left.High != right.High || left.Low != right.Low;

        public static bool operator >(UInt128 left, UInt128 right)
            => left.High > right.High || (left.High == right.High && left.Low > right.Low);

        public static bool operator <(UInt128 left, UInt128 right)
            => right.High > left.High || (right.High == left.High && right.Low > left.Low);

        public static bool operator >=(UInt128 left, UInt128 right)
            => left.High > right.High || (left.High == right.High && left.Low >= right.Low);

        public static bool operator <=(UInt128 left, UInt128 right)
            => right.High > left.High || (right.High == left.High && right.Low >= left.Low);

        public static UInt128 operator &(UInt128 left, UInt128 right)
            => new UInt128(left.High & right.High, left.Low & right.Low);

        public static UInt128 operator ~(UInt128 value)
            => new UInt128(~value.High, ~value.Low);

        public static UInt128 operator |(UInt128 left, UInt128 right)
            => new UInt128(left.High | right.High, left.Low | right.Low);

        public static UInt128 operator ^(UInt128 left, UInt128 right)
            => new UInt128(left.High ^ right.High, left.Low ^ right.Low);

        public static UInt128 operator <<(UInt128 value, int shift)
        {
            if (shift == 0) return value;
            return shift >= 64 ? new UInt128(value.Low << (shift - 64), 0UL) : new UInt128((value.High << shift) | (value.Low >> (64 - shift)), value.Low << shift);
        }

        public static UInt128 operator >>(UInt128 value, int shift)
        {
            if (shift == 0) return value;
            return shift >= 64 ? new UInt128(0UL, value.High >> (shift - 64)) : new UInt128(value.High >> shift, (value.Low >> shift) | (value.High << (64 - shift)));
        }

        public static implicit operator UInt128(byte x) => new UInt128(0, x);

        public static implicit operator UInt128(ushort x) => new UInt128(0, x);

        public static implicit operator UInt128(UInt24 x) => new UInt128(0, (ulong)x);

        public static implicit operator UInt128(uint x) => new UInt128(0, x);

        public static implicit operator UInt128(ulong x) => new UInt128(0, x);

        public static explicit operator ulong(UInt128 x)
        {
            if (x.High != 0)
                throw new OverflowException($"{x} does not fit into a Uint64");
            return x.Low;
        }

        #endregion operators
    }
}

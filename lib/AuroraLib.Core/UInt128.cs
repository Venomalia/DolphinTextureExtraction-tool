using System.Globalization;

namespace AuroraLib.Core
{
    /// <summary>
    /// Represents a 16-byte, 128-bit unsigned integer. is mainly used for checksums.
    /// </summary>
    [Serializable]
    public struct UInt128 : IFormattable, IComparable<UInt128>, IEquatable<UInt128>
    {
        public readonly ulong Low, High;

        public static readonly UInt128 MaxValue = new(ulong.MaxValue, ulong.MaxValue);
        public static readonly UInt128 MinValue = new(ulong.MinValue, ulong.MinValue);

        public UInt128(ulong high, ulong low)
        {
            High = high;
            Low = low;
        }

        public UInt128(ReadOnlySpan<char> HexString)
        {
            if (HexString.Length > 16)
            {
                Low = UInt64.Parse(HexString[..16], NumberStyles.HexNumber);
                High = UInt64.Parse(HexString[^16..], NumberStyles.HexNumber);
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

        public override string ToString() => ToString(string.Empty, null);

        public string ToString(IFormatProvider provider) => ToString(string.Empty, provider);

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
            ulong newLo = unchecked(left.Low + right.Low);
            ulong newHi = left.High + right.High;
            if (newLo < left.Low) newHi++;
            return new(newHi, newLo);
        }

        public static UInt128 operator -(UInt128 left, UInt128 right)
            => new(left.High - right.High - (left.Low < right.Low ? 1UL : 0UL), unchecked(left.Low - right.Low));

        public static UInt128 operator ++(UInt128 value)
        {
            ulong newLo = unchecked(value.Low + 1);
            return new(newLo != 0 ? value.High : value.High + 1, newLo);
        }

        public static UInt128 operator --(UInt128 value)
            => new(value.Low != 0 ? value.High : value.High - 1, unchecked(value.Low - 1));

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
            => new(left.High & right.High, left.Low & right.Low);

        public static UInt128 operator ~(UInt128 value)
            => new(~value.High, ~value.Low);

        public static UInt128 operator |(UInt128 left, UInt128 right)
            => new(left.High | right.High, left.Low | right.Low);

        public static UInt128 operator ^(UInt128 left, UInt128 right)
            => new(left.High ^ right.High, left.Low ^ right.Low);

        public static UInt128 operator <<(UInt128 value, int shift)
        {
            if (shift == 0) return value;
            return shift >= 64 ? new(value.Low << (shift - 64), 0UL) : new((value.High << shift) | (value.Low >> (64 - shift)), value.Low << shift);
        }

        public static UInt128 operator >>(UInt128 value, int shift)
        {
            if (shift == 0) return value;
            return shift >= 64 ? new(0UL, value.High >> (shift - 64)) : new(value.High >> shift, (value.Low >> shift) | (value.High << (64 - shift)));
        }

        public static implicit operator UInt128(byte x) => new(0, x);

        public static implicit operator UInt128(ushort x) => new(0, x);

        public static implicit operator UInt128(UInt24 x) => new(0, (ulong)x);

        public static implicit operator UInt128(uint x) => new(0, x);

        public static implicit operator UInt128(ulong x) => new(0, x);

        public static explicit operator ulong(UInt128 x) => x.Low;

        #endregion operators
    }
}

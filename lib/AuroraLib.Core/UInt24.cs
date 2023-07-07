namespace AuroraLib.Core
{
    /// <summary>
    /// Represents a 3-byte, 24-bit unsigned integer.
    /// </summary>
    // base on https://github.com/SuperHackio/Hack.io
    [Serializable]
    public struct UInt24 : IComparable, IFormattable, IConvertible, IComparable<UInt24>, IComparable<uint>, IEquatable<UInt24>, IEquatable<uint>
    {
        private readonly byte b0, b1, b2;

        public const uint MaxValue32 = 0x00ffffff;
        public const uint MinValue32 = 0x00000000;

        /// <summary>
        /// The value of this Int24 as an Int32
        /// </summary>
        public uint Value
            => (uint)(b0 | b1 << 8 | b2 << 16);

        public UInt24(uint value)
        {
            ValidateNumericRange(value);

            b0 = (byte)(value & 0xFF);
            b1 = (byte)(value >> 8 & 0xFF);
            b2 = (byte)(value >> 16 & 0xFF);
        }

        public UInt24(UInt24 value)
        {
            b0 = value.b0;
            b1 = value.b1;
            b2 = value.b2;
        }

        private static void ValidateNumericRange(uint value)
        {
            if (value > MaxValue32)
                throw new OverflowException(string.Format("Value of {0} will not fit in a 24-bit unsigned integer", value));
        }

        public override string ToString() => Value.ToString();

        public string ToString(IFormatProvider provider) => Value.ToString(provider);

        public string ToString(string format) => Value.ToString(format);

        public string ToString(string format, IFormatProvider provider) => Value.ToString(format, provider);

        public override bool Equals(object obj) => obj is UInt24 ui24 && ui24.Value == Value;

        public bool Equals(UInt24 other) => this == other;

        public bool Equals(uint other) => Value == other;

        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(object value)
        {
            if (value == null)
                return 1;
            else if (value is UInt24 num24)
                return CompareTo(num24);
            if (value is uint num32)
                return CompareTo(num32);
            throw new ArgumentException("Argument must be an UInt32 or an UInt24");
        }

        public int CompareTo(UInt24 value)
            => CompareTo(value.Value);

        public int CompareTo(uint value)
        {
            if ((object)value == null)
                return 1;
            return Value < value ? -1 : Value > value ? 1 : 0;
        }

        #region operators

        public static UInt24 operator +(UInt24 a, UInt24 b) => new(a.Value + b.Value);

        public static UInt24 operator -(UInt24 a, UInt24 b) => new(a.Value - b.Value);

        public static UInt24 operator *(UInt24 a, UInt24 b) => new(a.Value * b.Value);

        public static UInt24 operator /(UInt24 a, UInt24 b) => new(a.Value / b.Value);

        public static UInt24 operator %(UInt24 a, UInt24 b) => new(a.Value % b.Value);

        public static UInt24 operator &(UInt24 a, UInt24 b) => new(a.Value & b.Value);

        public static UInt24 operator |(UInt24 a, UInt24 b) => new(a.Value | b.Value);

        public static UInt24 operator ^(UInt24 a, UInt24 b) => new(a.Value ^ b.Value);

        public static uint operator >>(UInt24 a, int b) => a.Value >> b;

        public static uint operator <<(UInt24 a, int b) => a.Value << b;

        public static UInt24 operator ~(UInt24 a) => new(~a.Value);

        public static UInt24 operator ++(UInt24 a) => new(a.Value + 1);

        public static UInt24 operator --(UInt24 a) => new(a.Value - 1);

        public static bool operator ==(UInt24 l, UInt24 r) => l.Value == r.Value;

        public static bool operator !=(UInt24 l, UInt24 r) => l.Value != r.Value;

        public static bool operator >(UInt24 l, UInt24 r) => l.Value > r.Value;

        public static bool operator <(UInt24 l, UInt24 r) => l.Value < r.Value;

        public static bool operator >=(UInt24 l, UInt24 r) => l.Value >= r.Value;

        public static bool operator <=(UInt24 l, UInt24 r) => l.Value <= r.Value;

        public static implicit operator UInt24(byte x) => new(x);

        public static explicit operator byte(UInt24 x) => (byte)x.Value;

        public static explicit operator UInt24(sbyte x) => new((uint)x);

        public static explicit operator sbyte(UInt24 x) => (sbyte)x.Value;

        public static explicit operator UInt24(short x) => new((uint)x);

        public static explicit operator short(UInt24 x) => (short)x.Value;

        public static implicit operator UInt24(ushort x) => new(x);

        public static explicit operator ushort(UInt24 x) => (ushort)x.Value;

        public static explicit operator UInt24(Int24 x) => new((uint)x.Value);

        public static explicit operator Int24(UInt24 x) => new((int)x.Value);

        public static explicit operator UInt24(int x) => new((uint)x);

        public static implicit operator int(UInt24 x) => (int)x.Value;

        public static explicit operator UInt24(uint x) => new(x);

        public static implicit operator uint(UInt24 x) => x.Value;

        public static explicit operator UInt24(long x) => new UInt24((uint)x);

        public static implicit operator long(UInt24 x) => x.Value;

        public static explicit operator UInt24(ulong x) => new UInt24((uint)x);

        public static implicit operator ulong(UInt24 x) => x.Value;

        #endregion operators

        #region IConvertible

        bool IConvertible.ToBoolean(IFormatProvider provider)
            => Convert.ToBoolean(Value, provider);

        char IConvertible.ToChar(IFormatProvider provider)
            => Convert.ToChar(Value, provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider)
            => Convert.ToSByte(Value, provider);

        byte IConvertible.ToByte(IFormatProvider provider)
            => Convert.ToByte(Value, provider);

        short IConvertible.ToInt16(IFormatProvider provider)
            => Convert.ToInt16(Value, provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider)
            => Convert.ToUInt16(Value, provider);

        int IConvertible.ToInt32(IFormatProvider provider)
            => Convert.ToInt32(Value, provider);

        uint IConvertible.ToUInt32(IFormatProvider provider)
            => Value;

        long IConvertible.ToInt64(IFormatProvider provider)
            => Convert.ToInt64(Value, provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider)
            => Convert.ToUInt64(Value, provider);

        float IConvertible.ToSingle(IFormatProvider provider)
            => Convert.ToSingle(Value, provider);

        double IConvertible.ToDouble(IFormatProvider provider)
            => Convert.ToDouble(Value, provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider)
            => Convert.ToDecimal(Value, provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
            => Convert.ToDateTime(Value, provider);

        object IConvertible.ToType(Type type, IFormatProvider provider)
            => Convert.ChangeType(Value, type, provider);

        public TypeCode GetTypeCode()
            => TypeCode.UInt32;

        #endregion IConvertible
    }
}

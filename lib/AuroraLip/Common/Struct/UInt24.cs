using System;

namespace AuroraLip.Common
{

    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    /// <summary>
    /// I need this because Nintendo had this strange idea that using UInt24 is OK
    /// </summary>
    public struct UInt24
    {
        private const uint MaxValue32 = 0x00ffffff;
        private const uint MinValue32 = 0x00000000;
        /// <summary>
        /// 
        /// </summary>
        public const uint BitMask = 0xff000000;
        /// <summary>
        /// The value of this Int24 as an Int32
        /// </summary>
        public uint Value { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public UInt24(uint value)
        {
            ValidateNumericRange(value);
            Value = ApplyBitMask(value);
        }
        private static void ValidateNumericRange(uint value)
        {
            if (value > MaxValue32)
                throw new OverflowException(string.Format("Value of {0} will not fit in a 24-bit unsigned integer", value));
        }
        private static uint ApplyBitMask(uint value) => (value & ~BitMask);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UInt24 operator +(UInt24 a, UInt24 b) => new UInt24(a.Value + b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UInt24 operator -(UInt24 a, UInt24 b) => new UInt24(a.Value - b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UInt24 operator *(UInt24 a, UInt24 b) => new UInt24(a.Value * b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UInt24 operator /(UInt24 a, UInt24 b) => new UInt24(a.Value / b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UInt24 operator %(UInt24 a, UInt24 b) => new UInt24(a.Value % b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UInt24 operator &(UInt24 a, UInt24 b) => new UInt24(a.Value & b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UInt24 operator |(UInt24 a, UInt24 b) => new UInt24(a.Value | b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UInt24 operator ^(UInt24 a, UInt24 b) => new UInt24(a.Value ^ b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static uint operator >>(UInt24 a, int b) => a.Value >> b;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static uint operator <<(UInt24 a, int b) => a.Value << b;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static UInt24 operator ~(UInt24 a) => new UInt24(~a.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static UInt24 operator ++(UInt24 a) => new UInt24(a.Value + 1);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static UInt24 operator --(UInt24 a) => new UInt24(a.Value - 1);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator ==(UInt24 l, UInt24 r) => l.Value == r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator !=(UInt24 l, UInt24 r) => l.Value != r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator >(UInt24 l, UInt24 r) => l.Value > r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator <(UInt24 l, UInt24 r) => l.Value < r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator >=(UInt24 l, UInt24 r) => l.Value >= r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator <=(UInt24 l, UInt24 r) => l.Value <= r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value.ToString();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToString(IFormatProvider provider) => Value.ToString(provider);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToString(string format) => Value.ToString(format);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider provider) => Value.ToString(format, provider);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) => obj is UInt24 ui24 && ui24.Value == Value;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(byte x) => new UInt24(x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator byte(UInt24 x) => (byte)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(sbyte x) => new UInt24((uint)x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator sbyte(UInt24 x) => (sbyte)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(short x) => new UInt24((uint)x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator short(UInt24 x) => (short)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(ushort x) => new UInt24(x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator ushort(UInt24 x) => (ushort)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(Int24 x) => new UInt24((uint)x.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(UInt24 x) => new Int24((int)x.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(int x) => new UInt24((uint)x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator int(UInt24 x) => (int)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(uint x) => new UInt24(x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator uint(UInt24 x) => x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(long x) => new UInt24((uint)x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator long(UInt24 x) => x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(ulong x) => new UInt24((uint)x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator ulong(UInt24 x) => (ulong)x.Value;
    }
}

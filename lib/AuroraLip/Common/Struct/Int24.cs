using System;
using System.Collections;

namespace AuroraLip.Common
{

    /*
    * Super Hackio Incorporated
    * "Copyright © Super Hackio Incorporated 2020-2021"
    * https://github.com/SuperHackio/Hack.io
    */

    /// <summary>
    /// Extra BitArray functions
    /// </summary>
    public static class BitArrayEx
    {
        /// <summary>
        /// Converts this BitArray to an Int32
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int ToInt32(this BitArray array)
        {
            if (array.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] Finalarray = new int[1];
            array.CopyTo(Finalarray, 0);
            return Finalarray[0];
        }
    }

    /// <summary>
    /// I need this because Nintendo had this strange idea that using Int24 is OK
    /// </summary>
    public struct Int24
    {
        /// <summary>
        /// 
        /// </summary>
        public const int MaxValue = 8388607;
        /// <summary>
        /// 
        /// </summary>
        public const int MinValue = -8388608;
        /// <summary>
        /// 
        /// </summary>
        public const int BitMask = -16777216;
        /// <summary>
        /// The value of this Int24 as an Int32
        /// </summary>
        public int Value { get; }
        /// <summary>
        /// Create a new Int24
        /// </summary>
        /// <param name="Value"></param>
        public Int24(int Value)
        {
            ValidateNumericRange(Value);
            this.Value = ApplyBitMask(Value);
        }

        private static void ValidateNumericRange(int value)
        {
            if (value > (MaxValue + 1) || value < MinValue)
                throw new OverflowException($"Value of {value} will not fit in a 24-bit signed integer");
        }
        private static int ApplyBitMask(int value) => (value & 0x00800000) > 0 ? value | BitMask : value & ~BitMask;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int24 operator +(Int24 a, Int24 b) => new Int24(a.Value + b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int24 operator -(Int24 a, Int24 b) => new Int24(a.Value - b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int24 operator *(Int24 a, Int24 b) => new Int24(a.Value * b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int24 operator /(Int24 a, Int24 b) => new Int24(a.Value / b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int24 operator %(Int24 a, Int24 b) => new Int24(a.Value % b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int24 operator &(Int24 a, Int24 b) => new Int24(a.Value & b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int24 operator |(Int24 a, Int24 b) => new Int24(a.Value | b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Int24 operator ^(Int24 a, Int24 b) => new Int24(a.Value ^ b.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int operator >>(Int24 a, int b) => a.Value >> b;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int operator <<(Int24 a, int b) => a.Value << b;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Int24 operator ~(Int24 a) => new Int24(~a.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Int24 operator ++(Int24 a) => new Int24(a.Value + 1);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Int24 operator --(Int24 a) => new Int24(a.Value - 1);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator ==(Int24 l, Int24 r) => l.Value == r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator !=(Int24 l, Int24 r) => l.Value != r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator >(Int24 l, Int24 r) => l.Value > r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator <(Int24 l, Int24 r) => l.Value < r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator >=(Int24 l, Int24 r) => l.Value >= r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool operator <=(Int24 l, Int24 r) => l.Value <= r.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int operator +(byte a, Int24 b) => a + b.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int operator +(short a, Int24 b) => a + b.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int operator +(ushort a, Int24 b) => a + b.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int operator +(int a, Int24 b) => a + b.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static long operator +(uint a, Int24 b) => a + b.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static long operator +(long a, Int24 b) => a + b.Value;
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
        public override bool Equals(object obj) => obj is Int24 i24 && i24.Value == Value;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(byte x) => new Int24(x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator byte(Int24 x) => (byte)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(sbyte x) => new Int24(x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator sbyte(Int24 x) => (sbyte)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(short x) => new Int24(x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator short(Int24 x) => (short)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(ushort x) => new Int24(x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator ushort(Int24 x) => (ushort)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(UInt24 x) => new Int24((int)x.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator UInt24(Int24 x) => new UInt24((uint)x.Value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(int x) => new Int24(x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator int(Int24 x) => x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(uint x) => new Int24((int)x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator uint(Int24 x) => (uint)x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(long x) => new Int24((int)x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator long(Int24 x) => x.Value;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator Int24(ulong x) => new Int24((int)x);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public static explicit operator ulong(Int24 x) => (ulong)x.Value;
    }
}
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Common
{
    /// <summary>
    /// Extra BitConverter functions
    /// </summary>
    public static class BitConverterEx
    {
        #region Convert Int24

        /// <summary>
        /// Returns a 24-bit signed integer converted from three bytes at a specified position.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="StartIndex">The starting position within value.</param>
        /// <returns>A 24-bit signed integer formed by three bytes beginning at startIndex.</returns>
        [DebuggerStepThrough]
        public static Int24 ToInt24(ReadOnlySpan<byte> value)
            => new(value[0] | value[1] << 8 | value[2] << 16);

        /// <summary>
        /// Returns the specified 24-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 3</returns>
        [DebuggerStepThrough]
        public static byte[] GetBytes(Int24 value)
            => new byte[3] { (byte)value.Value, (byte)(value.Value >> 8), (byte)(value.Value >> 16) };

        /// <summary>
        /// Returns a 24-bit unsigned integer converted from three bytes at a specified position.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="StartIndex">The starting position within value.</param>
        /// <returns>A 24-bit unsigned integer formed by three bytes beginning at startIndex.</returns>
        [DebuggerStepThrough]
        public static UInt24 ToUInt24(ReadOnlySpan<byte> value)
            => new UInt24((uint)(value[0] | value[1] << 8 | value[2] << 16));

        /// <summary>
        /// Returns the specified 24-bit unsigned integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 3/returns>
        [DebuggerStepThrough]
        public static byte[] GetBytes(UInt24 value)
            => new byte[3] { (byte)value.Value, (byte)(value.Value >> 8), (byte)(value.Value >> 16) };

        #endregion Convert Int24

        #region ByteOrder

        /// <summary>
        /// Reverses the byte order of the specified buffer based on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to reverse.</param>
        /// <param name="type">The type of the data in the buffer.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReversesByteOrder(this Span<byte> buffer, Type type)
        {
            if (type.IsPrimitive || type == typeof(UInt24) || type == typeof(Int24))
            {
                buffer.Reverse();
                return;
            }

            int offset = 0;
            foreach (int FieldSize in GetPrimitiveTypeSizes(type))
            {
                buffer.Slice(offset, FieldSize).Reverse();
                offset += FieldSize;
            }
        }
        /// <inheritdoc cref="ReversesByteOrder(Span{byte}, Type)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReversesByteOrder<T>(this Span<byte> buffer) => buffer.ReversesByteOrder(typeof(T));

        /// <summary>
        /// Retrieves a list the sizes of primitive types and nested primitive types within the specified type.
        /// </summary>
        /// <param name="type">The type to analyze.</param>
        /// <returns>An array containing the sizes of primitive types within the specified type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static ReadOnlySpan<int> GetPrimitiveTypeSizes(Type type)
        {
            lock (TypePrimitives)
            {
                if (type.IsValueType && TypePrimitives.TryGetValue(type, out int[] primitives))
                {
                    return primitives;
                }

                List<int> primList = new();
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (FieldInfo field in fields)
                {
                    if (field.IsStatic) continue;

                    Type fieldtype = field.FieldType;

                    if (fieldtype.IsEnum)
                    {
                        fieldtype = Enum.GetUnderlyingType(fieldtype);
                    }

                    if (fieldtype.IsPrimitive || fieldtype == typeof(UInt24) || fieldtype == typeof(Int24))
                    {
                        primList.Add(Marshal.SizeOf(fieldtype));
                    }
                    else
                    {
                        primList.AddRange(GetPrimitiveTypeSizes(fieldtype).ToArray());
                    }
                }
                primitives = primList.ToArray();
                TypePrimitives.Add(type, primitives);
                return primitives;
            }
        }
        private static readonly Dictionary<Type, int[]> TypePrimitives = new();

        /// <summary>
        /// Flip the ByteOrder of the 8-bit unsigned integer.
        /// 11001000 => 00010011
        /// </summary>
        [DebuggerStepThrough]
        public static byte Swap(this byte b)
            => (byte)((b * 0x0202020202ul & 0x010884422010ul) % 1023);

        /// <summary>
        /// Swaps the alternate bits of a byte value.
        /// 11001000 = 11000100
        /// </summary>
        [DebuggerStepThrough]
        public static byte SwapAlternateBits(this byte value)
            => (byte)(((value & 0xAA) >> 1) | ((value & 0x55) << 1));

        /// <summary>
        /// Flip the ByteOrder of the 16-bit unsigned integer.
        /// </summary>
        [DebuggerStepThrough]
        public static ushort Swap(this ushort value)
            => (ushort)(((value & 0xFF) << 8) | ((value >> 8) & 0xFF));

        /// <summary>
        /// Flip the ByteOrder of the 16-bit signed integer.
        /// </summary>
        [DebuggerStepThrough]
        public static short Swap(this short value)
            => (short)Swap((ushort)value);

        /// <summary>
        /// Flip the ByteOrder of the 32-bit unsigned integer.
        /// </summary>
        [DebuggerStepThrough]
        public static uint Swap(this uint value)
            => ((value & 0x000000ff) << 24) | ((value & 0x0000ff00) << 8) | ((value & 0x00ff0000) >> 8) | ((value & 0xff000000) >> 24);

        /// <summary>
        /// Flip the ByteOrder of the 32-bit signed integer.
        /// </summary>
        [DebuggerStepThrough]
        public static int Swap(this int value)
            => (int)Swap((uint)value);

        /// <summary>
        /// Flip the ByteOrder of the 64-bit unsigned integer.
        /// </summary>
        [DebuggerStepThrough]
        public static ulong Swap(this ulong value)
            => ((0x00000000000000FF) & (value >> 56) | (0x000000000000FF00) & (value >> 40) | (0x0000000000FF0000) & (value >> 24) | (0x00000000FF000000) & (value >> 8) |
            (0x000000FF00000000) & (value << 8) | (0x0000FF0000000000) & (value << 24) | (0x00FF000000000000) & (value << 40) | (0xFF00000000000000) & (value << 56));

        /// <summary>
        /// Flip the ByteOrder of the 64-bit signed integer.
        /// </summary>
        [DebuggerStepThrough]
        public static long Swap(this long value)
            => (long)Swap((ulong)value);

        #endregion ByteOrder

        #region Nibbles

        /// <summary>
        /// Get a bit from a 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns>bit as bool</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static bool GetBit(this byte b, int index = 0)
            => (b & (1 << index)) != 0;

        /// <summary>
        /// Get a bit from a 16-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns>bit as bool</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static bool GetBit(this short b, int index = 0)
            => (b & (1 << index)) != 0;

        /// <summary>
        /// Get a bit from a 32-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns>bit as bool</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static bool GetBit(this int b, int index = 0)
            => (b & (1 << index)) != 0;

        /// <summary>
        /// Get a bit from a 64-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns>bit as bool</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static bool GetBit(this long b, int index = 0)
            => (b & (1 << index)) != 0;

        /// <summary>
        /// Set a bit in a 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be set</param>
        /// <param name="value"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte SetBit(this byte b, int index, bool value)
        {
            if (value)
                return (byte)(b | (1 << index));
            else
                return (byte)(b & ~(1 << index));
        }

        /// <summary>
        /// Set a bit in a 32-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be set</param>
        /// <param name="value"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static int SetBit(this int b, int index, bool value)
        {
            if (value)
                return (b | (1 << index));
            else
                return (b & ~(1 << index));
        }

        /// <summary>
        /// Get two 4-bit signed integer from a 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static (int low, int high) GetNibbles(this byte b)
            => (b & 0xf, b & 0xf0 >> 4);

        /// <summary>
        /// Get a 4-bit signed integer from a 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static int GetUInt4(this byte b, int index)
            => b & (0xf << index) >> index;

        /// <summary>
        /// Get a 4-bit signed integer from a 16-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static int GetUInt4(this short b, int index)
            => b & (0xf << index) >> index;

        /// <summary>
        /// Get a 4-bit signed integer from a 32-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static int GetUInt4(this int b, int index)
            => b & (0xf << index) >> index;

        /// <summary>
        /// Get a 4-bit signed integer from a 64-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static int GetUInt4(this long b, int index)
            => (int)b & (0xf << index) >> index;

        /// <summary>
        /// Get a <paramref name="length"/>-bit signed integer from a 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <param name="length">length of the bits to be read</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static int GetBits(this byte b, int index, int length)
            => b >> index - length & (1 << length) - 1;

        /// <summary>
        /// Get a <paramref name="length"/>-bit signed integer from a 16-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <param name="length">length of the bits to be read</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static int GetBits(this short b, int index, int length)
            => b >> index - length & (1 << length) - 1;

        /// <summary>
        /// Get a <paramref name="length"/>-bit signed integer from a 32-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <param name="length">length of the bits to be read</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static int GetBits(this int b, int index, int length)
            => b >> index - length & (1 << length) - 1;

        /// <summary>
        /// Get a <paramref name="length"/>-bit signed integer from a 64-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <param name="length">length of the bits to be read</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static long GetBits(this long b, int index, int length)
            => b >> index - length & (1 << length) - 1;

        /// <summary>
        /// Read single bits as steam of bools
        /// </summary>
        /// <param name="b"></param>
        /// <param name="byteorder">Byte order, in which bytes are read</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IEnumerable<bool> GetBits(this byte b, Endian byteorder = Endian.Little)
        {
            if (byteorder == Endian.Little)
            {
                for (int i = 0; i < 8; i++)
                    yield return b.GetBit(i);
            }
            else
            {
                for (int i = 7; i >= 0; i--)
                    yield return b.GetBit(i);
            }
        }

        #endregion Nibbles

        /// <summary>
        /// Converts this BitArray to an Int32
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static int ToInt32(this BitArray array)
        {
            if (array.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] Finalarray = new int[1];
            array.CopyTo(Finalarray, 0);
            return Finalarray[0];
        }

        #region DataXor
        /// <summary>
        /// Performs an XOR operation between the elements of the <paramref name="data"/> span and the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="data">The span of data to perform XOR on.</param>
        /// <param name="key">The key value to use for XOR operation.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DataXor(this Span<byte> data, byte key)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ key);
            }
        }

        /// <summary>
        /// Performs an XOR operation between the elements of the <paramref name="data"/> span and the <paramref name="key"/> span.
        /// </summary>
        /// <param name="data">The span of data to perform XOR on.</param>
        /// <param name="key">The span of the key to use for XOR operation.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DataXor(this Span<byte> data, ReadOnlySpan<byte> key)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
        }
        #endregion
    }
}

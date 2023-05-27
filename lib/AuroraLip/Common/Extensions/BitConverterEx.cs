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
        /// Flip the ByteOrder for each field of the given <paramref name="type"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="type"></param>
        [DebuggerStepThrough]
        public static void FlipByteOrder(this Span<byte> buffer, Type type)
        {
            if (type.IsPrimitive || type == typeof(UInt24) || type == typeof(Int24))
            {
                buffer.Reverse();
                return;
            }

            int subOffset = 0, fieldSize;

            FieldInfo[] fields;
            lock (TypeFields)
            {
                if (!TypeFields.TryGetValue(type, out fields))
                {
                    fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    TypeFields.Add(type, fields);
                }
            }

            foreach (var field in fields)
            {
                if (field.IsStatic) continue;

                Type fieldtype = field.FieldType;

                if (fieldtype.IsEnum)
                    fieldtype = Enum.GetUnderlyingType(fieldtype);

                fieldSize = Marshal.SizeOf(fieldtype);
                buffer.Slice(subOffset, fieldSize).FlipByteOrder(fieldtype);
                subOffset += fieldSize;
            }
        }

        private static readonly Dictionary<Type, FieldInfo[]> TypeFields = new();

        /// <summary>
        /// Flip the ByteOrder of the 8-bit unsigned integer.
        /// </summary>
        [DebuggerStepThrough]
        public static byte Swap(this byte b)
            => (byte)((b * 0x0202020202ul & 0x010884422010ul) % 1023);

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

        /// <summary>
        /// XOR each byte with the given key
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte[] DataXor(this byte[] data, byte key)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ key);
            }
            return data;
        }

        /// <summary>
        /// XOR each byte with the given key array
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte[] DataXor(this byte[] data, byte[] key)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return data;
        }
    }
}

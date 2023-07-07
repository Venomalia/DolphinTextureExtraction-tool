using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Core
{
    /// <summary>
    /// BitConverter Extra functions
    /// </summary>
    public static class BitConverterX
    {
        #region ConvertGeneric
        /// <summary>
        /// Converts a read-only span of bytes to an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="value">The read-only span of bytes.</param>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T To<T>(ReadOnlySpan<byte> value) where T : unmanaged
        {
            if (value.Length < sizeof(T))
            {
                throw new ArgumentException($"The input span must have a length of {sizeof(T)} bytes.", nameof(value));
            }

            T result;
            fixed (byte* pValue = value)
            {
                result = *(T*)pValue;
            }

            return result;
        }

        /// <summary>
        /// Converts a instance of <typeparamref name="T"/> to a byte array.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>A byte array representing the value.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte[] GetBytes<T>(T value) where T : unmanaged
        {
            byte[] result = new byte[sizeof(T)];

            fixed (byte* pResult = result)
            {
                T* pValue = (T*)pResult;
                *pValue = value;
            }

            return result;
        }

        /// <summary>
        /// Tries to write the bytes of a instance of <typeparamref name="T"/> to the specified destination span.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="destination">The destination span.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>true if the bytes were successfully written; otherwise, false.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryWriteBytes<T>(Span<byte> destination, T value) where T : unmanaged
        {
            if (destination.Length < sizeof(T))
            {
                return false;
            }

            fixed (byte* destinationPtr = &destination.GetPinnableReference())
            {
                T* valuePtr = (T*)destinationPtr;
                *valuePtr = value;
            }

            return true;
        }
        #endregion

        #region SwapGeneric
        /// <summary>
        /// Swaps the byte order of the specified instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="vaule"></param>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe T Swap<T>(T vaule) where T : unmanaged
        {
            Span<byte> src = new(&vaule, sizeof(T));
            Swap<T>(src);
            return vaule;
        }

        /// <summary>
        /// Swaps the byte order of the specified buffer based on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to reverse.</param>
        /// <param name="type">The type of the data in the buffer.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Swap(Span<byte> buffer, Type type)
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

        /// <inheritdoc cref="Swap(Span{byte}, Type)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(Span<byte> buffer)
            => Swap(buffer, typeof(T));

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
                    return primitives;

                List<int> primList = new();
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (FieldInfo field in fields)
                {
                    if (field.IsStatic) continue;

                    Type fieldtype = field.FieldType;

                    if (fieldtype.IsEnum)
                        fieldtype = Enum.GetUnderlyingType(fieldtype);

                    if (fieldtype.IsPrimitive || fieldtype == typeof(UInt24) || fieldtype == typeof(Int24))
                        primList.Add(Marshal.SizeOf(fieldtype));
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
        #endregion

        #region Swap
        /// <summary>
        /// Swaps the bits in the given byte value.
        /// </summary>
        /// <param name="value">The byte value to swap the bits for.</param>
        /// <returns>The byte value with swapped bits.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Swap(byte value)
            => (byte)((value * 0x0202020202ul & 0x010884422010ul) % 1023);

        /// <summary>
        /// Swaps the bits in the given sbyte value.
        /// </summary>
        /// <param name="value">The sbyte value to swap the bits for.</param>
        /// <returns>The byte value with swapped bits.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte Swap(sbyte value)
            => (sbyte)Swap((byte)value);

        /// <summary>
        /// Swaps the alternate bits of a byte value.
        /// </summary>
        /// <param name="value">The byte value to swap the bits for.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SwapAlternateBits(byte value)
            => (byte)((value & 0xAA) >> 1 | (value & 0x55) << 1);

        /// <summary>
        /// Swaps the alternate bits of a sbyte value.
        /// </summary>
        /// <param name="value">The sbyte value to swap the bits for.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte SwapAlternateBits(sbyte value)
            => (sbyte)SwapAlternateBits((byte)value);

        /// <summary>
        /// Swaps the bytes in the 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">The ushort value to swap the bytes for.</param>
        /// <returns>The ushort value with swapped bytes.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Swap(ushort value)
            => (ushort)((value & 0xFF) << 8 | value >> 8 & 0xFF);

        /// <summary>
        /// Swaps the bytes in the 16-bit signed integer.
        /// </summary>
        /// <param name="value">The short value to swap the bytes for.</param>
        /// <returns>The short value with swapped bytes.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Swap(short value)
            => (short)Swap((ushort)value);

        /// <summary>
        /// Swaps the bytes in the 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The uint value to swap the bytes for.</param>
        /// <returns>The uint value with swapped bytes.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Swap(uint value)
            => (value & 0x000000ff) << 24 | (value & 0x0000ff00) << 8 | (value & 0x00ff0000) >> 8 | (value & 0xff000000) >> 24;

        /// <summary>
        /// Swaps the bytes in the 32-bit signed integer.
        /// </summary>
        /// <param name="value">The int value to swap the bytes for.</param>
        /// <returns>The int value with swapped bytes.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Swap(int value)
            => (int)Swap((uint)value);

        /// <summary>
        /// Swaps the bytes in the 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">The ulong value to swap the bytes for.</param>
        /// <returns>The ulong value with swapped bytes.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Swap(ulong value)
            => 0x00000000000000FF & value >> 56 | 0x000000000000FF00 & value >> 40 | 0x0000000000FF0000 & value >> 24 | 0x00000000FF000000 & value >> 8 |
            0x000000FF00000000 & value << 8 | 0x0000FF0000000000 & value << 24 | 0x00FF000000000000 & value << 40 | 0xFF00000000000000 & value << 56;

        /// <summary>
        /// Swaps the bytes in the 64-bit signed integer.
        /// </summary>
        /// <param name="value">The long value to swap the bytes for.</param>
        /// <returns>The long value with swapped bytes.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Swap(long value)
            => (long)Swap((ulong)value);
        #endregion

        #region GetBit
        /// <summary>
        /// Get a bit from a 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns>bit as bool</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(byte b, int index = 0)
            => (b & 1 << index) != 0;

        /// <summary>
        /// Get a bit from a 16-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns>bit as bool</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(short b, int index = 0)
            => (b & 1 << index) != 0;

        /// <summary>
        /// Get a bit from a 32-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns>bit as bool</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(int b, int index = 0)
            => (b & 1 << index) != 0;

        /// <summary>
        /// Get a bit from a 64-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <returns>bit as bool</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(long b, int index = 0)
            => (b & 1 << index) != 0;
        #endregion

        #region GetBits
        /// <summary>
        /// Get a <paramref name="length"/>-bit signed integer from a 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <param name="length">length of the bits to be read</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBits(byte b, int index, int length)
            => b >> index - length & (1 << length) - 1;

        /// <summary>
        /// Get a <paramref name="length"/>-bit signed integer from a 16-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <param name="length">length of the bits to be read</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBits(short b, int index, int length)
            => b >> index - length & (1 << length) - 1;

        /// <summary>
        /// Get a <paramref name="length"/>-bit signed integer from a 32-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <param name="length">length of the bits to be read</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBits(int b, int index, int length)
            => b >> index - length & (1 << length) - 1;

        /// <summary>
        /// Get a <paramref name="length"/>-bit signed integer from a 64-bit signed integer.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="index">bit position that should be read</param>
        /// <param name="length">length of the bits to be read</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetBits(long b, int index, int length)
            => b >> index - length & (1 << length) - 1;

        /// <summary>
        /// Read single bits as steam of bools
        /// </summary>
        /// <param name="b"></param>
        /// <param name="byteorder">Byte order, in which bytes are read</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IEnumerable<bool> GetBits(byte b, Endian byteorder = Endian.Little)
        {
            if (byteorder == Endian.Little)
            {
                for (int i = 0; i < 8; i++)
                    yield return GetBit(b, i);
            }
            else
            {
                for (int i = 7; i >= 0; i--)
                    yield return GetBit(b, i);
            }
        }
        #endregion

        #region SetBit
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
                return (byte)(b | 1 << index);
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
                return b | 1 << index;
            else
                return b & ~(1 << index);
        }
        #endregion

        #region Nibbles
        /// <summary>
        /// Get two 4-bit signed integer from a 8-bit unsigned integer.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int low, int high) GetNibbles(this byte b)
            => (b & 0xf, b & 0xf0 >> 4);
        #endregion Nibbles

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

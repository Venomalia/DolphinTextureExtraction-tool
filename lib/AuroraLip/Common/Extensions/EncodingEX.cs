using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace AuroraLib.Common
{
    public static class EncodingEX
    {
        public static Encoding DefaultEncoding { get; set; } = Encoding.GetEncoding(28591);

        internal static readonly Predicate<byte> InvalidByte = b => b < 32 || b == 127;

        #region GetString
        /// <summary>
        /// Converts a span of bytes to a string using the default character encoding.
        /// </summary>
        /// <param name="bytes">The span of bytes to convert.</param>
        /// <returns>The resulting string.</returns>
        [DebuggerStepThrough]
        public static string GetString(ReadOnlySpan<byte> bytes)
        {
            Span<char> chars = stackalloc char[bytes.Length];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)bytes[i];
            }
            return new(chars);
        }

        /// <summary>
        /// Converts a span of bytes to a string using the specified character encoding.
        /// </summary>
        /// <param name="bytes">The span of bytes to convert.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>The resulting string.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(ReadOnlySpan<byte> bytes, Encoding encoding)
            => encoding.GetString(bytes);

        /// <summary>
        /// Converts a span of bytes to a string, stopping at the specified terminator byte.
        /// </summary>
        /// <param name="bytes">The span of bytes to convert.</param>
        /// <param name="terminator">The terminator byte indicating the end of the string.</param>
        /// <returns>The resulting string.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(ReadOnlySpan<byte> bytes, in byte terminator)
        {
            int end = bytes.IndexOf(terminator);
            return GetString(bytes[..(end == -1 ? bytes.Length : end)]);
        }

        /// <summary>
        /// Converts a span of bytes to a string using the specified encoding, stopping at the specified terminator byte.
        /// </summary>
        /// <param name="bytes">The span of bytes to convert.</param>
        /// <param name="encoding">The encoding to use for the conversion.</param>
        /// <param name="terminator">The terminator byte indicating the end of the string.</param>
        /// <returns>The resulting string.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(ReadOnlySpan<byte> bytes, Encoding encoding, in byte terminator)
        {
            int end = bytes.IndexOf(terminator);
            return GetString(bytes[..(end == -1 ? bytes.Length : end)], encoding);
        }
        #endregion

        #region GetValidString
        /// <summary>
        /// Converts a span of bytes to a string, excluding invalid characters (bytes with values less than 0x20 or equal to 127).
        /// </summary>
        /// <param name="bytes">The span of bytes to convert.</param>
        /// <returns>The resulting string.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetValidString(ReadOnlySpan<byte> bytes)
            => GetString(bytes[..ValidSize(bytes)]);

        /// <summary>
        /// Converts a span of bytes to a string using the specified encoding, excluding invalid characters (bytes with values less than 0x20 or equal to 127).
        /// </summary>
        /// <param name="bytes">The span of bytes to convert.</param>
        /// <param name="encoding">The encoding to use for the conversion.</param>
        /// <returns>The resulting string.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetValidString(ReadOnlySpan<byte> bytes, Encoding encoding)
            => GetString(bytes[..ValidSize(bytes)], encoding);

        internal static int ValidSize(ReadOnlySpan<byte> bytes)
        {
            int end = bytes.Length;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] < 0x20 || bytes[i] == 127)
                {
                    end = i;
                    break;
                }
            }
            return end;
        }

        /// <summary>
        /// Converts a span of bytes to a string, excluding bytes that match the specified invalid byte predicate.
        /// </summary>
        /// <param name="bytes">The span of bytes to convert.</param>
        /// <param name="invalidByte">The predicate used to determine if a byte is invalid.</param>
        /// <returns>The resulting string.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetValidString(ReadOnlySpan<byte> bytes, Predicate<byte> invalidByte)
            => GetString(bytes[..ValidSize(bytes, invalidByte)]);

        /// <summary>
        /// Converts a span of bytes to a string using the specified encoding, excluding bytes that match the specified invalid byte predicate.
        /// </summary>
        /// <param name="bytes">The span of bytes to convert.</param>
        /// <param name="encoding">The encoding to use for the conversion.</param>
        /// <param name="invalidByte">The predicate used to determine if a byte is invalid.</param>
        /// <returns>The resulting string.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetValidString(ReadOnlySpan<byte> bytes, Encoding encoding, Predicate<byte> invalidByte)
            => GetString(bytes[..ValidSize(bytes, invalidByte)], encoding);

        internal static int ValidSize(ReadOnlySpan<byte> bytes, Predicate<byte> invalidByte)
        {
            int end = bytes.Length;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (invalidByte.Invoke(bytes[i]))
                {
                    end = i;
                    break;
                }
            }
            return end;
        }
        #endregion


        /// <summary>
        ///
        /// </summary>
        /// <param name="String"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte[] GetBytes(this string String)
            => DefaultEncoding.GetBytes(String);

    }
}

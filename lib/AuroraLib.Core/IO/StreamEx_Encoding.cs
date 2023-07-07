using AuroraLib.Core.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace AuroraLib.Core.IO
{
    public static partial class StreamEx
    {
        #region ReadString
        /// <summary>
        /// Reads a String from the Stream. String are terminated by "<paramref name="validbytes"/> == false".
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(this Stream stream)
            => EncodingX.GetString(stream.ReadStringBytes(EncodingX.InvalidByte));

        /// <summary>
        /// Reads a string from the specified <paramref name="stream"/> using the specified <paramref name="encoding"/>.
        /// The string is read until an invalid byte is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use when getting the string.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(this Stream stream, Encoding Encoding)
            => Encoding.GetString(ReadStringBytes(stream, EncodingX.InvalidByte));

        /// <summary>
        /// Reads a string from the specified <paramref name="stream"/>.
        /// The string is read until the specified <paramref name="terminator"/> byte is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="terminator">The byte that indicates the end of the string.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(this Stream stream, byte terminator)
            => EncodingX.GetString(ReadStringBytes(stream, s => s == terminator));

        /// <summary>
        /// Reads a string from the specified <paramref name="stream"/> using the specified <paramref name="encoding"/>.
        /// The string is read until the specified <paramref name="terminator"/> byte is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use when getting the string.</param>
        /// <param name="terminator">The byte that indicates the end of the string.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(this Stream stream, Encoding Encoding, byte terminator)
            => Encoding.GetString(ReadStringBytes(stream, s => s == terminator));

        /// <summary>
        /// Reads a string from the specified <paramref name="stream"/> and stops reading when the specified <paramref name="stopByte"/> is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="stopByte">The stop byte predicate that determines when to stop reading.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(this Stream stream, Predicate<byte> stopByte)
            => EncodingX.GetString(ReadStringBytes(stream, stopByte));

        /// <summary>
        /// Reads a string from the <paramref name="stream"/> using the specified <paramref name="encoding"/> and stops reading when the specified <paramref name="stopByte"/> is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use for converting the read bytes to a string.</param>
        /// <param name="stopByte">The stop byte predicate that determines when to stop reading.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(this Stream stream, Encoding encoding, Predicate<byte> stopByte)
            => encoding.GetString(ReadStringBytes(stream, stopByte));

        private static byte[] ReadStringBytes(this Stream stream, Predicate<byte> stopByte)
        {
            List<byte> bytes = new();
            int readByte;
            do
            {
                readByte = stream.ReadByte();
                if (readByte == -1)
                    throw new EndOfStreamException();

                if (stopByte.Invoke((byte)readByte))
                    break;

                bytes.Add((byte)readByte);

            } while (true);

            return bytes.ToArray();
        }

        /// <summary>
        /// Reads a string by reading the specified number of bytes from the specified <paramref name="stream"/>.
        /// <paramref name="Padding"/> bytes are removed from the resulting string.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The length of the string to read.</param>
        /// <param name="Padding">The byte to be removed from the resulting string if found (optional).</param>
        /// <returns>The string read from the stream with the terminator byte removed if present.</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream FS, int length, byte Padding = 0)
        {
            Span<byte> bytes = stackalloc byte[length];
            FS.Read(bytes);
            return EncodingX.GetString(bytes, Padding);
        }

        /// <summary>
        /// Reads a string by reading the specified number of bytes from the specified <paramref name="stream"/> using the specified <paramref name="encoding"/>.
        /// <paramref name="Padding"/> bytes are removed from the resulting string.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="encoding">The encoding to use for converting the read bytes to a string.</param>
        /// <param name="Padding">The byte to be removed from the resulting string if found (optional).</param>
        /// <returns>The string read from the stream with the terminator byte removed if present.</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream FS, int length, Encoding encoding, byte Padding = 0)
        {
            Span<byte> bytes = stackalloc byte[length];
            FS.Read(bytes);
            return EncodingX.GetString(bytes, encoding, Padding);
        }

        #endregion

        #region WriteString
        /// <summary>
        /// Writes a sequence of characters to the <paramref name="stream"/> using the specified <paramref name="encoding"/>.
        /// won't be null terminated.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="chars">The characters to write.</param>
        /// <param name="encoding">The encoding to use for converting characters to bytes.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, ReadOnlySpan<char> chars, Encoding encoding)
        {
            Span<byte> buffer = stackalloc byte[encoding.GetByteCount(chars)];
            encoding.GetBytes(chars, buffer);
            stream.Write(buffer);
        }

        /// <inheritdoc cref="Write(Stream, ReadOnlySpan{char}, Encoding)"/>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, ReadOnlySpan<char> chars)
            => stream.Write(chars, EncodingX.DefaultEncoding);

        /// <summary>
        /// Writes a specified number of characters to the <paramref name="stream"/> using the specified <paramref name="encoding"/> and adds a <paramref name="terminator"/> <see cref="byte"/> at the end.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="chars">The characters to write.</param>
        /// <param name="encoding">The encoding to use for converting characters to bytes.</param>
        /// <param name="length">The maximum number of bytes to write.</param>
        /// <param name="terminator">The terminator byte to add at the end (default is 0x0).</param>
        /// <exception cref="ArgumentException">Thrown when the encoded bytes exceed the specified length.</exception>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, ReadOnlySpan<char> chars, Encoding encoding, int length, byte terminator = 0x0)
        {
            if (encoding.GetByteCount(chars) > length)
            {
                throw new ArgumentException();
            }

            Span<byte> buffer = stackalloc byte[length];
            buffer.Fill(terminator);
            encoding.GetBytes(chars, buffer);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a specified number of characters to the <paramref name="stream"/> and adds a <paramref name="terminator"/> <see cref="byte"/> at the end.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="chars">The characters to write.</param>
        /// <param name="length">The maximum number of bytes to write.</param>
        /// <param name="terminator">The terminator byte to add at the end (default is 0x0).</param>
        /// <exception cref="ArgumentException">Thrown when the encoded bytes exceed the specified length.</exception>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream stream, ReadOnlySpan<char> chars, int length, byte terminator = 0x0)
            => stream.Write(chars, EncodingX.DefaultEncoding, length, terminator);

        /// <inheritdoc cref="Write(Stream, ReadOnlySpan{char}, int, byte)"/>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteString(this Stream stream, ReadOnlySpan<char> chars, byte terminator = 0x0)
        {
            stream.Write(chars);
            stream.WriteByte(terminator);
        }
        #endregion

        #region MatchString
        /// <summary>
        /// Matches the specified characters with the data in the <paramref name="stream"/> using the specified <paramref name="encoding"/>.
        /// </summary>
        /// <param name="stream">The stream to match against.</param>
        /// <param name="chars">The characters to match.</param>
        /// <param name="encoding">The encoding used for converting characters to bytes.</param>
        /// <returns>true if the specified characters match the data in the stream; otherwise, false.</returns>
        [DebuggerStepThrough]
        public static bool Match(this Stream stream, ReadOnlySpan<char> chars, Encoding encoding)
        {
            Span<byte> buffer = stackalloc byte[encoding.GetByteCount(chars)];
            encoding.GetBytes(chars, buffer);
            return stream.Match(buffer);
        }

        /// <summary>
        /// Matches the specified characters with the data in the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to match against.</param>
        /// <param name="chars">The characters to match.</param>
        /// <returns>true if the specified characters match the data in the stream; otherwise, false.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Match(this Stream stream, ReadOnlySpan<char> chars)
            => stream.Match(chars, EncodingX.DefaultEncoding);

        #endregion

        #region WriteAlignString
        /// <summary>
        /// Writes padding to the <paramref name="stream"/> to align the position to the specified <paramref name="boundary"/>, using the provided characters and <paramref name="encoding"/>.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="boundary">The desired alignment boundary.</param>
        /// <param name="chars">The characters to use for padding.</param>
        /// <param name="encoding">The encoding used to convert the characters to bytes.</param>
        [DebuggerStepThrough]
        public static void WriteAlign(this Stream stream, int boundary, ReadOnlySpan<char> chars, Encoding encoding)
        {
            Span<byte> buffer = stackalloc byte[encoding.GetByteCount(chars)];
            encoding.GetBytes(chars, buffer);
            stream.WriteAlign(boundary, buffer);
        }

        /// <summary>
        /// Writes padding to the <paramref name="stream"/> to align the position to the specified <paramref name="boundary"/>, using the provided characters.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="boundary">The desired alignment boundary.</param>
        /// <param name="chars">The characters to use for padding.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteAlign(this Stream stream, int boundary, ReadOnlySpan<char> chars)
            => stream.WriteAlign(boundary, chars, EncodingX.DefaultEncoding);
        #endregion
    }
}

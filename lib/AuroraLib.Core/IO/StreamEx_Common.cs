using AuroraLib.Common;
using AuroraLib.Core.Exceptions;
using AuroraLib.Core.Interfaces;
using AuroraLib.Core.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AuroraLib.Core.IO
{
    /// <summary>
    /// Extension of the <see cref="Stream"/>.
    /// </summary>
    public static partial class StreamEx
    {
        #region Read
        /// <summary>
        /// Reads a block of bytes from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] Read(this Stream stream, int Count)
        {
            if (stream.Position + Count > stream.Length)
                ThrowHelper<byte>(Count);

            byte[] Final = new byte[Count];
            stream.Read(Final);

            return Final;
        }

        /// <inheritdoc cref="Read(Stream, int)"/>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Read(this Stream stream, uint Count)
            => Read(stream, (int)Count);
        #endregion

        #region PeekByte
        /// <summary>
        /// Peek the next byte
        /// </summary>
        /// <param name="FS">this</param>
        /// <returns>The next byte to be read</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte PeekByte(this Stream FS)
        {
            byte val = (byte)FS.ReadByte();
            FS.Position--;
            return val;
        }
        #endregion

        /// <summary>
        /// Copies the entire contents of the stream to the specified destination stream.
        /// </summary>
        /// <param name="stream">The source stream to copy from.</param>
        /// <param name="destination">The destination stream to copy to.</param>
        /// <param name="bufferSize">The size of the buffer used for copying. Default is 81920 bytes.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyAllTo(this Stream stream, Stream destination, int bufferSize = 81920)
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(destination, bufferSize);
        }

        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the <see cref="Stream.Position"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte[] ToArray(this Stream stream, int bufferSize = 81920)
        {
            if (stream is MemoryStream ms)
                return ms.ToArray();

            using MemoryStream memoryStream = new((int)(stream.Length));
            stream.CopyAllTo(memoryStream, bufferSize);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Writes the stream contents to a byte array, from the current <see cref="Stream.Position"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToArrayHere(this Stream stream, int bufferSize = 81920)
        {
            using MemoryStream memoryStream = new((int)(stream.Length - stream.Position));
            stream.CopyTo(memoryStream, bufferSize);
            return memoryStream.ToArray();
        }

        #region Match

        /// <summary>
        /// Matches the specified <paramref name="bytes"/> with the data in the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to match against.</param>
        /// <param name="bytes">The bytes to match.</param>
        /// <returns>true if the specified bytes match the data in the stream; otherwise, false.</returns>
        [DebuggerStepThrough]
        public static bool Match(this Stream stream, Span<byte> bytes)
        {
            Span<byte> buffer = stackalloc byte[bytes.Length];
            int i = stream.Read(buffer);
            return i == bytes.Length && buffer.SequenceEqual(bytes);
        }

        /// <summary>
        /// Matches the identifier in the <paramref name="stream"/> with the specified <paramref name="identifier"/>.
        /// </summary>
        /// <param name="stream">The stream to match against.</param>
        /// <param name="identifier">The identifier to match.</param>
        /// <returns>true if the identifier matches the content of the stream; otherwise, false.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Match(this Stream stream, in IIdentifier identifier)
            => stream.Match(identifier.AsSpan());

        /// <summary>
        /// Matches the identifier in the <paramref name="stream"/> with the specified <paramref name="identifier"/> and throws an <see cref="InvalidIdentifierException"/> if the match fails.
        /// </summary>
        /// <param name="stream">The stream to match against.</param>
        /// <param name="identifier">The identifier to match.</param>
        /// <exception cref="InvalidIdentifierException">Thrown when the match fails.</exception>
        [DebuggerStepThrough]
        public static void MatchThrow(this Stream stream, in IIdentifier identifier)
        {
            Span<byte> magic = identifier.AsSpan();
            Span<byte> buffer = stackalloc byte[magic.Length];
            int i = stream.Read(buffer);
            if (i != magic.Length || !buffer.SequenceEqual(magic))
            {
                throw new InvalidIdentifierException(new Identifier(buffer.ToArray()), identifier);
            }
        }

        #endregion

        #region Search
        /// <summary>
        /// searches for a specific pattern in a stream and moves its position until the pattern is found.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pattern">the string to search for</param>
        /// <returns>"true" when the pattern is found</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Search(this Stream stream, string pattern) => stream.Search(pattern.GetBytes());

        /// <summary>
        /// Searches for the specified <paramref name="pattern"/> of <see cref="byte"/> in the <paramref name="stream"/>.
        /// Moves the current position until a pattern is found or the end is reached.
        /// </summary>
        /// <param name="stream">The stream to search.</param>
        /// <param name="pattern">The pattern of bytes to search for.</param>
        /// <returns>True if the pattern is found in the stream, otherwise false.</returns>
        [DebuggerStepThrough]
        public static bool Search(this Stream stream, ReadOnlySpan<byte> pattern)
        {
            int i = 0;
            int readbyte;
            while ((readbyte = stream.ReadByte()) > -1)
            {
                if (readbyte == pattern[i])
                {
                    i++;
                    if (i != pattern.Length)
                        continue;

                    stream.Seek(-pattern.Length, SeekOrigin.Current);
                    return true;
                }
                else
                {
                    i = 0;
                }
            }
            return false;
        }

        /// <summary>
        /// Searches for the specified <paramref name="patterns"/> of <see cref="Byte"/> in the <paramref name="stream"/>.
        /// Moves the current position until a pattern is found or the end is reached.
        /// </summary>
        /// <param name="stream">The stream to search.</param>
        /// <param name="patterns">The patterns of bytes to search for.</param>
        /// <param name="match">When this method returns true, contains the matched pattern; otherwise, null.</param>
        /// <returns>True if any of the patterns are found in the stream, otherwise false.</returns>
        public static bool Search(this Stream stream, IEnumerable<byte[]> patterns, out byte[] match)
        {
            int[] i = new int[patterns.Count()];
            for (int p = 0; p < patterns.Count(); p++)
                i[p] = 0;

            int readbyte;
            while ((readbyte = stream.ReadByte()) > -1)
            {
                for (int p = 0; p < patterns.Count(); p++)
                {
                    if (readbyte == patterns.ElementAt(p)[i[p]])
                    {
                        i[p]++;
                        if (i[p] != patterns.ElementAt(p).Length)
                            continue;

                        stream.Seek(-patterns.ElementAt(p).Length, SeekOrigin.Current);
                        match = patterns.ElementAt(p);
                        return true;
                    }
                    else
                        i[p] = 0;
                }
            }
            match = null;
            return false;
        }
        #endregion

        #region DetectByteOrder
        /// <summary>
        /// Attempts to detect the byte order in which the stream is written, based on the provided types.
        /// </summary>
        /// <typeparam name="T">The types to use for checking the byte order.</typeparam>
        /// <param name="stream">The stream to check.</param>
        /// <returns>The detected byte order.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Endian DetectByteOrder<T>(this Stream stream) where T : unmanaged
            => stream.At(stream.Position, s => s.DetectByteOrder(sizeof(T)) < 0 ? Endian.Little : Endian.Big);

        /// <summary>
        /// Attempts to detect the byte order in which the stream is written, based on the provided types.
        /// </summary>
        /// <param name="stream">The stream to check.</param>
        /// <param name="types">The types to use for checking the byte order</param>
        /// <returns>The detected byte order.</returns>
        public static Endian DetectByteOrder(this Stream stream, params Type[] types)
        {
            long orpos = stream.Position;
            int proBigOrder = 0;
            foreach (var type in types)
            {
                proBigOrder += stream.DetectByteOrder(Marshal.SizeOf(type));
            }
            stream.Seek(orpos, SeekOrigin.Begin);
            return proBigOrder < 0 ? Endian.Little : Endian.Big;
        }

        private static int DetectByteOrder(this Stream stream, int size)
        {
            Span<byte> buffer = stackalloc byte[size];
            stream.Read(buffer);

            int proBigOrder = 0;
            for (int i = 0; i < size; i++)
            {
                int j = i < size / 2 ? i : size - i - 1;
                proBigOrder += (buffer[j] == 0 ? (i < size / 2 ? 1 : -1) : 0);
            }
            return proBigOrder;
        }
        #endregion

        #region Align

        /// <summary>
        /// sets the position within the current stream to the nearest possible boundary.
        /// </summary>
        /// <param name="stream">the current stream</param>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <param name="boundary">The byte boundary to Seek to</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static long Align(this Stream stream, long offset, SeekOrigin origin, int boundary = 32)
        {
            if (boundary <= 1)
                throw new ArgumentException($"{nameof(boundary)}: Must be 2 or more");

            switch (origin)
            {
                case SeekOrigin.Current:
                    offset += stream.Position;
                    break;

                case SeekOrigin.End:
                    offset = stream.Length - offset;
                    break;
            }
            return stream.Seek(AlignPosition(offset, boundary), SeekOrigin.Begin);
        }

        /// <summary>
        /// sets the position within the current stream to the nearest possible boundary.
        /// </summary>
        /// <param name="stream">the current stream</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Align(this Stream stream, int boundary = 32)
            => stream.Seek(AlignPosition(stream.Position, boundary), SeekOrigin.Begin);

        #endregion

        #region WriteAlign

        /// <summary>
        /// Writes padding bytes to the stream until its position aligns with the specified boundary.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="boundary">The boundary to align the position of the stream with.</param>
        /// <param name="Padding">The byte value to use for padding (default is 0x00).</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteAlign(this Stream stream, int boundary = 32, byte Padding = 0x00)
        {
            Span<byte> bytes = stackalloc byte[(int)(boundary - (stream.Position % boundary))];
            bytes.Fill(Padding);
            stream.Write(bytes);
        }

        /// <summary>
        /// Writes <paramref name="Padding"/> to the <paramref name="stream"/> to align the position to the specified <paramref name="boundary"/>.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="boundary">The desired alignment boundary.</param>
        /// <param name="padding">The padding characters to write.</param>
        [DebuggerStepThrough]
        public static void WriteAlign(this Stream stream, int boundary, ReadOnlySpan<byte> padding)
        {
            int PadCount = (int)(boundary - (stream.Position % boundary));
            while (PadCount > 0)
            {
                int i = Math.Min(PadCount, padding.Length);
                stream.Write(padding[..i]);
                PadCount -= i;
            }
        }

        #endregion

        /// <summary>
        /// Calculates the aligned position based on the specified <paramref name="position"/> and <paramref name="boundary"/>.
        /// </summary>
        /// <param name="position">The position to align.</param>
        /// <param name="boundary">The alignment boundary (default is 32).</param>
        /// <returns>The aligned position.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AlignPosition(long position, int boundary = 32)
        {
            long remainder = position % boundary;
            if (remainder != 0)
                return position + boundary - remainder;
            return position;
        }
    }
}

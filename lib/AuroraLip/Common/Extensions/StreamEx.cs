using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AuroraLip.Common
{

    /// <summary>
    /// Extension of the <see cref="Stream"/>.
    /// </summary>
    public static partial class StreamEx
    {
        /// <summary>
        /// Reads a block of bytes from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <param name="order">Byte order, in which bytes are read.</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] Read(this Stream stream, int Count, Endian order = Endian.Little, int Offset = 0)
        {
            byte[] Final = new byte[Count];
            stream.Read(Final, Offset, Count);
            switch (order)
            {
                case Endian.Big:
                    Array.Reverse(Final);
                    break;
                case Endian.Middle:
                    throw new NotImplementedException();
            }
            return Final;
        }

        /// <summary>
        /// Writes a block of bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Array">The buffer containing data to write to the stream</param>
        /// <param name="Count">The maximum number of bytes to write</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        /// <exception cref="ArgumentException">offset and count describe an invalid range in array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
        /// <exception cref="IOException">An I/O error occurred, or Another thread may have caused an unexpected change in the position of the operating system's file handle</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed</exception>
        /// <exception cref="NotSupportedException">The current stream instance does not support writing</exception>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, byte[] Array, int Count, Endian order = Endian.Little, int Offset = 0)
        {
            switch (order)
            {
                case Endian.Big:
                    System.Array.Reverse(Array);
                    break;
                case Endian.Middle:
                    throw new NotImplementedException();
            }
            stream.Write(Array, Offset, Count);
        }

        [DebuggerStepThrough]
        public static void Write(this Stream stream, byte[] Array, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(Array, Array.Length,order,Offset);

        [DebuggerStepThrough]
        public static void WriteBigEndian(this Stream FS, byte[] Array, int Count, int Offset = 0)
            => FS.Write(Array, Count, Endian.Big, Offset);

        /// <summary>
        /// Adds Padding to the Current Position in the provided Stream
        /// </summary>
        /// <param name="FS">The Stream to add padding to</param>
        /// <param name="Multiple">The byte multiple to pad to</param>
        /// <param name="Padding">The byte multiple to pad to</param>
        [DebuggerStepThrough]
        public static void PadTo(this Stream FS, int Multiple, byte Padding = 0x00)
        {
            while (FS.Position % Multiple != 0)
                FS.WriteByte(Padding);
        }
        /// <summary>
        /// Peek the next byte
        /// </summary>
        /// <param name="FS">this</param>
        /// <returns>The next byte to be read</returns>
        [DebuggerStepThrough]
        public static byte PeekByte(this Stream FS)
        {
            byte val = (byte)FS.ReadByte();
            FS.Position--;
            return val;
        }
        /// <summary>
        /// Reads the stream and outputs it as array
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte[] ToArray(this Stream stream, int bufferSize = 81920)
        {
            if (stream is MemoryStream ms)
                return ms.ToArray();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream, bufferSize);
                return memoryStream.ToArray();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="magic">Match</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool MatchString(this Stream stream, in string magic)
        {
            if (magic.Length > stream.Length - stream.Position) return false;
            return stream.ReadString(magic.Length) == magic;
        }

        /// <summary>
        /// searches for a specific pattern in a stream and moves its position until the pattern is found.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pattern">the string to search for</param>
        /// <returns>"true" when the pattern is found</returns>
        [DebuggerStepThrough]
        public static bool Search(this Stream stream, string pattern) => stream.Search(pattern.ToByte());

        /// <summary>
        /// searches for a specific pattern in a stream and moves its position until the pattern is found.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pattern">the byte[] to search for</param>
        /// <returns>"true" when the pattern is found</returns>
        [DebuggerStepThrough]
        public static bool Search(this Stream stream, byte[] pattern)
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
                    i = 0;
            }
            return false;
        }

        /// <summary>
        /// Adds Padding to the Current Position in the provided Stream
        /// </summary>
        /// <param name="stream">The Stream to add padding to</param>
        /// <param name="multiple">The byte multiple to pad to</param>
        public static void AddPadding(this Stream stream, int multiple,in string Padding)
        {
            int PadCount = 0;
            while (stream.Position % multiple != 0)
            {
                stream.WriteByte((byte)Padding[PadCount++]);
                if (Padding.Length < PadCount) PadCount = 0;
            }
        }
    }
}
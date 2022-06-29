using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Drawing;
using Hack.io.Util;

namespace Hack.io
{
    /// <summary>
    /// Extension of the <see cref="FileStream"/> designed for Big Endian
    /// </summary>
    public static class FileStreamEx
    {
        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer (For Little Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] Read(this FileStream FS, int Offset, int Count)
        {
            byte[] Final = new byte[Count];
            FS.Read(Final, Offset, Count);
            return Final;
        }
        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer... but Backwards! (For Big Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] ReadReverse(this FileStream FS, int Offset, int Count)
        {
            byte[] Final = new byte[Count];
            FS.Read(Final, Offset, Count);
            Array.Reverse(Final);
            return Final;
        }
        /// <summary>
        /// Writes a block of bytes to the file stream... but Backwards! (For Big Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Array">The buffer containing data to write to the stream</param>
        /// <param name="Offset">The zero-based byte offset in array from which to begin copying bytes to the stream</param>
        /// <param name="Count">The maximum number of bytes to write</param>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        /// <exception cref="ArgumentException">offset and count describe an invalid range in array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
        /// <exception cref="IOException">An I/O error occurred, or Another thread may have caused an unexpected change in the position of the operating system's file handle</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed</exception>
        /// <exception cref="NotSupportedException">The current stream instance does not support writing</exception>
        [DebuggerStepThrough]
        public static void WriteReverse(this FileStream FS, byte[] Array, int Offset, int Count)
        {
            System.Array.Reverse(Array);
            FS.Write(Array, Offset, Count);
        }
        /// <summary>
        /// Reads a String from the file. Strings are terminated by 0x00. <para/> The decoded string is in SHIFT-JIS
        /// </summary>
        /// <param name="FS"></param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this FileStream FS)
        {
            List<byte> bytes = new List<byte>();
            int strCount = 0;
            while (FS.ReadByte() != 0)
            {
                FS.Position -= 1;
                bytes.Add((byte)FS.ReadByte());

                strCount++;
                if (strCount > FS.Length)
                    throw new IOException("An error has occurred while reading the string");
            }
            return Encoding.GetEncoding("Shift-JIS").GetString(bytes.ToArray(), 0, bytes.ToArray().Length);
        }
        /// <summary>
        /// Reads a String from the file. String length is determined by the "<paramref name="StringLength"/>" parameter. <para/> The decoded string is in SHIFT-JIS
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="StringLength">Length of the string to read. Cannot be longer than the <see cref="FileStream.Length"/></param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this FileStream FS, int StringLength)
        {
            byte[] bytes = new byte[StringLength];
            FS.Read(bytes, 0, StringLength);
            return Encoding.GetEncoding("Shift-JIS").GetString(bytes, 0, StringLength);
        }
        /// <summary>
        /// Reads a String from the file. Strings are terminated by 0x00. <para/> The decoded string is defined by the "<paramref name="Encoding"/>" Parameter.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this FileStream FS, Encoding Encoding)
        {
            List<byte> bytes = new List<byte>();
            int strCount = 0;
            while (FS.ReadByte() != 0)
            {
                FS.Position -= 1;
                bytes.Add((byte)FS.ReadByte());

                strCount++;
                if (strCount > FS.Length)
                    throw new IOException("An error has occurred while reading the string");
            }
            return Encoding.GetString(bytes.ToArray(), 0, bytes.ToArray().Length);
        }
        /// <summary>
        /// Reads a String from the file. String length is determined by the "<paramref name="StringLength"/>" parameter. <para/> The decoded string is defined by the "<paramref name="Encoding"/>" Parameter.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="StringLength">Length of the string to read. Cannot be longer than the <see cref="FileStream.Length"/></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this FileStream FS, int StringLength, Encoding Encoding)
        {
            byte[] bytes = new byte[StringLength];
            FS.Read(bytes, 0, StringLength);
            return Encoding.GetString(bytes, 0, StringLength);
        }
        /// <summary>
        /// Writes a string. String won't be null terminated
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        [DebuggerStepThrough]
        public static void WriteString(this FileStream FS, string String)
        {
            byte[] Write = Encoding.GetEncoding(932).GetBytes(String);
            FS.Write(Write, 0, Write.Length);
        }
        /// <summary>
        /// Writes a string. String will be null terminated with the <paramref name="Terminator"/>
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        /// <param name="Terminator">The Terminator of the string. Usually 0x00</param>
        [DebuggerStepThrough]
        public static void WriteString(this FileStream FS, string String, byte Terminator)
        {
            byte[] Write = Encoding.GetEncoding(932).GetBytes(String);
            FS.Write(Write, 0, Write.Length);
            FS.WriteByte(Terminator);
        }
        /// <summary>
        /// Reads a char from the file.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="CharLength">Expected Length of the Character</param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static char ReadChar(this FileStream FS, int CharLength, Encoding Encoding)
        {
            return Encoding.GetString(FS.Read(0, CharLength))[0];
        }
        /// <summary>
        /// Peek the next byte
        /// </summary>
        /// <param name="FS">this</param>
        /// <returns>The next byte to be read</returns>
        [DebuggerStepThrough]
        public static byte PeekByte(this FileStream FS)
        {
            byte val = (byte)FS.ReadByte();
            FS.Position--;
            return val;
        }
    }

    /// <summary>
    /// Extension of the <see cref="MemoryStream"/> designed for Big Endian
    /// </summary>
    public static class MemoryStreamEx
    {
        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer (For Little Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] Read(this MemoryStream FS, int Offset, uint Count)
        {
            byte[] Final = new byte[Count];
            for (uint i = 0; i < Count; i++)
                Final[i] = (byte)FS.ReadByte();
            return Final;
        }
        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer (For Little Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] Read(this MemoryStream FS, int Offset, int Count)
        {
            byte[] Final = new byte[Count];
            FS.Read(Final, Offset, Count);
            return Final;
        }
        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer... but Backwards! (For Big Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] ReadReverse(this MemoryStream FS, int Offset, int Count)
        {
            byte[] Final = new byte[Count];
            FS.Read(Final, Offset, Count);
            Array.Reverse(Final);
            return Final;
        }
        /// <summary>
        /// Writes a block of bytes to the file stream... but Backwards! (For Big Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Array">The buffer containing data to write to the stream</param>
        /// <param name="Offset">The zero-based byte offset in array from which to begin copying bytes to the stream</param>
        /// <param name="Count">The maximum number of bytes to write</param>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        /// <exception cref="ArgumentException">offset and count describe an invalid range in array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
        /// <exception cref="IOException">An I/O error occurred, or Another thread may have caused an unexpected change in the position of the operating system's file handle</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed</exception>
        /// <exception cref="NotSupportedException">The current stream instance does not support writing</exception>
        [DebuggerStepThrough]
        public static void WriteReverse(this MemoryStream FS, byte[] Array, int Offset, int Count)
        {
            System.Array.Reverse(Array);
            FS.Write(Array, Offset, Count);
        }
        /// <summary>
        /// Reads a String from the file. Strings are terminated by 0x00. <para/> The decoded string is in SHIFT-JIS
        /// </summary>
        /// <param name="FS"></param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this MemoryStream FS)
        {
            List<byte> bytes = new List<byte>();
            int strCount = 0;
            while (FS.ReadByte() != 0)
            {
                FS.Position -= 1;
                bytes.Add((byte)FS.ReadByte());

                strCount++;
                if (strCount > FS.Length)
                    throw new IOException("An error has occurred while reading the string");
            }
            return Encoding.GetEncoding("Shift-JIS").GetString(bytes.ToArray(), 0, bytes.ToArray().Length);
        }
        /// <summary>
        /// Reads a String from the file. String length is determined by the "<paramref name="StringLength"/>" parameter. <para/> The decoded string is in SHIFT-JIS
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="StringLength">Length of the string to read. Cannot be longer than the <see cref="FileStream.Length"/></param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this MemoryStream FS, int StringLength)
        {
            byte[] bytes = new byte[StringLength];
            FS.Read(bytes, 0, StringLength);
            return Encoding.GetEncoding("Shift-JIS").GetString(bytes, 0, StringLength);
        }
        /// <summary>
        /// Reads a String from the file. Strings are terminated by 0x00. <para/> The decoded string is defined by the "<paramref name="Encoding"/>" Parameter.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this MemoryStream FS, Encoding Encoding)
        {
            List<byte> bytes = new List<byte>();
            int strCount = 0;
            while (FS.ReadByte() != 0)
            {
                FS.Position -= 1;
                bytes.Add((byte)FS.ReadByte());

                strCount++;
                if (strCount > FS.Length)
                    throw new IOException("An error has occurred while reading the string");
            }
            return Encoding.GetString(bytes.ToArray(), 0, bytes.ToArray().Length);
        }
        /// <summary>
        /// Reads a String from the file. String length is determined by the "<paramref name="StringLength"/>" parameter. <para/> The decoded string is defined by the "<paramref name="Encoding"/>" Parameter.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="StringLength">Length of the string to read. Cannot be longer than the <see cref="FileStream.Length"/></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this MemoryStream FS, int StringLength, Encoding Encoding)
        {
            byte[] bytes = new byte[StringLength];
            FS.Read(bytes, 0, StringLength);
            return Encoding.GetString(bytes, 0, StringLength);
        }
        /// <summary>
        /// Writes a string. String won't be null terminated
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        [DebuggerStepThrough]
        public static void WriteString(this MemoryStream FS, string String)
        {
            byte[] Write = Encoding.GetEncoding(932).GetBytes(String);
            FS.Write(Write, 0, Write.Length);
        }
        /// <summary>
        /// Writes a string. String will be null terminated with the <paramref name="Terminator"/>
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        /// <param name="Terminator">The Terminator of the string. Usually 0x00</param>
        [DebuggerStepThrough]
        public static void WriteString(this MemoryStream FS, string String, byte Terminator)
        {
            byte[] Write = Encoding.GetEncoding(932).GetBytes(String);
            FS.Write(Write, 0, Write.Length);
            FS.WriteByte(Terminator);
        }
        /// <summary>
        /// Reads a char from the file.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="CharLength">Expected Length of the Character</param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static char ReadChar(this MemoryStream FS, int CharLength, Encoding Encoding)
        {
            return Encoding.GetString(FS.Read(0, CharLength))[0];
        }
        /// <summary>
        /// Peek the next byte
        /// </summary>
        /// <param name="FS">this</param>
        /// <returns>The next byte to be read</returns>
        [DebuggerStepThrough]
        public static byte PeekByte(this MemoryStream FS)
        {
            byte val = (byte)FS.ReadByte();
            FS.Position--;
            return val;
        }
    }

    /// <summary>
    /// Extension of the <see cref="Stream"/> designed for Big Endian
    /// </summary>
    public static class StreamEx
    {
        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer (For Little Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] Read(this Stream FS, int Offset, uint Count)
        {
            byte[] Final = new byte[Count];
            for (uint i = 0; i < Count; i++)
                Final[i] = (byte)FS.ReadByte();
            return Final;
        }
        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer (For Little Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] Read(this Stream FS, int Offset, int Count)
        {
            byte[] Final = new byte[Count];
            FS.Read(Final, Offset, Count);
            return Final;
        }
        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer... but Backwards! (For Big Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] ReadReverse(this Stream FS, int Offset, int Count)
        {
            byte[] Final = new byte[Count];
            FS.Read(Final, Offset, Count);
            Array.Reverse(Final);
            return Final;
        }
        /// <summary>
        /// Writes a block of bytes to the file stream... but Backwards! (For Big Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Array">The buffer containing data to write to the stream</param>
        /// <param name="Offset">The zero-based byte offset in array from which to begin copying bytes to the stream</param>
        /// <param name="Count">The maximum number of bytes to write</param>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        /// <exception cref="ArgumentException">offset and count describe an invalid range in array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
        /// <exception cref="IOException">An I/O error occurred, or Another thread may have caused an unexpected change in the position of the operating system's file handle</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed</exception>
        /// <exception cref="NotSupportedException">The current stream instance does not support writing</exception>
        [DebuggerStepThrough]
        public static void WriteReverse(this Stream FS, byte[] Array, int Offset, int Count)
        {
            System.Array.Reverse(Array);
            FS.Write(Array, Offset, Count);
        }
        /// <summary>
        /// Reads a String from the file. Strings are terminated by 0x00. <para/> The decoded string is in SHIFT-JIS
        /// </summary>
        /// <param name="FS"></param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream FS)
        {
            List<byte> bytes = new List<byte>();
            int strCount = 0;
            while (FS.ReadByte() != 0)
            {
                FS.Position -= 1;
                bytes.Add((byte)FS.ReadByte());

                strCount++;
                if (strCount > FS.Length)
                    throw new IOException("An error has occurred while reading the string");
            }
            return Encoding.GetEncoding("Shift-JIS").GetString(bytes.ToArray(), 0, bytes.ToArray().Length);
        }
        /// <summary>
        /// Reads a String from the file. String length is determined by the "<paramref name="StringLength"/>" parameter. <para/> The decoded string is in SHIFT-JIS
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="StringLength">Length of the string to read. Cannot be longer than the <see cref="FileStream.Length"/></param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream FS, int StringLength)
        {
            byte[] bytes = new byte[StringLength];
            FS.Read(bytes, 0, StringLength);
            return Encoding.GetEncoding("Shift-JIS").GetString(bytes, 0, StringLength);
        }
        /// <summary>
        /// Reads a String from the file. Strings are terminated by 0x00. <para/> The decoded string is defined by the "<paramref name="Encoding"/>" Parameter.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream FS, Encoding Encoding)
        {
            List<byte> bytes = new List<byte>();
            int ByteCount = Encoding.GetStride();
            byte[] Checker = new byte[ByteCount];
            bool IsDone = false;
            do
            {
                if (FS.Position > FS.Length)
                    throw new IOException("Stream ended before the String was terminated!");
                Checker = FS.Read(0, ByteCount);
                if (Checker.All(B => B == 0x00))
                {
                    IsDone = true;
                    break;
                }
                bytes.AddRange(Checker);
            } while (!IsDone);
            return Encoding.GetString(bytes.ToArray(), 0, bytes.Count);
        }
        /// <summary>
        /// Reads a String from the file. String length is determined by the "<paramref name="StringLength"/>" parameter. <para/> The decoded string is defined by the "<paramref name="Encoding"/>" Parameter.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="StringLength">Length of the string to read. Cannot be longer than the <see cref="FileStream.Length"/></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream FS, int StringLength, Encoding Encoding)
        {
            StringLength = StringLength * Encoding.GetStride();
            byte[] bytes = new byte[StringLength];
            FS.Read(bytes, 0, StringLength);
            return Encoding.GetString(bytes, 0, StringLength);
        }
        /// <summary>
        /// Writes a string. String won't be null terminated
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        [DebuggerStepThrough]
        public static void WriteString(this Stream FS, string String)
        {
            byte[] Write = Encoding.GetEncoding(932).GetBytes(String);
            FS.Write(Write, 0, Write.Length);
        }
        /// <summary>
        /// Writes a string. String will be null terminated with the <paramref name="Terminator"/>
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        /// <param name="Terminator">The Terminator of the string. Usually 0x00</param>
        [DebuggerStepThrough]
        public static void WriteString(this Stream FS, string String, byte Terminator)
        {
            byte[] Write = Encoding.GetEncoding(932).GetBytes(String);
            FS.Write(Write, 0, Write.Length);
            FS.WriteByte(Terminator);
        }
        /// <summary>
        /// Writes a string. String will be NULL terminated
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        /// <param name="Encoding"></param>
        [DebuggerStepThrough]
        public static void WriteString(this Stream FS, string String, Encoding Encoding)
        {
            byte[] Write = Encoding.GetBytes(String);
            FS.Write(Write, 0, Write.Length);
            int stride = Encoding.GetStride();
            FS.Write(new byte[stride], 0, stride);
        }
        /// <summary>
        /// writes an SByte
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="Data">The SByte</param>
        [DebuggerStepThrough]
        public static void WriteSByte(this Stream FS, sbyte Data)
        {
            FS.WriteByte((byte)(Data < 0 ? (255 - (Data + 1)) : Data));
        }
        /// <summary>
        /// Writes a Colour to a stream
        /// </summary>
        [DebuggerStepThrough]
        public static void WriteColour(this Stream FS, Color Col, string ColourOrder = "RGBA")
        {
            ColourOrder = ColourOrder.ToUpper();
            for (int i = 0; i < ColourOrder.Length; i++)
            {
                switch (ColourOrder[i])
                {
                    case 'R':
                        FS.WriteByte(Col.R);
                        break;
                    case 'G':
                        FS.WriteByte(Col.G);
                        break;
                    case 'B':
                        FS.WriteByte(Col.B);
                        break;
                    case 'A':
                        FS.WriteByte(Col.A);
                        break;
                    default:
                        break;
                }
            }
        }
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
        /// Reads a char from the file.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static char ReadChar(this Stream FS, Encoding Encoding) => Encoding.GetString(FS.Read(0, Encoding.GetStride()))[0];
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
    }
}
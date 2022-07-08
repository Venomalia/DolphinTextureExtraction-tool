using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AuroraLip.Common
{

    /// <summary>
    /// Extension of the <see cref="Stream"/> designed for Big Endian
    /// </summary>
    public static class StreamEx
    {
        /// <summary>
        /// Reads a block of bytes from the stream(For Little Endian)
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
        public static byte[] Read(this Stream FS,int Count, int Offset = 0)
        {
            byte[] Final = new byte[Count];
            FS.Read(Final, Offset, Count);
            return Final;
        }
        /// <summary>
        /// Reads a block of bytes from the stream... but Backwards! (For Big Endian)
        /// </summary>
        /// <param name="FS">This</param>
        /// <param name="Count">The maximum number of bytes to read.</param>
        /// <param name="Offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Offset or Count is negative.</exception>
        /// <exception cref="NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ArgumentException">Offset and Count describe an invalid range in array.</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
        [DebuggerStepThrough]
        public static byte[] ReadBigEndian(this Stream FS, int Count, int Offset = 0)
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
        public static void WriteBigEndian(this Stream FS, byte[] Array, int Count, int Offset = 0)
        {
            System.Array.Reverse(Array);
            FS.Write(Array, Offset, Count);
        }
        /// <summary>
        /// Reads a String from the Stream. String are terminated by "<paramref name="validbytes"/> == false".
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="validbytes">Determines if the byte is valid</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream stream, Predicate<byte> validbytes = null)
            => stream.ReadString(EncodingEX.DefaultEncoding, validbytes);
        /// <summary>
        /// Reads a String from the Stream. String are terminated by "<paramref name="validbytes"/> == false".
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        /// <param name="validbytes">Determines if the byte is valid</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream stream, Encoding Encoding, Predicate<byte> validbytes = null)
        {
            if (validbytes == null) validbytes = EncodingEX.GetValidbytesPredicate(Encoding);

            List<byte> bytes = new List<byte>();

            int readbyte;
            while ((readbyte = stream.ReadByte()) > -1)
            {
                if (!validbytes.Invoke((byte)readbyte)) break;
                bytes.Add((byte)readbyte);
            }
            return Encoding.GetString(bytes.ToArray());
        }
        /// <summary>
        /// Reads a String from the file. String length is determined by the "<paramref name="StringLength"/>" parameter.
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="StringLength">Length of the string to read. Cannot be longer than the <see cref="FileStream.Length"/></param>
        /// <returns>Complete String</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream FS, int StringLength)
        {
            byte[] bytes = new byte[StringLength];
            FS.Read(bytes, 0, StringLength);
            return bytes.ToValidString();
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
            int stride = Encoding.GetMaxByteCount(0);
            FS.Write(new byte[stride], 0, stride);
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
        /// Adds Padding to the Current Position in the provided Stream
        /// </summary>
        /// <param name="J3DFile">The Stream to add padding to</param>
        /// <param name="multiple">The byte multiple to pad to</param>
        public static void AddPadding(this Stream J3DFile, int multiple,in string Padding)
        {
            int PadCount = 0;
            while (J3DFile.Position % multiple != 0)
            {
                J3DFile.WriteByte((byte)Padding[PadCount++]);
                if (Padding.Length < PadCount) PadCount = 0;
            }
        }
    }
}
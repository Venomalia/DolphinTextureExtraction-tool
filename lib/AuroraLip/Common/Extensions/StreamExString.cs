using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AuroraLip.Common
{
    public static partial class StreamEx
    {
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

    }
}

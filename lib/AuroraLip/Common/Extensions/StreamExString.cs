using System.Diagnostics;
using System.Text;

namespace AuroraLib.Common
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

        [DebuggerStepThrough]
        public static void Write(this Stream FS, string String)
            => FS.Write(String, Encoding.GetEncoding(28591));

        /// <summary>
        /// Writes a string. String won't be null terminated
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        /// <param name="Encoding">Encoding to use when getting the string</param>
        [DebuggerStepThrough]
        public static void Write(this Stream FS, string String, Encoding Encoding)
        {
            byte[] Write = Encoding.GetBytes(String);
            FS.Write(Write, 0, Write.Length);
        }

        /// <summary>
        /// Writes a string with a fixed length. remaining byts are filled with the <paramref name="padding"/> byte
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        /// <param name="padding">The Terminator of the string. Usually 0x00</param>
        /// <param name="length"></param>
        [DebuggerStepThrough]
        public static void WriteString(this Stream FS, string String, byte padding, int length)
            => FS.WriteString(String, padding, length, Encoding.GetEncoding(28591));

        /// <summary>
        /// Writes a string with a fixed length. remaining byts are filled with the <paramref name="padding"/> byte
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        /// <param name="padding">The padding byte. Usually 0x00</param>
        /// <param name="length"></param>
        /// <param name="encoding">Encoding to use when getting the string</param>
        [DebuggerStepThrough]
        public static void WriteString(this Stream FS, string String, byte padding, int length, Encoding encoding)
        {
            byte[] Write = encoding.GetBytes(String);
            Array.Resize(ref Write, length);
            for (int i = String.Length; i < length; i++)
            {
                Write[i] = padding;
            }
            FS.Write(Write, 0, Write.Length);
        }

        /// <summary>
        /// Writes a string. String will be terminated with the <paramref name="Terminator"/>
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        /// <param name="Terminator">The Terminator of the string. Usually 0x00</param>
        [DebuggerStepThrough]
        public static void WriteString(this Stream FS, string String, byte Terminator = 0x0)
            => FS.WriteString(String, Terminator, Encoding.GetEncoding(28591));

        /// <summary>
        /// Writes a string. String will be terminated with the <paramref name="terminator"/>
        /// </summary>
        /// <param name="FS"></param>
        /// <param name="String">String to write to the file</param>
        /// <param name="terminator">The Terminator of the string. Usually 0x00</param>
        /// <param name="encoding">Encoding to use when getting the string</param>
        [DebuggerStepThrough]
        public static void WriteString(this Stream FS, string String, byte terminator, Encoding encoding)
        {
            FS.Write(String, encoding);
            FS.WriteByte(terminator);
        }
    }
}

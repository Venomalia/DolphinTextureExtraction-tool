using System.Diagnostics;
using System.Text;

namespace AuroraLib.Common
{
    public static partial class StreamEx
    {
        #region ReadString
        /// <summary>
        /// Reads a String from the Stream. String are terminated by "<paramref name="validbytes"/> == false".
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="validbytes">Determines if the byte is valid</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream stream)
            => EncodingEX.GetString(stream.ReadStringBytes(EncodingEX.InvalidByte));

        /// <summary>
        /// Reads a string from the specified <paramref name="stream"/> using the specified <paramref name="encoding"/>.
        /// The string is read until an invalid byte is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use when getting the string.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream stream, Encoding Encoding)
            => Encoding.GetString(ReadStringBytes(stream, EncodingEX.InvalidByte));

        /// <summary>
        /// Reads a string from the specified <paramref name="stream"/>.
        /// The string is read until the specified <paramref name="terminator"/> byte is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="terminator">The byte that indicates the end of the string.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream stream, byte terminator)
            => EncodingEX.GetString(ReadStringBytes(stream, s => s == terminator));

        /// <summary>
        /// Reads a string from the specified <paramref name="stream"/> using the specified <paramref name="encoding"/>.
        /// The string is read until the specified <paramref name="terminator"/> byte is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use when getting the string.</param>
        /// <param name="terminator">The byte that indicates the end of the string.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream stream, Encoding Encoding, byte terminator)
            => Encoding.GetString(ReadStringBytes(stream, s => s == terminator));

        /// <summary>
        /// Reads a string from the specified <paramref name="stream"/> and stops reading when the specified <paramref name="stopByte"/> is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="stopByte">The stop byte predicate that determines when to stop reading.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
        public static string ReadString(this Stream stream, Predicate<byte> stopByte)
            => EncodingEX.GetString(ReadStringBytes(stream, stopByte));

        /// <summary>
        /// Reads a string from the <paramref name="stream"/> using the specified <paramref name="encoding"/> and stops reading when the specified <paramref name="stopByte"/> is encountered.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="encoding">The encoding to use for converting the read bytes to a string.</param>
        /// <param name="stopByte">The stop byte predicate that determines when to stop reading.</param>
        /// <returns>The string read from the stream.</returns>
        [DebuggerStepThrough]
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
            return EncodingEX.GetString(bytes, Padding);
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
            return EncodingEX.GetString(bytes, encoding, Padding);
        }

        #endregion

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

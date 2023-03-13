using System.Diagnostics;

namespace AuroraLib.Common
{
    public static partial class StreamEx
    {
        #region Read

        /// <summary>
        /// Returns a 8-bit unsigned integer, read from one byte at the current position.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static byte ReadUInt8(this Stream stream)
            => (byte)stream.ReadByte();

        /// <summary>
        /// Returns a 8-bit signed integer, read from one byte at the current position.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static sbyte ReadInt8(this Stream stream)
            => (sbyte)stream.ReadByte();

        /// <summary>
        /// Returns a 16-bit unsigned integer converted from two bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 16-bit unsigned integer formed by two bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static ushort ReadUInt16(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToUInt16(stream.Read(2, order, Offset), 0);

        /// <summary>
        /// Returns a 16-bit signed integer converted from two bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 16-bit signed integer formed by two bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static short ReadInt16(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToInt16(stream.Read(2, order, Offset), 0);

        /// <summary>
        /// Returns a 24-bit unsigned integer converted from three bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 24-bit unsigned integer formed by three bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static UInt24 ReadUInt24(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverterEx.ToUInt24(stream.Read(3, order, Offset), 0);

        /// <summary>
        /// Returns a 24-bit signed integer converted from three bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 24-bit signed integer formed by three bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static Int24 ReadInt24(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverterEx.ToInt24(stream.Read(3, order, Offset), 0);

        /// <summary>
        /// Returns a 32-bit unsigned integer converted from four bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 32-bit unsigned integer formed by four bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static uint ReadUInt32(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToUInt32(stream.Read(4, order, Offset), 0);

        /// <summary>
        /// Returns a 32-bit signed integer converted from four bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 32-bit signed integer formed by four bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static int ReadInt32(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToInt32(stream.Read(4, order, Offset), 0);

        /// <summary>
        /// Returns a 64-bit unsigned integer converted from eight bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 64-bit unsigned integer formed by eight bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static ulong ReadUInt64(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToUInt64(stream.Read(8, order, Offset), 0);

        /// <summary>
        /// Returns a 64-bit signed integer converted from eight bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A 64-bit signed integer formed by eight bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static long ReadInt64(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToInt64(stream.Read(8, order, Offset), 0);

        /// <summary>
        /// Returns a single-precision floating point number converted from four bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A single-precision floating point number formed by from bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static float ReadSingle(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToSingle(stream.Read(4, order, Offset), 0);

        /// <summary>
        /// Returns a double-precision floating point number converted from eight bytes at a specified position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="order">Byte order</param>
        /// <param name="Offset">The byte offset.</param>
        /// <returns>A double-precision floating point number formed by eight bytes beginning at the Offset.</returns>
        [DebuggerStepThrough]
        public static double ReadDouble(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToDouble(stream.Read(8, order, Offset), 0);
        #endregion

        #region Write
        /// <summary>
        /// Writes the specified 16-bit unsigned integer value as an block of two bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, ushort value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverter.GetBytes(value), 2, order, Offset);

        /// <summary>
        /// Writes the specified 16-bit signed integer value as an block of two bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, short value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverter.GetBytes(value), 2, order, Offset);

        /// <summary>
        /// Writes the specified 24-bit unsigned integer value as an block of three bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, UInt24 value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverterEx.GetBytes(value), 3, order, Offset);

        /// <summary>
        /// Writes the specified 24-bit signed integer value as an block of three bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, Int24 value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverterEx.GetBytes(value), 3, order, Offset);

        /// <summary>
        /// Writes the specified 32-bit unsigned integer value as an block of four bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, uint value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverter.GetBytes(value), 4, order, Offset);

        /// <summary>
        /// Writes the specified 32-bit signed integer value as an block of four bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, int value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverter.GetBytes(value), 4, order, Offset);

        /// <summary>
        /// Writes the specified 64-bit unsigned integer value as an block of eight bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, ulong value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverter.GetBytes(value), 8, order, Offset);

        /// <summary>
        /// Writes the specified 64-bit signed integer value as an block of eight bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, long value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverter.GetBytes(value), 8, order, Offset);
        /// <summary>
        /// Writes the specified single-precision floating point number value as an block of four bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, float value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverter.GetBytes(value), 4, order, Offset);
        /// <summary>
        /// Writes the specified double-precision floating point number value as an block of eight bytes to the file stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value">The number to convert.</param>
        /// <param name="order">Byte order, in which bytes are write.</param>
        /// <param name="Offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        [DebuggerStepThrough]
        public static void Write(this Stream stream, double value, Endian order = Endian.Little, int Offset = 0)
            => stream.Write(BitConverter.GetBytes(value), 8, order, Offset);
        #endregion
    }
}

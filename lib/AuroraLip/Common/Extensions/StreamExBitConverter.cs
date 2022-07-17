using System;
using System.Diagnostics;
using System.IO;

namespace AuroraLip.Common
{
    public static partial class StreamEx
    {
        [DebuggerStepThrough]
        public static ushort ReadUInt16(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToUInt16(stream.Read(2, order, Offset), 0);

        [DebuggerStepThrough]
        public static short ReadInt16(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToInt16(stream.Read(2, order, Offset), 0);

        [DebuggerStepThrough]
        public static UInt24 ReadUInt24(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverterEx.ToUInt24(stream.Read(3, order, Offset), 0);

        [DebuggerStepThrough]
        public static Int24 ReadInt24(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverterEx.ToInt24(stream.Read(3, order, Offset), 0);

        [DebuggerStepThrough]
        public static uint ReadUInt32(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToUInt32(stream.Read(4, order, Offset), 0);

        [DebuggerStepThrough]
        public static int ReadInt32(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToInt32(stream.Read(4, order, Offset), 0);

        [DebuggerStepThrough]
        public static ulong ReadUInt64(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToUInt64(stream.Read(8, order, Offset), 0);

        [DebuggerStepThrough]
        public static long ReadInt64(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToInt64(stream.Read(8, order, Offset), 0);

        [DebuggerStepThrough]
        public static float ReadSingle(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToSingle(stream.Read(4, order, Offset), 0);

        [DebuggerStepThrough]
        public static double ReadDouble(this Stream stream, Endian order = Endian.Little, int Offset = 0)
            => BitConverter.ToDouble(stream.Read(8, order, Offset), 0);
    }
}

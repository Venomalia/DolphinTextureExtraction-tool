using AuroraLip.Common;
using System;
using System.IO;

namespace AuroraLip.Compression.Formats
{
    public class ZLib : ICompression
    {
        public bool CanWrite { get; } = false;

        public bool CanRead { get; } = false;

        public byte[] Compress(in byte[] Data)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(in byte[] Data)
        {
            throw new NotImplementedException();
        }

        private bool IsMatch(in byte[] Data)
        {
            byte num = (byte)(Data[0] & 15);
            float single = Data[0] << 8 | Data[1];

            return ((Data[0] & 15) != 8 || (Data[0] >> 4 & 15) <= 7) && single / 31f == (float)(single / 31) && num != 15;
        }

        public bool IsMatch(Stream stream, in string extension = "")
        {
            return IsMatch(stream.Read(2));
        }
    }
}

using System;

namespace AuroraLip.Compression.Formats
{
    public class ZLib : ICompression
    {
        public bool CanCompress { get; } = false;

        public bool CanDecompress { get; } = false;

        public byte[] Compress(in byte[] Data)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(in byte[] Data)
        {
            throw new NotImplementedException();
        }

        public bool IsMatch(in byte[] Data)
        {
            byte num = (byte)(Data[0] & 15);
            float single = Data[0] << 8 | Data[1];

            return ((Data[0] & 15) != 8 || (Data[0] >> 4 & 15) <= 7) && single / 31f == (float)(single / 31) && num != 15;
        }
    }
}

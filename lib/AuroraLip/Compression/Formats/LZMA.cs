using System;

namespace AuroraLip.Compression.Formats
{
    public class LZMA : ICompression
    {
        public bool CanCompress { get; } = false;

        public bool CanDecompress { get; } = false;

        public byte[] Compress(byte[] Data)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(byte[] Data)
        {
            throw new NotImplementedException();
        }

        public bool IsMatch(byte[] Data)
        {
            return Data.Length > 8 && Data[0] == 93;
        }
    }
}

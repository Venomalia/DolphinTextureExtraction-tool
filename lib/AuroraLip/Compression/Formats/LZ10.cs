using System;

namespace AuroraLip.Compression.Formats
{
    /// <summary>
    /// Nintendo LZ10 compression algorithm
    /// </summary>
    public class LZ10 : ICompression
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
            return Data.Length > 2 && Data[0] == 31 && Data[1] == 139;
        }
    }
}

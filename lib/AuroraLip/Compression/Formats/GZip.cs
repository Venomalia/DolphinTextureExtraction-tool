using System;

namespace AuroraLip.Compression.Formats
{
    /// <summary>
    /// gzip open-source compression algorithm.
    /// </summary>
    public class GZip : ICompression
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
            return Data.Length > 9 && Data[0] == 31 && Data[1] == 139 && Data[2] <= 8;
        }
    }
}

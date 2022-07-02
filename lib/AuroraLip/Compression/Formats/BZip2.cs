using AuroraLip.Common;
using System;

namespace AuroraLip.Compression.Formats
{

    /// <summary>
    /// bzip2 open-source compression algorithm, uses the Burrows–Wheeler algorithm.
    /// </summary>
    public class BZip2 : ICompression, IMagicNumber
    {

        public string Magic { get; } = "BZh";

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
            return Data.Length > 4 && Data[0] == 66 && Data[1] == 90 && Data[2] == 104 && Data[3] >= 49 && Data[3] <= 57;
        }
    }
}

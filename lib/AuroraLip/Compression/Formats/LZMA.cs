using System;

namespace AuroraLip.Compression.Formats
{

    /// <summary>
    /// Lempel–Ziv–Markov chain open-source compression algorithm, This algorithm uses a dictionary compression similar to the LZ77 algorithm
    /// </summary>
    public class LZMA : ICompression
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
            return Data.Length > 8 && Data[0] == 93;
        }
    }
}

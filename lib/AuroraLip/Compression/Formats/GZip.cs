using System;
using System.IO;

namespace AuroraLip.Compression.Formats
{

    /// <summary>
    /// gzip open-source compression algorithm.
    /// </summary>
    public class GZip : ICompression
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
        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 9 && stream.ReadByte() == 31 && stream.ReadByte() == 139 && stream.ReadByte() <= 8;
    }
}

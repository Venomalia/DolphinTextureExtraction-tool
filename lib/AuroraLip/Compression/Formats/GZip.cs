using System.IO.Compression;

namespace AuroraLib.Compression.Formats
{
    /// <summary>
    /// gzip open-source compression algorithm.
    /// </summary>
    public class GZip : ICompression, ICompressionLevel
    {
        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        public void Compress(in byte[] source, Stream destination)
        {
            using GZipStream gZipStream = new(destination, System.IO.Compression.CompressionLevel.Optimal);
            gZipStream.Write(source);

        }

        public byte[] Decompress(Stream source)
        {
            using GZipStream gZipStream = new(source, CompressionMode.Decompress);
            return gZipStream.ToArray();
        }

        public byte[] Compress(byte[] Data, CompressionLevel level)
        {
            System.IO.Compression.CompressionLevel gzlvel = default;
            switch (level)
            {
                case CompressionLevel.NoCompression:
                    gzlvel = System.IO.Compression.CompressionLevel.NoCompression;
                    break;

                case CompressionLevel.SmallestSize:
                case CompressionLevel.Optimal:
                    gzlvel = System.IO.Compression.CompressionLevel.Optimal;
                    break;

                case CompressionLevel.Fastest:
                    gzlvel = System.IO.Compression.CompressionLevel.Fastest;
                    break;
            }
            using (MemoryPoolStream ms = new())
            using (GZipStream gZipStream = new(ms, gzlvel))
            {
                gZipStream.Write(Data);
                return ms.ToArray();
            }
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 9 && stream.ReadByte() == 31 && stream.ReadByte() == 139;
    }
}

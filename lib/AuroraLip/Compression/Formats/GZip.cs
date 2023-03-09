using System.IO.Compression;

namespace AuroraLip.Compression.Formats
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
            using (GZipStream gZipStream = new GZipStream(new MemoryStream(source), System.IO.Compression.CompressionLevel.Optimal))
            {
                gZipStream.CopyTo(destination);
            }
        }

        public byte[] Decompress(Stream source)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (GZipStream gZipStream = new GZipStream(source, CompressionMode.Decompress))
            {
                gZipStream.CopyTo(memoryStream);
            }
            return memoryStream.ToArray();
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
            MemoryStream memoryStream = new MemoryStream();
            using (GZipStream gZipStream = new GZipStream(memoryStream, gzlvel))
            {
                (new MemoryStream(Data)).CopyTo(gZipStream);
            }
            return memoryStream.ToArray();
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 9 && stream.ReadByte() == 31 && stream.ReadByte() == 139;
    }
}

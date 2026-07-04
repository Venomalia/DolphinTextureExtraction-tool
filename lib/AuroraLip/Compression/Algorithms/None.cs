using AuroraLib.Compression.Interfaces;
using System.IO.Compression;

namespace AuroraLib.Compression.Algorithms
{
    public class None : ICompressionAlgorithm
    {
        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default) => true;

        public void Compress(ReadOnlySpan<byte> source, Stream destination, CompressionLevel level = CompressionLevel.Optimal)
        {
            Compress(source, destination, 0);
        }

        public static void Compress(ReadOnlySpan<byte> source, Stream destination, int level)
        {
            destination.Write(source);
        }

        public void Decompress(Stream source, Stream destination)
        {
            source.CopyTo(destination);
        }
    }
}

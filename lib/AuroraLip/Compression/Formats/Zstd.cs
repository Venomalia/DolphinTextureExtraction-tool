using AuroraLib.Compression.Interfaces;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;
using System.IO.Compression;
using ZstdSharp;

namespace AuroraLib.Compression.Formats
{
    public class Zstd : ICompressionAlgorithm, IHasIdentifier
    {
        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(0xFD2FB528);

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x10 && stream.Match(_identifier);

        public void Compress(ReadOnlySpan<byte> source, Stream destination, CompressionLevel level = CompressionLevel.Optimal)
        {
            int zslevel = level switch
            {
                CompressionLevel.NoCompression => 0,
                CompressionLevel.Fastest => 5,
                CompressionLevel.Optimal => 10,
                CompressionLevel.SmallestSize => 20,
                _ => throw new NotImplementedException(),
            };
            Compress(source, destination, zslevel);
        }

        public static void Compress(ReadOnlySpan<byte> source, Stream destination, int level)
        {
            using SpanBuffer<byte> destinationSpan = new(Compressor.GetCompressBound(source.Length));
            using Compressor compressor = new(level);
            int length = compressor.Wrap(source, destinationSpan);
            destination.Write(destinationSpan.Span[..length]);
        }

        public void Decompress(Stream source, Stream destination)
        {
            using SpanBuffer<byte> sourceSpan = new((int)(source.Length - source.Position));
            source.Read(sourceSpan);
            ulong DecompressedSize = Decompressor.GetDecompressedSize(sourceSpan);
            using SpanBuffer<byte> destinationSpan = new((int)DecompressedSize);
            using Decompressor decompressor = new();
            decompressor.Unwrap(sourceSpan, destinationSpan);
            destination.Write(destinationSpan);
        }

    }
}

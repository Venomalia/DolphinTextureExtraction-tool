using AuroraLib.Compression.Interfaces;
using AuroraLib.Core.Buffers;
using IronCompress;
using System.IO.Compression;

namespace AuroraLib.Compression
{
    public abstract class IronCompressBase : ICompressionAlgorithm
    {
        protected static readonly Iron Iron = new();

        protected abstract Codec Codec { get; }

        public abstract bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default);

        public void Decompress(Stream source, Stream destination)
        {
            using SpanBuffer<byte> buffer = new((int)(source.Length - source.Position));
            source.Read(buffer);
            Decompress(buffer, destination);
        }

        public void Decompress(ReadOnlySpan<byte> source, Stream destination)
        {
            using IronCompressResult result = Iron.Decompress(Codec, source);
            destination.Write(result);
        }
        public void Compress(ReadOnlySpan<byte> source, Stream destination, CompressionLevel level = CompressionLevel.Optimal)
        {
            using IronCompressResult result = Iron.Compress(Codec, source, null, level);
            destination.Write(result);
        }
    }
}

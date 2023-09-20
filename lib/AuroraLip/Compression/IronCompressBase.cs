using IronCompress;

namespace AuroraLib.Compression
{
    public abstract class IronCompressBase : ICompression, ICompressionLevel
    {
        public bool CanRead => true;
        public bool CanWrite => true;

        protected static readonly Iron Iron = new();
        protected abstract Codec Codec { get; }

        public void Compress(in byte[] source, Stream destination)
        {
            using (IronCompressResult result = Iron.Compress(Codec, source))
            {
                destination.Write(result.AsSpan());
            }
        }

        public byte[] Compress(byte[] source, CompressionLevel level)
        {
            using (IronCompressResult result = Iron.Compress(Codec, source, null, level.ToSystemIO()))
            {
                return result.AsSpan().ToArray();
            }
        }

        public byte[] Decompress(Stream source)
        {
            using (IronCompressResult result = Iron.Decompress(Codec, source.ToArray()))
            {
                return result.AsSpan().ToArray();
            }
        }

        public byte[] Decompress(ReadOnlySpan<byte> source, int? outputLength = null)
        {
            using (IronCompressResult result = Iron.Decompress(Codec, source, outputLength))
            {
                return result.AsSpan().ToArray();
            }
        }

        public abstract bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default);
    }
}

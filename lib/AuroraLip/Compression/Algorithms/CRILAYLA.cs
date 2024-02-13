using AuroraLib.Compression.Interfaces;
using AuroraLib.Core.Interfaces;
using System.IO.Compression;

namespace AuroraLib.Compression.Algorithms
{
    public class CRILAYLA : ICompressionAlgorithm, IHasIdentifier
    {
        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier64 _identifier = new("CRILAYLA");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x10 && stream.Match(_identifier);

        public void Decompress(Stream source, Stream destination)
            => LibCPK.CRILAYLA.Decompress(source, destination);

        public void Compress(ReadOnlySpan<byte> source, Stream destination, CompressionLevel level = CompressionLevel.Optimal)
            => LibCPK.CRILAYLA.Compress(source, destination);
    }
}

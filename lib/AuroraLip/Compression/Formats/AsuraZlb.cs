using AuroraLib.Common;
using AuroraLib.Common.Struct;

namespace AuroraLib.Compression.Formats
{
    public class AsuraZlb : ICompression, IHasIdentifier
    {
        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier64 _identifier = new("AsuraZlb");

        private static readonly ZLib ZLib = new();

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 4 && stream.Match(_identifier);

        public byte[] Decompress(Stream source)
            => source.At(0x14, s => ZLib.Decompress(s));

        public void Compress(in byte[] source, Stream destination)
        {
            long start = destination.Position;

            destination.Write(_identifier);
            destination.Write(1);
            destination.Write(0); // Placeholder
            destination.Write(source.Length);
            ZLib.Compress(source, destination, 5);
            destination.Seek(start + 0xC, SeekOrigin.Begin);
            destination.Write(destination.Length - start - 0x14);
        }

    }
}

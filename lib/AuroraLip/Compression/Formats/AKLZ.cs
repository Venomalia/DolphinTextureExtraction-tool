using AuroraLib.Common;
using AuroraLib.Core.Interfaces;
using System.Reflection.Emit;

namespace AuroraLib.Compression.Formats
{
    /// <summary>
    /// This LZSS header was used in Skies of Arcadia Legends
    /// </summary>
    public class AKLZ : ICompression, IHasIdentifier
    {
        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier _identifier = new("AKLZ~?Qd=ÌÌÍ");

        private static readonly LZSS LZSS = new(12, 4, 2);

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 4 && stream.Match(_identifier);

        public byte[] Decompress(Stream source)
        {
            source.Position += 0xC;
            uint decompressedSize = source.ReadUInt32(Endian.Big);

            using Stream stream = LZSS.Decompress(source, (int)decompressedSize);
            return stream.ToArray();
        }

        public void Compress(in byte[] source, Stream destination) => throw new NotImplementedException();
    }
}

using AuroraLib.Common;
using AuroraLib.Common.Struct;
using LibCPK;

namespace AuroraLib.Compression.Formats
{
    public class CRILAYLA : ICompression, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier64 _identifier = new("CRILAYLA");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x10 && stream.Match(_identifier);

        public void Compress(in byte[] source, Stream destination)
        {
            destination.Write(CPK.CompressCRILAYLA(source));
        }

        public byte[] Decompress(Stream source)
            => CPK.DecompressLegacyCRI(source.ToArray());
    }
}

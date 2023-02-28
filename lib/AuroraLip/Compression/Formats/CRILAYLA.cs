using AuroraLip.Common;
using LibCPK;
using System.IO;

namespace AuroraLip.Compression.Formats
{
    public class CRILAYLA : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        public const string magic = "CRILAYLA";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        public void Compress(in byte[] source, Stream destination)
        {
            destination.Write(CPK.CompressCRILAYLA(source));
        }

        public byte[] Decompress(Stream source)
            => CPK.DecompressLegacyCRI(source.ToArray());
    }
}

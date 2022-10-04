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

        public static string magic { get; } = "CRILAYLA";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        public byte[] Compress(in byte[] Data)
        {
            return CPK.CompressCRILAYLA(Data);
        }

        public byte[] Decompress(in byte[] Data)
        {
            return CPK.DecompressLegacyCRI(Data);
        }
    }
}

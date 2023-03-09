using AuroraLip.Common;
using AuroraLip.Compression;
using AuroraLip.Compression.Formats;

namespace MuramasaTDB_Encoding
{
    /// <summary>
    /// FCMP use LZ01 algorithm.
    /// </summary>
    public class FCMP : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        public const string magic = "FCMP";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        public void Compress(in byte[] source, Stream destination)
        {
            // Write out the header
            destination.Write(magic.ToByte());
            destination.Write(source.Length); // Decompressed length
            destination.Write(305397760);

            LZ01.Compress_ALG(source, destination);
        }

        public byte[] Decompress(Stream source)
        {
            source.Position += 4;
            int destinationLength = source.ReadInt32();
            int unk = source.ReadInt32(); //always 305397760?
            return LZ01.Decompress_ALG(source, destinationLength);
        }
    }
}

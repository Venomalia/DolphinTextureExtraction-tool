using AuroraLib.Common;
using AuroraLib.Compression;
using AuroraLib.Compression.Formats;

namespace MuramasaTDB_Encoding
{
    /// <summary>
    /// FCMP use LZ01 algorithm.
    /// </summary>
    public class FCMP : ICompression, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("FCMP");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x10 && stream.Match(_identifier);

        public void Compress(in byte[] source, Stream destination)
        {
            // Write out the header
            destination.Write(_identifier);
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

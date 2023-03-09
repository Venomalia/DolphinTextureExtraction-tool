using AuroraLip.Common;

namespace AuroraLip.Compression.Formats
{
    public class GCLZ : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        public static string magic = "GCLZ";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic) && stream.ReadByte() == 16;

        public void Compress(in byte[] source, Stream destination)
        {
            // GCLZ compression can only handle files smaller than 16MB
            if (source.Length > 0xFFFFFF)
            {
                throw new Exception($"{typeof(GCLZ)} compression can't be used to compress files larger than {0xFFFFFF:N0} bytes.");
            }
            // Write out the header
            destination.Write(magic.ToByte());
            destination.Write(0x10 | (source.Length << 8));

            LZ10.Compress_ALG(source, destination);
        }

        public byte[] Decompress(Stream source)
        {
            source.Position += 5;
            int destinationLength = (int)source.ReadUInt24();

            return LZ10.Decompress_ALG(source, destinationLength);
        }
    }
}

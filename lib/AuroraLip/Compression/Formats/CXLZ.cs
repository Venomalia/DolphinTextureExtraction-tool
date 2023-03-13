using AuroraLib.Common;

namespace AuroraLib.Compression.Formats
{
    public class CXLZ : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        public static string magic = "CXLZ";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic) && stream.ReadByte() == 16;

        public void Compress(in byte[] source, Stream destination)
        {
            // CXLZ compression can only handle files smaller than 16MB
            if (source.Length > 0xFFFFFF)
            {
                throw new Exception($"{typeof(CXLZ)} compression can't be used to compress files larger than {0xFFFFFF:N0} bytes.");
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

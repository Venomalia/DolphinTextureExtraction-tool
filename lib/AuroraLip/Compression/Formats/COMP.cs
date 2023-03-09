using AuroraLip.Common;

namespace AuroraLip.Compression.Formats
{
    /// <summary>
    /// COMP use LZ11 algorithm.
    /// </summary>
    public class COMP : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        public const string magic = "COMP";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic) && stream.ReadByte() == 17;

        public void Compress(in byte[] source, Stream destination)
        {
            var destinationStartPosition = destination.Position;
            // Write out the header
            destination.Write(magic.ToByte());
            if (source.Length <= 0xFFFFFF)
            {
                destination.Write(0x11 | (source.Length << 8));
            }
            else
            {
                destination.WriteByte(0x11);
                destination.Write(source.Length);
            }

            LZ11.Compress_ALG(source, destination);
        }

        public byte[] Decompress(Stream source)
        {
            source.Position += 5;
            int destinationLength = (int)source.ReadUInt24();

            return LZ11.Decompress_ALG(source, destinationLength);
        }
    }
}

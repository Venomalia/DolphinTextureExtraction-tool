using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Compression.Formats
{
    /// <summary>
    /// COMP use LZ11 algorithm.
    /// </summary>
    public class COMP : ICompression, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("COMP");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x10 && stream.Match(_identifier) && stream.ReadByte() == 17;

        public void Compress(in byte[] source, Stream destination)
        {
            var destinationStartPosition = destination.Position;
            // Write out the header
            destination.Write(_identifier);
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

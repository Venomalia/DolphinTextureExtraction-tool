using AuroraLib.Common;

namespace AuroraLib.Compression.Formats
{
    /// <summary>
    /// The PRS compression algorithm is based on LZ77 with run-length encoding emulation and extended matches.
    /// </summary>
    public partial class PRS_BE : ICompression
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public static Endian Order => Endian.Big;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 6 && (stream.ReadByte() & 128) == 128 && stream.At(-2, SeekOrigin.End, S => S.ReadUInt16()) == 0;

        public byte[] Decompress(Stream source)
            => PRS.Decompress_ALG(source, Order);

        public void Compress(in byte[] source, Stream destination)
            => PRS.Compress_ALG(source, destination, Order);
    }
}

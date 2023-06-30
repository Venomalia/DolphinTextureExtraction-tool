using AuroraLib.Common;

namespace AuroraLib.Compression.Formats
{
    /// <summary>
    /// This LZSS header was used by Sega in early GC games like F-zero GX or Super Monkey Ball.
    /// </summary>
    public class LZSS_Sega : ICompression
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public const string Extension = ".lz";

        private static readonly LZSS LZSS = new(12, 4, 2);

        public bool IsMatch(Stream stream, in string extension = "")
        {
            if (stream.Length < 0x10)
                return false;

            uint compressedSize = stream.ReadUInt32();
            uint decompressedSize = stream.ReadUInt32();
            return (compressedSize == stream.Length - 8 || compressedSize == stream.Length) && decompressedSize != compressedSize && decompressedSize >= 0x20;
        }

        public void Compress(in byte[] source, Stream destination)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(Stream source)
        {
            uint compressedSize = source.ReadUInt32();
            uint decompressedSize = source.ReadUInt32();

            return LZSS.Decompress(source, (int)decompressedSize).ToArray();
        }
    }
}

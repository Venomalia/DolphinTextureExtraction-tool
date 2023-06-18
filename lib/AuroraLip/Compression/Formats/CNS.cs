using AuroraLib.Common;
using AuroraLib.Compression;

namespace AuroraLip.Compression.Formats
{
    /// <summary>
    /// CNS compression algorithm, used in Games from Red Entertainment.
    /// </summary>
    public class CNS : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        public const string magic = "@CNS";

        public void Compress(in byte[] source, Stream destination) => throw new NotImplementedException();

        public byte[] Decompress(Stream source)
        {
            // CNS files start with the "@CNS" followed by the file extension.
            source.Position += 8;
            uint decompressedSize = source.ReadUInt32(Endian.Little);
            source.Position += 4; // 0

            return Decompress_ALG(source, (int)decompressedSize);
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);


        public static byte[] Decompress_ALG(Stream source, int decomLength)
        {
            byte data1, data2;

            byte[] destination = new byte[decomLength];
            int destinationPointer = 0;

            while (destinationPointer < decomLength && source.Position != source.Length)
            {
                data1 = source.ReadUInt8();

                if ((data1 & 0x80) == 0) // Uncompressed value
                {
                    for (int i = 0; i < data1; i++)
                    {
                        destination[destinationPointer++] = source.ReadUInt8();
                    }
                }
                else // Compressed value
                {
                    data2 = source.ReadUInt8();

                    int matchDistance = data2 + 1;
                    int matchLength = (data1 & 0x7F) + 3;

                    for (int i = 0; i < matchLength; i++)
                    {
                        destination[destinationPointer] = destination[destinationPointer - matchDistance];
                        destinationPointer++;
                    }
                }
            }
            return destination;
        }
    }
}

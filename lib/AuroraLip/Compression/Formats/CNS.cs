using AuroraLib.Common;

namespace AuroraLib.Compression.Formats
{
    /// <summary>
    /// CNS compression algorithm, used in Games from Red Entertainment.
    /// </summary>
    public class CNS : ICompression, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("@CNS");

        public byte[] Decompress(Stream source)
        {
            // CNS files start with the "@CNS" followed by the file extension.
            source.Position += 8;
            uint decompressedSize = source.ReadUInt32(Endian.Little);
            source.Position += 4; // 0

            return Decompress_ALG(source, (int)decompressedSize);
        }

        public void Compress(in byte[] source, Stream destination)
        {
            destination.Write(_identifier);
            if (source[0] == 0x0 && source[1] == 0x20 && source[2] == 0xAF && source[3] == 0x30)
            {
                destination.WriteString("TPL");
            }
            else
            {
                destination.WriteString("PAK");
            }
            destination.Write(source.Length, Endian.Little);
            destination.Write(0);

            Compress_ALG(source, destination);
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);


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


        public static void Compress_ALG(byte[] sourceArray, Stream destination)
        {
            int sourceLength = sourceArray.Length,
                sourcePointer = 0x0;

            byte data1, data2;

            // Initalize the LZ dictionary
            LzWindowDictionary dictionary = new();
            dictionary.SetWindowSize(255);
            dictionary.SetMaxMatchAmount(130);

            using (MemoryStream buffer = new(255))
            {
                // Start compression
                while (sourcePointer < sourceLength)
                {
                    // Search for a match
                    int[] match = dictionary.Search(sourceArray, (uint)sourcePointer, (uint)sourceLength);


                    if ((buffer.Length != 0 && match[1] > 0) || buffer.Length == 127)
                    {
                        destination.WriteByte((byte)buffer.Length);
                        buffer.WriteTo(destination);
                        buffer.SetLength(0);
                    }

                    if (match[1] > 0) // There is a match
                    {

                        data1 = (byte)(0x80 | (match[1] - 3));
                        data2 = (byte)(match[0] - 1);
                        destination.WriteByte(data1);
                        destination.WriteByte(data2);

                        dictionary.AddEntryRange(sourceArray, sourcePointer, match[1]);

                        sourcePointer += match[1];
                    }
                    else // There is not a match
                    {
                        buffer.WriteByte(sourceArray[sourcePointer]);

                        dictionary.AddEntry(sourceArray, sourcePointer);

                        sourcePointer++;
                    }

                    // Check to see if we reached the end of the file
                    if (sourcePointer >= sourceLength)
                        break;
                }

                if (buffer.Length != 0)
                {
                    destination.WriteByte((byte)buffer.Length);
                    buffer.WriteTo(destination);
                    buffer.SetLength(0);
                }
            }
        }
    }
}

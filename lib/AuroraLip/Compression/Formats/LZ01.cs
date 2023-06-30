using AuroraLib.Common;

namespace AuroraLib.Compression.Formats
{
    /*
     * base on Puyo Tools
     * https://github.com/nickworonekin/puyotools/blob/master/src/PuyoTools.Core/Compression/Formats
     */

    public class LZ01 : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        public const string magic = "LZ01";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x10 && stream.MatchString(magic);

        public void Compress(in byte[] source, Stream destination)
        {
            var destinationStartPosition = destination.Position;
            // Write out the header
            destination.Write(magic.GetBytes());
            destination.Write(0); // Compressed length (will be filled in later)
            destination.Write(source.Length); // Decompressed length
            destination.Write(0);

            Compress_ALG(source, destination);

            // Go back to the beginning of the file and write out the compressed length
            var destinationLength = (int)(destination.Position - destinationStartPosition);
            destination.At(destinationStartPosition + 4, x => x.Write(destinationLength));
        }

        public byte[] Decompress(Stream source)
        {
            source.Position += 4;

            // Get the source length and the destination length
            int sourceLength = source.ReadInt32();
            int destinationLength = source.ReadInt32();

            source.Position += 4;

            return Decompress_ALG(source, destinationLength);
        }

        public static byte[] Decompress_ALG(Stream source, int decomLength)
        {
            int bufferPointer = 0xFEE, destinationPointer = 0x0;
            byte[] destination = new byte[decomLength];
            byte[] buffer = new byte[0x1000];

            // Start decompression
            while (true)
            {
                byte flag = source.ReadUInt8();

                for (int i = 0; i < 8; i++)
                {
                    if ((flag & 0x1) != 0) // Not compressed block
                    {
                        byte value = source.ReadUInt8();

                        destination[destinationPointer++] = value;

                        buffer[bufferPointer] = value;
                        bufferPointer = (bufferPointer + 1) & 0xFFF;
                    }
                    else // Compressed block
                    {
                        byte b1 = source.ReadUInt8(), b2 = source.ReadUInt8();

                        int matchOffset = (((b2 >> 4) & 0xF) << 8) | b1;
                        int matchLength = (b2 & 0xF) + 3;

                        for (int j = 0; j < matchLength; j++)
                        {
                            destination[destinationPointer++] = buffer[(matchOffset + j) & 0xFFF];

                            buffer[bufferPointer] = buffer[(matchOffset + j) & 0xFFF];
                            bufferPointer = (bufferPointer + 1) & 0xFFF;
                        }
                    }

                    // Check to see if we reached the end
                    if (destinationPointer == decomLength || source.Position == source.Length)
                    {
                        return destination;
                    }

                    flag >>= 1;
                }
            }
        }

        public static void Compress_ALG(in byte[] sourceArray, Stream destination)
        {
            int sourceLength = sourceArray.Length,
                sourcePointer = 0x0;

            // Initalize the LZ dictionary
            LzBufferDictionary dictionary = new LzBufferDictionary();
            dictionary.SetBufferSize(0x1000);
            dictionary.SetBufferStart(0xFEE);
            dictionary.SetMaxMatchAmount(0xF + 3);

            using (var buffer = new MemoryStream(16)) // Will never contain more than 16 bytes
            {
                // Start compression
                while (sourcePointer < sourceLength)
                {
                    byte flag = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        // Search for a match
                        int[] match = dictionary.Search(sourceArray, (uint)sourcePointer, (uint)sourceLength);

                        if (match[1] > 0) // There is a match
                        {
                            buffer.Write((ushort)((match[0] & 0xFF) | (match[0] & 0xF00) << 4 | ((match[1] - 3) & 0xF) << 8));

                            dictionary.AddEntryRange(sourceArray, sourcePointer, match[1]);

                            sourcePointer += match[1];
                        }
                        else // There is not a match
                        {
                            flag |= (byte)(1 << i);

                            buffer.WriteByte(sourceArray[sourcePointer]);

                            dictionary.AddEntry(sourceArray, sourcePointer);

                            sourcePointer++;
                        }

                        // Check to see if we reached the end of the file
                        if (sourcePointer >= sourceLength)
                            break;
                    }

                    // Flush the buffer and write it to the destination stream
                    destination.WriteByte(flag);

                    buffer.WriteTo(destination);
                    buffer.SetLength(0);
                }
            }
        }
    }
}

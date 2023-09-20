using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Compression.Formats
{
    /*
     * Puyo Tools
     * https://github.com/nickworonekin/puyotools/blob/master/src/PuyoTools.Core/Compression
     * CNX decompression support by drx (Luke Zapart)
     * <thedrx@gmail.com>
     */

    public class CNX : ICompression, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new((byte)'C', (byte)'N', (byte)'X', 0x2);

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x10 && stream.Match(_identifier);

        public void Compress(in byte[] source, Stream destination)
        {
            var destinationStartPosition = destination.Position;
            // Write out the header
            destination.Write(_identifier);

            // Get the file extension, and adjust as necessary to get it as 3 bytes

            //var fileExtension = source is FileStream fs ? Path.GetExtension(fs.Name) : string.Empty;
            string fileExtension = "DEC";
            destination.WriteString(fileExtension);

            destination.WriteByte(0x10);
            destination.Write(0, Endian.Big); // Compressed length (will be filled in later)
            destination.Write(source.Length, Endian.Big); // Decompressed length

            Compress_ALG(source, destination);

            // Go back to the beginning of the file and write out the compressed length
            var destinationLength = (int)(destination.Position - destinationStartPosition);
            destination.At(destinationStartPosition + 8, x => x.Write(destinationLength - 16, Endian.Big));
        }

        public byte[] Decompress(Stream source)
        {
            // Magic code & file extension
            source.Position += 8;

            // Get the source length and destination length
            int sourceLength = source.ReadInt32(Endian.Big) + 16;
            int destinationLength = source.ReadInt32(Endian.Big);

            return Decompress_ALG(source, destinationLength);
        }

        public static byte[] Decompress_ALG(Stream source, int decomLength)
        {
            byte[] destination = new byte[decomLength];

            int destinationPointer = 0x0;
            // Start decompression
            while (true)
            {
                byte flag = source.ReadUInt8();

                // If all bits are 0, this is the end of the compressed data.
                if (flag == 0)
                {
                    return destination;
                }

                for (int i = 0; i < 4; i++)
                {
                    byte value;
                    ushort matchPair;
                    int matchDistance, matchLength;

                    switch (flag & 0x3)
                    {
                        // Jump to the next 0x800 boundary
                        case 0:
                            value = source.ReadUInt8();
                            source.Position += value;

                            i = 3;
                            break;

                        // Not compressed, single byte
                        case 1:
                            value = source.ReadUInt8();
                            destination[destinationPointer++] = value;
                            break;

                        // Compressed
                        case 2:
                            matchPair = source.ReadUInt16(Endian.Big);

                            matchDistance = (matchPair >> 5) + 1;
                            matchLength = (matchPair & 0x1F) + 4;

                            for (int j = 0; j < matchLength; j++)
                            {
                                destination[destinationPointer] = destination[destinationPointer - matchDistance];
                                destinationPointer++;
                            }
                            break;

                        // Not compressed, multiple bytes
                        case 3:
                            matchLength = source.ReadUInt8();

                            for (int j = 0; j < matchLength; j++)
                            {
                                value = source.ReadUInt8();
                                destination[destinationPointer++] = value;
                            }
                            break;
                    }

                    // Check to see if we reached the end
                    if (destinationPointer >= decomLength)
                    {
                        return destination;
                    }

                    flag >>= 2;
                }
            }
        }

        public static void Compress_ALG(byte[] sourceArray, Stream destination)
        {
            int sourceLength = sourceArray.Length,
                sourcePointer = 0x0;

            // Initalize the LZ dictionary
            LzWindowDictionary dictionary = new();
            dictionary.SetWindowSize(0x800);
            dictionary.SetMinMatchAmount(4);
            dictionary.SetMaxMatchAmount(0x1F + 4);

            using MemoryStream buffer = new(256); // Will never contain more than 256 bytes
                                                  // Set the initial match
            int[] match = new[] { 0, 0 };

            // Start compression
            while (sourcePointer < sourceLength)
            {
                byte flag = 0;

                for (int i = 0; i < 4; i++)
                {
                    if (match[1] > 0) // There is a match
                    {
                        flag |= (byte)(2 << (i * 2));

                        buffer.Write((ushort)((((match[0] - 1) & 0x7FF) << 5) | ((match[1] - 4) & 0x1F)), Endian.Big);

                        dictionary.AddEntryRange(sourceArray, sourcePointer, match[1]);

                        sourcePointer += match[1];

                        // Search for a match
                        if (sourcePointer < sourceLength)
                        {
                            match = dictionary.Search(sourceArray, (uint)sourcePointer, (uint)sourceLength);
                        }
                    }
                    else // There is not a match
                    {
                        dictionary.AddEntry(sourceArray, sourcePointer);

                        byte matchLength = 1;

                        // Search for a match
                        while (sourcePointer + matchLength < sourceLength
                            && matchLength < 255
                            && (match = dictionary.Search(sourceArray, (uint)(sourcePointer + matchLength), (uint)sourceLength))[1] == 0)
                        {
                            dictionary.AddEntry(sourceArray, sourcePointer + matchLength);

                            matchLength++;
                        }

                        // Determine the type of flag to write based on the length of the match
                        if (matchLength > 1)
                        {
                            flag |= (byte)(3 << (i * 2));
                            buffer.WriteByte(matchLength);
                        }
                        else
                        {
                            flag |= (byte)(1 << (i * 2));
                        }

                        buffer.Write(sourceArray, sourcePointer, matchLength);

                        sourcePointer += matchLength;
                    }

                    // Check to see if we reached the end of the file
                    if (sourcePointer >= sourceLength)
                    {
                        break;
                    }
                }

                // Check to see if we reached the end of the file
                if (sourcePointer >= sourceLength && ((flag >> 6) & 0x3) == 0)
                {
                    // Write out values for this flag
                    buffer.WriteByte(0);
                }

                // Flush the buffer and write it to the destination stream
                destination.WriteByte(flag);

                buffer.WriteTo(destination);
                buffer.SetLength(0);
            }

            // Write the final flag of 0
            destination.WriteByte(0);
        }
    }
}

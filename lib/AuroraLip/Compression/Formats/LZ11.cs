using AuroraLip.Common;
using System.IO;

namespace AuroraLip.Compression.Formats
{

    /// <summary>
    /// Nintendo LZ11 compression algorithm
    /// </summary>
    public class LZ11 : ICompression
    {

        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 4 && stream.ReadByte() == 17;

        public void Compress(in byte[] source, Stream destination)
        {
            // Write out the header
            if (source.Length <= 0xFFFFFF)
            {
                destination.Write(0x11 | (source.Length << 8));
            }
            else
            {
                destination.WriteByte(0x11);
                destination.Write(source.Length);
            }

            Compress_ALG(source, destination);
        }

        public byte[] Decompress(Stream source)
        {
            source.Position += 1;
            int destinationLength = (int)source.ReadUInt24();

            return Decompress_ALG(source, destinationLength);
        }

        public static byte[] Decompress_ALG(Stream source, int decomLength)
        {
            int matchDistance, matchLength;
            byte[] destination = new byte[decomLength];
            int destinationPointer = 0;

            while (true)
            {
                byte flag = source.ReadUInt8();
                for (int i = 0; i < 8; i++)
                {
                    if ((flag & 128) != 0) // Data is compressed
                    {
                        byte data1 = source.ReadUInt8();
                        if (data1 >> 4 == 0) // 1+2 bytes
                        {
                            byte data2 = source.ReadUInt8();
                            byte data3 = source.ReadUInt8();
                            matchLength = ((data1 & 15) << 4 | data2 >> 4) + 17;
                            matchDistance = ((data2 & 15) << 8 | data3) + 1;
                        }
                        else if (data1 >> 4 != 1) // 1+1 bytes
                        {
                            byte data5 = source.ReadUInt8();
                            matchDistance = ((data1 & 15) << 8 | data5) + 1;
                            matchLength = (data1 >> 4) + 1;
                        }
                        else // 1+3 bytes
                        {
                            byte data6 = source.ReadUInt8();
                            byte data7 = source.ReadUInt8();
                            byte data8 = source.ReadUInt8();
                            matchLength = ((data1 & 15) << 12 | data6 << 4 | data7 >> 4) + 273;
                            matchDistance = ((data7 & 15) << 8 | data8) + 1;
                        }
                        for (int j = 0; j < matchLength; j++)
                        {
                            destination[destinationPointer] = destination[destinationPointer - matchDistance];
                            destinationPointer++;
                        }
                    }
                    else // Not compressed
                    {
                        destination[destinationPointer++] = source.ReadUInt8(); ;
                    }

                    // Check to see if we reached the end
                    if (destinationPointer >= decomLength)
                    {
                        return destination;
                    }
                    flag <<= 1;
                }
            }
        }

        /*
         * base on Puyo Tools
         * https://github.com/nickworonekin/puyotools/blob/master/src/PuyoTools.Core/Compression/Formats
         */
        public static void Compress_ALG(in byte[] sourceArray, Stream destination)
        {
            int sourceLength = sourceArray.Length,
                sourcePointer = 0x0;

            // Initalize the LZ dictionary
            LzWindowDictionary dictionary = new LzWindowDictionary();
            dictionary.SetWindowSize(0x1000);
            dictionary.SetMaxMatchAmount(0x1000);

            using (var buffer = new MemoryStream(32)) // Will never contain more than 32 bytes
            {
                // Write out the header
                // Magic code & decompressed length
                if (sourceLength <= 0xFFFFFF)
                {
                    destination.Write(0x11 | (sourceLength << 8));
                }
                else
                {
                    destination.WriteByte(0x11);
                    destination.Write(sourceLength);
                }

                // Start compression
                while (sourcePointer < sourceLength)
                {
                    byte flag = 0;

                    for (int i = 7; i >= 0; i--)
                    {
                        // Search for a match
                        int[] match = dictionary.Search(sourceArray, (uint)sourcePointer, (uint)sourceLength);

                        if (match[1] > 0) // There is a match
                        {
                            flag |= (byte)(1 << i);

                            // How many bytes will the match take up?
                            if (match[1] <= 0xF + 1) // 2 bytes
                            {
                                buffer.Write((ushort)((match[1] - 1) << 12 | ((match[0] - 1) & 0xFFF)), Endian.Big);
                            }
                            else if (match[1] <= 0xFF + 17) // 3 bytes
                            {
                                buffer.WriteByte((byte)(((match[1] - 17) & 0xFF) >> 4));
                                buffer.Write((ushort)((match[1] - 17) << 12 | ((match[0] - 1) & 0xFFF)), Endian.Big);
                            }
                            else // 4 bytes
                            {
                                buffer.Write((uint)(0x10000000 | ((match[1] - 273) & 0xFFFF) << 12 | ((match[0] - 1) & 0xFFF)), Endian.Big);
                            }

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

                    // Flush the buffer and write it to the destination stream
                    destination.WriteByte(flag);

                    buffer.WriteTo(destination);
                    buffer.SetLength(0);
                }
            }
        }
    }
}

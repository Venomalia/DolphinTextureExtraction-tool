using AuroraLip.Common;
using System;
using System.IO;

namespace AuroraLip.Compression.Formats
{
    /// <summary>
    /// Nintendo LZ10 compression algorithm
    /// </summary>
    public class LZ10 : ICompression
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 16 && stream.ReadByte() == 16;

        public void Compress(in byte[] source, Stream destination)
        {
            // LZ10 compression can only handle files smaller than 16MB
            if (source.Length > 0xFFFFFF)
            {
                throw new Exception($"{typeof(LZ10)} compression can't be used to compress files larger than {0xFFFFFF:N0} bytes.");
            }
            // Write out the header
            destination.Write(0x10 | (source.Length << 8));

            Compress_ALG(source, destination);
        }

        public byte[] Decompress(Stream source)
        {
            source.Position += 1;
            int destinationLength = (int)source.ReadUInt24();

            var Decom = Decompress_ALG(source, destinationLength);

            //has chunks?
            if (source.Position != source.Length)
            {
                var buffer = new MemoryStream();
                buffer.Write(Decom);
                try
                {
                    while (source.Position != source.Length)
                    {
                        byte Test = source.ReadUInt8();

                        if (Test != 0)
                        {
                            if (Test == 16)
                            {
                                Events.NotificationEvent?.Invoke(NotificationType.Info, $"{typeof(LZ10)} new chunk found at {source.Position}.");
                                destinationLength = (int)source.ReadUInt24();
                                Decom = Decompress_ALG(source, destinationLength);
                                buffer.Write(Decom);
                            }
                            else
                            {
                                Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{typeof(LZ10)} file contains {source.Length - source.Position} unread bytes, starting at position {source.Position}!");
                                break;
                            }
                        }
                    }
                }
                catch (System.Exception)
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{typeof(LZ10)} chunk error at {source.Position}.");
                }
                return buffer.ToArray();
            }

            return Decom;
        }

        public static byte[] Decompress_ALG(Stream source, int decomLength)
        {
            byte[] destination = new byte[decomLength];
            int destinationPointer = 0;

            while (true)
            {
                byte flag = source.ReadUInt8();
                for (int i = 0; i < 8; i++)
                {
                    if ((flag & 0x80) != 0) // Compressed
                    {
                        byte data1 = source.ReadUInt8();
                        byte data2 = source.ReadUInt8();
                        int matchDistance = ((data1 & 0xf) << 8 | data2) + 1;
                        int matchLength = (data1 >> 4) + 3;
                        for (int j = 0; j < matchLength; j++)
                        {
                            destination[destinationPointer] = destination[destinationPointer - matchDistance];
                            destinationPointer++;
                        }
                    }
                    else // Not compressed
                    {
                        destination[destinationPointer++] = source.ReadUInt8();
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
        public static void Compress_ALG(byte[] sourceArray, Stream destination)
        {
            int sourceLength = sourceArray.Length,
                sourcePointer = 0x0;

            // Initalize the LZ dictionary
            LzWindowDictionary dictionary = new LzWindowDictionary();
            dictionary.SetWindowSize(0x1000);
            dictionary.SetMaxMatchAmount(0xF + 3);

            using (var buffer = new MemoryStream(16)) // Will never contain more than 16 bytes
            {
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

                            buffer.Write((ushort)((match[1] - 3) << 12 | ((match[0] - 1) & 0xFFF)), Endian.Big);

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

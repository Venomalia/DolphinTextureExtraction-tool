using AuroraLib.Common;

namespace AuroraLib.Compression.Formats
{

    /*
     * base on Puyo Tools
     * https://github.com/nickworonekin/puyotools/blob/master/src/PuyoTools.Core/Compression/Formats
     * LZ00 decompression support by QPjDDYwQLI thanks to the author of ps2dis
     */

    /// <summary>
    /// Standard LZ algorithm with encryption
    /// </summary>
    public class LZ00 : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public string Magic => magic;

        public const string magic = "LZ00";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        public void Compress(in byte[] source, Stream destination)
        {
            // Get the encryption key
            // Since the original files appear to use the time the file was compressed (as Unix time), we will do the same.
            uint key = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var destinationStartPosition = destination.Position;
            // Write out the header
            destination.Write(magic.ToByte());
            destination.Write(0); // Compressed length (will be filled in later)
            destination.Write(0);
            destination.Write(0);

            /*
                        // Filename (or null bytes)
                        if (destination is FileStream fs)
                        {
                            destination.WriteString(Path.GetFileName(fs.Name)); //, 32, EncodingExtensions.ShiftJIS);
                        }
                        else
                        {
                            destination.Write(new byte[32]); // Elements in array default to 0
                        }
             */
            destination.Write(new byte[32]); // Elements in array default to 0

            destination.Write(source.Length); // Decompressed length
            destination.Write(key); // Encryption key
            destination.Write(0);
            destination.Write(0);

            Compress_ALG(source, destination, key);

            // Go back to the beginning of the file and write out the compressed length
            var destinationLength = (int)(destination.Position - destinationStartPosition);
            destination.At(destinationStartPosition + 4, x => x.Write(destinationLength));

        }

        public byte[] Decompress(Stream source)
        {
            source.Position += 4;


            // Get the source length, destination length, and encryption key
            int sourceLength = source.ReadInt32();
            source.Position += 40;

            int destinationLength = source.ReadInt32();
            uint key = source.ReadUInt32();

            source.Position += 8;

            return Decompress_ALG(source, destinationLength, key);
        }

        public static byte[] Decompress_ALG(Stream source, int decomLength, uint key = 0)
        {
            int destinationPointer = 0x0, bufferPointer = 0xFEE;
            byte[] destination = new byte[decomLength];

            // Initalize the buffer
            byte[] buffer = new byte[0x1000];

            // Start decompression
            while (true)
            {
                byte flag = Transform(source.ReadUInt8(), ref key);

                for (int i = 0; i < 8; i++)
                {
                    if ((flag & 0x1) != 0) // Not compressed
                    {
                        byte value = Transform(source.ReadUInt8(), ref key);

                        destination[destinationPointer++] = value;

                        buffer[bufferPointer] = value;
                        bufferPointer = (bufferPointer + 1) & 0xFFF;
                    }
                    else // Compressed
                    {
                        ushort matchPair = Transform(source.ReadUInt16(), ref key);

                        int matchOffset = ((matchPair >> 4) & 0xF00) | (matchPair & 0xFF);
                        int matchLength = ((matchPair >> 8) & 0xF) + 3;

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


        public static void Compress_ALG(in byte[] sourceArray, Stream destination, uint key = 0)
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
                    destination.WriteByte(Transform(flag, ref key));

                    // Loop through the buffer and encrypt the contents before writing it to the destination
                    var backingBuffer = buffer.GetBuffer();
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        backingBuffer[i] = Transform(backingBuffer[i], ref key);
                    }

                    buffer.WriteTo(destination);
                    buffer.SetLength(0);
                }
            }
        }

        private static byte Transform(byte value, ref uint key)
        {
            // Generate a new key
            uint x = (((((((key << 1) + key) << 5) - key) << 5) + key) << 7) - key;
            x = (x << 6) - x;
            x = (x << 4) - x;

            key = ((x << 2) - x) + 12345;

            // Now return the value since we have the key
            uint t = (key >> 16) & 0x7FFF;
            return (byte)(value ^ ((((t << 8) - t) >> 15)));
        }

        private static ushort Transform(ushort value, ref uint key)
        {
            return (ushort)(Transform((byte)(value & 0xFF), ref key) | (Transform((byte)(value >> 8), ref key) << 8));
        }

    }
}

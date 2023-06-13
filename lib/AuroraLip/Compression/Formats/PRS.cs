using AuroraLib.Common;
using AuroraLip.Compression;

namespace AuroraLib.Compression.Formats
{
    /*
     * Puyo Tools
     * https://github.com/nickworonekin/puyotools/blob/master/src/PuyoTools.Core/Compression/Formats/PrsCompression.cs
     * PRS compression implementation from FraGag.Compression.Prs
     * https://github.com/FraGag/prs.net
     */

    /// <summary>
    /// The PRS compression algorithm is based on LZ77 with run-length encoding emulation and extended matches.
    /// </summary>
    public partial class PRS : ICompression
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public static Endian Order => Endian.Little;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 6 & stream.PeekByte() > 17 && (stream.ReadByte() & 0x1) == 1 && stream.At(-2, SeekOrigin.End, S => S.ReadUInt16()) == 0;

        public byte[] Decompress(Stream source)
            => Decompress_ALG(source, Order);

        public void Compress(in byte[] source, Stream destination)
            => Compress_ALG(source, destination, Order);

        public static byte[] Decompress_ALG(Stream source, Endian order = Endian.Little)
        {
            int lookBehindOffset, lookBehindLength;

            Stream destination = new MemoryStream();
            FlagReader Flag = new(source, order);

            while (source.Position < source.Length)
            {
                if (Flag.Readbit())  // Uncompressed value
                {
                    destination.WriteByte(source.ReadUInt8());
                    continue;
                }

                if (Flag.Readbit()) // Compressed value
                {
                    // Long
                    lookBehindOffset = source.ReadUInt16(order);

                    if (lookBehindOffset == 0 && source.Position < source.Length)
                    {
                        break;
                    }

                    lookBehindLength = lookBehindOffset & 7;
                    lookBehindOffset >>= 3;
                    lookBehindOffset |= -0x2000;
                    if (lookBehindLength == 0)
                    {
                        lookBehindLength = source.ReadUInt8() + 1;
                    }
                    else
                    {
                        lookBehindLength += 2;
                    }
                }
                else
                {
                    // Short
                    lookBehindLength = Flag.ReadInt(2) + 2;
                    lookBehindOffset = source.ReadUInt8();
                    lookBehindOffset |= -0x100;
                }

                for (int i = 0; i < lookBehindLength; i++)
                {
                    destination.WriteByte(destination.At(lookBehindOffset, SeekOrigin.Current, s => s.ReadUInt8()));
                }
            }

            return destination.ToArray();
        }

        public static void Compress_ALG(in byte[] source, Stream destination, Endian order = Endian.Little)
        {
            int position = 0;
            int currentLookBehindPosition, currentLookBehindLength;
            int lookBehindOffset, lookBehindLength;

            MemoryStream buffer = new();
            FlagWriter flagWriter = new(destination, buffer, order);

            while (position < source.Length)
            {
                lookBehindOffset = 0;
                lookBehindLength = 0;

                for (currentLookBehindPosition = position - 1; (currentLookBehindPosition >= 0) && (currentLookBehindPosition >= position - 0x1FF0) && (lookBehindLength < 256); currentLookBehindPosition--)
                {
                    currentLookBehindLength = 1;
                    if (source[currentLookBehindPosition] == source[position])
                    {
                        do
                        {
                            currentLookBehindLength++;
                        }
                        while ((currentLookBehindLength <= 256) &&
                            (position + currentLookBehindLength <= source.Length) &&
                            source[currentLookBehindPosition + currentLookBehindLength - 1] == source[position + currentLookBehindLength - 1]);

                        currentLookBehindLength--;
                        if (((currentLookBehindLength >= 2 && currentLookBehindPosition - position >= -0x100) || currentLookBehindLength >= 3) && currentLookBehindLength > lookBehindLength)
                        {
                            lookBehindOffset = currentLookBehindPosition - position;
                            lookBehindLength = currentLookBehindLength;
                        }
                    }
                }

                if (lookBehindLength == 0) // Uncompressed value
                {
                    buffer.WriteByte(source[position++]);
                    flagWriter.WriteBit(true);
                }
                else // Compressed value
                {
                    position += lookBehindLength;
                    flagWriter.WriteBit(false);
                    if ((lookBehindOffset >= -0x100) && (lookBehindLength <= 5))
                    {
                        // Short
                        flagWriter.WriteBit(false);
                        lookBehindLength -= 2;
                        flagWriter.WriteBit(((lookBehindLength >> 1) & 1) == 1);
                        buffer.WriteByte((byte)(lookBehindOffset & 0xFF));
                        flagWriter.WriteBit((lookBehindLength & 1) == 1);
                    }
                    else
                    {
                        // Long
                        if (lookBehindLength <= 9)
                        {
                            lookBehindLength -= 2;
                            ushort value = (ushort)((lookBehindOffset << 3) | (lookBehindLength & 0x07));
                            buffer.Write(value, order);
                        }
                        else
                        {
                            ushort value = (ushort)((lookBehindOffset << 3));
                            buffer.Write(value, order);
                            buffer.WriteByte((byte)(lookBehindLength - 1));
                        }
                        flagWriter.WriteBit(true);
                    }
                }
            }

            flagWriter.WriteBit(false);
            flagWriter.WriteBit(true);
            flagWriter.Flush();

            destination.WriteByte(0);
            destination.WriteByte(0);
            return;
        }

    }
}

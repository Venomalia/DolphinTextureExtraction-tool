using AuroraLip.Common;
using System.IO;
using System.Linq;

namespace AuroraLip.Compression.Formats
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
    public class PRS : ICompression
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 3 && (stream.ReadByte() & 0x1) == 1 && stream.At(-2, SeekOrigin.End, S => S.ReadUInt16()) == 0;

        public byte[] Decompress(in byte[] Data)
        {
            int bitPos = 9;
            byte currentByte;
            int lookBehindOffset, lookBehindLength;

            Stream source = new MemoryStream(Data);
            Stream destination = new MemoryStream();

            currentByte = source.ReadUInt8();
            while (true)
            {
                if (GetControlBit(ref bitPos, ref currentByte, source) != 0)
                {
                    // Direct byte
                    destination.WriteByte(source.ReadUInt8());
                    continue;
                }

                if (GetControlBit(ref bitPos, ref currentByte, source) != 0)
                {
                    lookBehindOffset = source.ReadUInt8();
                    lookBehindOffset |= source.ReadUInt8() << 8;
                    if (lookBehindOffset == 0)
                    {
                        // End of the compressed data
                        break;
                    }

                    lookBehindLength = lookBehindOffset & 7;
                    lookBehindOffset = (lookBehindOffset >> 3) | -0x2000;
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
                    lookBehindLength = 0;
                    lookBehindLength = (lookBehindLength << 1) | GetControlBit(ref bitPos, ref currentByte, source);
                    lookBehindLength = (lookBehindLength << 1) | GetControlBit(ref bitPos, ref currentByte, source);
                    lookBehindOffset = source.ReadUInt8() | -0x100;
                    lookBehindLength += 2;
                }

                for (int i = 0; i < lookBehindLength; i++)
                {
                    long writePosition = destination.Position;
                    destination.Seek(writePosition + lookBehindOffset, SeekOrigin.Begin);
                    byte b = destination.ReadUInt8();
                    destination.Seek(writePosition, SeekOrigin.Begin);
                    destination.WriteByte(b);
                }
            }

            return destination.ToArray();
        }

        public byte[] Compress(in byte[] Data)
        {

            Stream source = new MemoryStream(Data);
            Stream destination = new MemoryStream();

            // Get the source length
            int sourceLength = (int)(source.Length - source.Position);

            byte[] sourceArray = new byte[sourceLength];
            var totalSourceBytesRead = 0;
            int sourceBytesRead;
            do
            {
                sourceBytesRead = source.Read(sourceArray, totalSourceBytesRead, sourceLength - totalSourceBytesRead);
                if (sourceBytesRead == 0)
                {
                    throw new IOException($"Unable to read all bytes in {nameof(source)}");
                }
                totalSourceBytesRead += sourceBytesRead;
            }
            while (totalSourceBytesRead < sourceLength);

            byte bitPos = 0;
            byte controlByte = 0;

            int position = 0;
            int currentLookBehindPosition, currentLookBehindLength;
            int lookBehindOffset, lookBehindLength;

            MemoryStream data = new MemoryStream();

            while (position < sourceLength)
            {
                currentLookBehindLength = 0;
                lookBehindOffset = 0;
                lookBehindLength = 0;

                for (currentLookBehindPosition = position - 1; (currentLookBehindPosition >= 0) && (currentLookBehindPosition >= position - 0x1FF0) && (lookBehindLength < 256); currentLookBehindPosition--)
                {
                    currentLookBehindLength = 1;
                    if (sourceArray[currentLookBehindPosition] == sourceArray[position])
                    {
                        do
                        {
                            currentLookBehindLength++;
                        } while ((currentLookBehindLength <= 256) &&
                            (position + currentLookBehindLength <= sourceArray.Length) &&
                            sourceArray[currentLookBehindPosition + currentLookBehindLength - 1] == sourceArray[position + currentLookBehindLength - 1]);

                        currentLookBehindLength--;
                        if (((currentLookBehindLength >= 2 && currentLookBehindPosition - position >= -0x100) || currentLookBehindLength >= 3) && currentLookBehindLength > lookBehindLength)
                        {
                            lookBehindOffset = currentLookBehindPosition - position;
                            lookBehindLength = currentLookBehindLength;
                        }
                    }
                }

                if (lookBehindLength == 0)
                {
                    data.WriteByte(sourceArray[position++]);
                    PutControlBit(1, ref controlByte, ref bitPos, data, destination);
                }
                else
                {
                    Copy(lookBehindOffset, lookBehindLength, ref controlByte, ref bitPos, data, destination);
                    position += lookBehindLength;
                }
            }

            PutControlBit(0, ref controlByte, ref bitPos, data, destination);
            PutControlBit(1, ref controlByte, ref bitPos, data, destination);
            if (bitPos != 0)
            {
                controlByte = (byte)((controlByte << bitPos) >> 8);
                Flush(ref controlByte, ref bitPos, data, destination);
            }

            destination.WriteByte(0);
            destination.WriteByte(0);
            return destination.ToArray();
        }

        private static void Copy(int offset, int size, ref byte controlByte, ref byte bitPos, MemoryStream data, Stream destination)
        {
            if ((offset >= -0x100) && (size <= 5))
            {
                size -= 2;
                PutControlBit(0, ref controlByte, ref bitPos, data, destination);
                PutControlBit(0, ref controlByte, ref bitPos, data, destination);
                PutControlBit((size >> 1) & 1, ref controlByte, ref bitPos, data, destination);
                data.WriteByte((byte)(offset & 0xFF));
                PutControlBit(size & 1, ref controlByte, ref bitPos, data, destination);
            }
            else
            {
                if (size <= 9)
                {
                    PutControlBit(0, ref controlByte, ref bitPos, data, destination);
                    data.WriteByte((byte)(((offset << 3) & 0xF8) | ((size - 2) & 0x07)));
                    data.WriteByte((byte)((offset >> 5) & 0xFF));
                    PutControlBit(1, ref controlByte, ref bitPos, data, destination);
                }
                else
                {
                    PutControlBit(0, ref controlByte, ref bitPos, data, destination);
                    data.WriteByte((byte)((offset << 3) & 0xF8));
                    data.WriteByte((byte)((offset >> 5) & 0xFF));
                    data.WriteByte((byte)(size - 1));
                    PutControlBit(1, ref controlByte, ref bitPos, data, destination);
                }
            }
        }

        private static void PutControlBit(int bit, ref byte controlByte, ref byte bitPos, MemoryStream data, Stream destination)
        {
            controlByte >>= 1;
            controlByte |= (byte)(bit << 7);
            bitPos++;
            if (bitPos >= 8)
            {
                Flush(ref controlByte, ref bitPos, data, destination);
            }
        }

        private static void Flush(ref byte controlByte, ref byte bitPos, MemoryStream data, Stream destination)
        {
            destination.WriteByte(controlByte);
            controlByte = 0;
            bitPos = 0;

            byte[] bytes = data.ToArray();
            destination.Write(bytes, 0, bytes.Length);
            data.SetLength(0);
        }

        private static int GetControlBit(ref int bitPos, ref byte currentByte, Stream source)
        {
            bitPos--;
            if (bitPos == 0)
            {
                currentByte = source.ReadUInt8();
                bitPos = 8;
            }

            int flag = currentByte & 1;
            currentByte >>= 1;
            return flag;
        }

    }
}

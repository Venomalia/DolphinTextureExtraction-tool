using AuroraLib.Common;

namespace AuroraLib.Compression.Formats
{
    /*
     * Super Hackio Incorporated
     * IO Library for Compressing and Decompressing YAZ0 files
     * "Copyright © Super Hackio Incorporated 2020-2021"
     * https://github.com/SuperHackio/Hack.io/blob/master/Hack.io.YAZ0/YAZ0.cs
     */

    /// <summary>
    /// Nintendo YAZ0 compression algorithm
    /// </summary>
    public class YAZ0 : ICompression, IMagicIdentify
    {
        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        public virtual string Magic => magic;

        public const string magic = "Yaz0";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 4 && stream.MatchString(Magic);

        public void Compress(in byte[] source, Stream destination)
        {
            // Write out the header
            destination.Write(magic);
            destination.Write(source.Length);
            destination.Write(0);
            destination.Write(0);

            Compress_ALG(source, destination);
        }

        public byte[] Decompress(Stream source)
        {
            source.Position += 4;
            uint DecompressedSize = source.ReadUInt32(Endian.Big),
                CompressedDataOffset = source.ReadUInt32(Endian.Big),
                UncompressedDataOffset = source.ReadUInt32(Endian.Big);

            return Decompress_ALG(source, DecompressedSize);
        }

        public static byte[] Decompress_ALG(Stream source, uint decomLength)
        {
            byte[] destination = new byte[decomLength];
            int destinationPointer = 0;

            while (destinationPointer < decomLength)
            {
                byte FlagByte = source.ReadUInt8();

                for (int i = 7; i > -1 && (destinationPointer < decomLength); i--)
                {
                    if (FlagByte.GetBit(i) == true)
                        destination[destinationPointer++] = source.ReadUInt8();
                    else
                    {
                        byte Tmp = source.ReadUInt8();
                        int Offset = (((byte)(Tmp & 0x0F) << 8) | source.ReadUInt8()) + 1,
                            Length = (Tmp & 0xF0) == 0 ? source.ReadByte() + 0x12 : (byte)((Tmp & 0xF0) >> 4) + 2;

                        for (int j = 0; j < Length; j++)
                        {
                            destination[destinationPointer] = destination[destinationPointer - Offset];
                            destinationPointer++;
                        }
                    }
                }
            }
            return destination;
        }

        public static void Compress_ALG(in byte[] source, Stream destination)
        {
            ByteCountA = 0;
            MatchPos = 0;
            PrevFlag = 0;
            List<byte> OutputFile = new List<byte>();
            Ret r = new Ret() { srcPos = 0, dstPos = 0 };
            byte[] dst = new byte[24];
            int dstSize = 0;
            //int percent = -1;

            uint validBitCount = 0;
            byte currCodeByte = 0;

            while (r.srcPos < source.Length)
            {
                uint numBytes;
                uint matchPos = 0;
                uint srcPosBak;

                numBytes = EncodeAdvanced(source, source.Length, r.srcPos, ref matchPos);
                if (numBytes < 3)
                {
                    //straight copy
                    dst[r.dstPos++] = source[r.srcPos++];
                    //set flag for straight copy
                    currCodeByte |= (byte)(0x80 >> (int)validBitCount);
                }
                else
                {
                    //RLE part
                    uint dist = (uint)(r.srcPos - matchPos - 1);
                    byte byte1;
                    byte byte2;
                    byte byte3;

                    if (numBytes >= 0x12) // 3 byte encoding
                    {
                        byte1 = (byte)(0 | (dist >> 8));
                        byte2 = (byte)(dist & 0xff);
                        dst[r.dstPos++] = byte1;
                        dst[r.dstPos++] = byte2;
                        // maximum runlength for 3 byte encoding
                        if (numBytes > 0xff + 0x12)
                        {
                            numBytes = (uint)(0xff + 0x12);
                        }
                        byte3 = (byte)(numBytes - 0x12);
                        dst[r.dstPos++] = byte3;
                    }
                    else // 2 byte encoding
                    {
                        byte1 = (byte)(((numBytes - 2) << 4) | (dist >> 8));
                        byte2 = (byte)(dist & 0xff);
                        dst[r.dstPos++] = byte1;
                        dst[r.dstPos++] = byte2;
                    }
                    r.srcPos += (int)numBytes;
                }
                validBitCount++;
                //write eight codes
                if (validBitCount == 8)
                {
                    OutputFile.Add(currCodeByte);
                    for (int i = 0; i < r.dstPos; i++)
                        OutputFile.Add(dst[i]);
                    dstSize += r.dstPos + 1;

                    srcPosBak = (uint)r.srcPos;
                    currCodeByte = 0;
                    validBitCount = 0;
                    r.dstPos = 0;
                }
            }
            if (validBitCount > 0)
            {
                OutputFile.Add(currCodeByte);
                for (int i = 0; i < r.dstPos; i++)
                    OutputFile.Add(dst[i]);
                dstSize += r.dstPos + 1;

                currCodeByte = 0;
                validBitCount = 0;
                r.dstPos = 0;
            }
            destination.Write(OutputFile.ToArray());
        }

        #region Helper

        private struct Ret
        {
            public int srcPos, dstPos;
        }

        private static uint ByteCountA;
        private static uint MatchPos;
        private static int PrevFlag = 0;

        private static uint EncodeAdvanced(byte[] src, int size, int pos, ref uint pMatchPos)
        {
            int startPos = pos - 0x1000;
            uint numBytes = 1;

            // if prevFlag is set, it means that the previous position was determined by look-ahead try.
            // so just use it. this is not the best optimization, but nintendo's choice for speed.
            if (PrevFlag == 1)
            {
                pMatchPos = MatchPos;
                PrevFlag = 0;
                return ByteCountA;
            }
            PrevFlag = 0;
            numBytes = EncodeSimple(src, size, pos, ref MatchPos);
            pMatchPos = MatchPos;

            // if this position is RLE encoded, then compare to copying 1 byte and next position(pos+1) encoding
            if (numBytes >= 3)
            {
                ByteCountA = EncodeSimple(src, size, pos + 1, ref MatchPos);
                // if the next position encoding is +2 longer than current position, choose it.
                // this does not guarantee the best optimization, but fairly good optimization with speed.
                if (ByteCountA >= numBytes + 2)
                {
                    numBytes = 1;
                    PrevFlag = 1;
                }
            }
            return numBytes;
        }

        private static uint EncodeSimple(byte[] src, int size, int pos, ref uint pMatchPos)
        {
            int startPos = pos - 0x1000;
            uint numBytes = 1;
            uint matchPos = 0;

            if (startPos < 0)
            {
                startPos = 0;
            }
            for (int i = startPos; i < pos; i++)
            {
                int j;
                for (j = 0; j < size - pos; j++)
                {
                    if (src[i + j] != src[j + pos])
                    {
                        break;
                    }
                }
                if (j > numBytes)
                {
                    numBytes = (uint)j;
                    matchPos = (uint)i;
                }
            }
            pMatchPos = matchPos;
            if (numBytes == 2)
            {
                numBytes = 1;
            }
            return numBytes;
        }

        #endregion Helper
    }
}

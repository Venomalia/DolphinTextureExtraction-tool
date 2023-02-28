using AuroraLip.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AuroraLip.Compression.Formats
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

        public string Magic { get; } = "Yaz0";

        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 4 && stream.MatchString(Magic);


        public void Compress(in byte[] source, Stream destination)
        {
            destination.Write(Compress(source));
        }
        public byte[] Compress(in byte[] data)
        {
            ByteCountA = 0;
            MatchPos = 0;
            PrevFlag = 0;
            List<byte> OutputFile = new List<byte>() { 0x59, 0x61, 0x7A, 0x30 };
            OutputFile.AddRange(BitConverter.GetBytes(data.Length).Reverse());
            OutputFile.AddRange(new byte[8]);
            Ret r = new Ret() { srcPos = 0, dstPos = 0 };
            byte[] dst = new byte[24];
            int dstSize = 0;
            //int percent = -1;

            uint validBitCount = 0;
            byte currCodeByte = 0;

            while (r.srcPos < data.Length)
            {
                uint numBytes;
                uint matchPos = 0;
                uint srcPosBak;

                numBytes = EncodeAdvanced(data, data.Length, r.srcPos, ref matchPos);
                if (numBytes < 3)
                {
                    //straight copy
                    dst[r.dstPos] = data[r.srcPos];
                    r.dstPos++;
                    r.srcPos++;
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
                //if ((r.srcPos + 1) * 100 / srcSize != percent)
                //{
                //    percent = (r.srcPos + 1) * 100 / srcSize;
                //}
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

            return OutputFile.ToArray();
        }

        public byte[] Decompress(Stream source)
        {
            if (source.ReadString(4) != Magic)
                throw new Exception($"{typeof(YAZ0)}:Invalid Identifier");
            
            uint DecompressedSize = source.ReadUInt32(Endian.Big),
                CompressedDataOffset = source.ReadUInt32(Endian.Big),
                UncompressedDataOffset = source.ReadUInt32(Endian.Big);

            List<byte> Decoding = new List<byte>();
            while (Decoding.Count < DecompressedSize)
            {
                byte FlagByte = (byte)source.ReadByte();
                BitArray FlagSet = new BitArray(new byte[1] { FlagByte });

                for (int i = 7; i > -1 && (Decoding.Count < DecompressedSize); i--)
                {
                    if (FlagSet[i] == true)
                        Decoding.Add((byte)source.ReadByte());
                    else
                    {
                        byte Tmp = (byte)source.ReadByte();
                        int Offset = (((byte)(Tmp & 0x0F) << 8) | (byte)source.ReadByte()) + 1,
                            Length = (Tmp & 0xF0) == 0 ? source.ReadByte() + 0x12 : (byte)((Tmp & 0xF0) >> 4) + 2;

                        for (int j = 0; j < Length; j++)
                            Decoding.Add(Decoding[Decoding.Count - Offset]);
                    }
                }
            }
            return Decoding.ToArray();
        }


        #region Helper

        struct Ret
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

        private static uint ToDword(uint d)
        {
            byte w1 = (byte)(d & 0xFF);
            byte w2 = (byte)((d >> 8) & 0xFF);
            byte w3 = (byte)((d >> 16) & 0xFF);
            byte w4 = (byte)(d >> 24);
            return (uint)((w1 << 24) | (w2 << 16) | (w3 << 8) | w4);
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
        #endregion

    }
}

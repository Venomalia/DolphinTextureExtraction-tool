using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Hack.io.YAZ0
{
    /// <summary>
    /// Class containing methods to compress and decompress Data into Yaz0
    /// </summary>
    public static class YAZ0
    {
        private const string Magic = "Yaz0";
        /// <summary>
        /// Decompress a File
        /// </summary>
        /// <param name="Filename">Full path to the file</param>
        public static void Decompress(string Filename) => File.WriteAllBytes(Filename, Decomp(File.ReadAllBytes(Filename)));
        /// <summary>
        /// Decompress a MemoryStream
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static MemoryStream Decompress(MemoryStream Data) => new MemoryStream(Decomp(Data.ToArray()));
        /// <summary>
        /// Decompress a byte[]
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] Data) => Decomp(Data);
        /// <summary>
        /// Compress a File
        /// </summary>
        /// <param name="Filename">File to compress</param>
        /// <param name="Quick">If true, takes shorter time to compress, but is overall weaker then if disabled (resulting in larger files)</param>
        public static void Compress(string Filename, bool Quick = false) => File.WriteAllBytes(Filename, Quick ? QuickCompress(File.ReadAllBytes(Filename)) : DoCompression(File.ReadAllBytes(Filename)));
        /// <summary>
        /// Compress a MemoryStream
        /// </summary>
        /// <param name="YAZ0">MemoryStream to compress</param>
        /// <param name="Quick">The Algorithm to use. True to use YAZ0 Fast</param>
        public static MemoryStream Compress(MemoryStream YAZ0, bool Quick = false) => new MemoryStream(Quick ? QuickCompress(YAZ0.ToArray()) : DoCompression(YAZ0.ToArray()));
        /// <summary>
        /// Compress a byte[]
        /// </summary>
        /// <param name="Data">The data to compress</param>
        /// <param name="Quick">The Algorithm to use. True to use YAZ0 Fast</param>
        /// <returns></returns>
        public static byte[] Compress(byte[] Data, bool Quick = false) => Quick ? QuickCompress(Data) : DoCompression(Data);
        /// <summary>
        /// Checks a given file for Yaz0 Encoding
        /// </summary>
        /// <param name="Filename">File to check</param>
        /// <returns>true if the file is Yaz0 Encoded</returns>
        public static bool Check(string Filename)
        {
            FileStream YAZ0 = new FileStream(Filename, FileMode.Open);
            bool Check = YAZ0.ReadString(4) == Magic;
            YAZ0.Close();
            return Check;
        }
        /// <summary>
        /// Converts a Yaz0 Encoded file to a Yaz0 Decoded MemoryStream
        /// </summary>
        /// <param name="Filename">The file to decode into a MemoryStream</param>
        /// <returns>The decoded MemoryStream</returns>
        public static MemoryStream DecompressToMemoryStream(string Filename) => new MemoryStream(Decomp(File.ReadAllBytes(Filename)));
        
        private static byte[] DoCompression(byte[] file) => Encode(file);
        /*{
            Based on https://github.com/Daniel-McCarthy/Mr-Peeps-Compressor/blob/master/PeepsCompress/PeepsCompress/Algorithm%20Classes/YAZ0.cs
            List<byte> InstructionBits = new List<byte>();
            List<byte> SetDictionaries = new List<byte>();
            List<byte> UncompressedData = new List<byte>();
            List<int[]> CompressedData = new List<int[]>();

            int maxDictionarySize = 4096;
            int minMatchLength = 3;
            int maxMatchLength = 255 + 0x12;
            int decompressedSize = 0;

            for (int i = 0; i < file.Length; i++)
            {
                if (SetDictionaries.Contains(file[i]))
                {
                    //compressed data
                    int[] matches = FindAllMatches(ref SetDictionaries, file[i]);
                    int[] bestMatch = FindLargestMatch(ref SetDictionaries, matches, ref file, i, maxMatchLength);

                    if (bestMatch[1] >= minMatchLength)
                    {
                        InstructionBits.Add(0);
                        bestMatch[0] = SetDictionaries.Count - bestMatch[0];

                        for (int j = 0; j < bestMatch[1]; j++)
                            SetDictionaries.Add(file[i + j]);

                        i = i + bestMatch[1] - 1;

                        CompressedData.Add(bestMatch);
                        decompressedSize += bestMatch[1];
                    }
                    else
                    {
                        //uncompressed data
                        InstructionBits.Add(1);
                        UncompressedData.Add(file[i]);
                        SetDictionaries.Add(file[i]);
                        decompressedSize++;
                    }
                }
                else
                {
                    //uncompressed data
                    InstructionBits.Add(1);
                    UncompressedData.Add(file[i]);
                    SetDictionaries.Add(file[i]);
                    decompressedSize++;
                }

                if (SetDictionaries.Count > maxDictionarySize)
                {
                    int overflow = SetDictionaries.Count - maxDictionarySize;
                    SetDictionaries.RemoveRange(0, overflow);
                }
            }

            return BuildFinalBlocks(ref InstructionBits, ref UncompressedData, ref CompressedData, file.Length, 0);
        }*/

        //From https://github.com/Gericom/EveryFileExplorer/blob/master/CommonCompressors/YAZ0.cs
        private static unsafe byte[] QuickCompress(byte[] Data)
        {
            byte* dataptr = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(Data, 0);

            byte[] result = new byte[Data.Length + Data.Length / 8 + 0x10];
            byte* resultptr = (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(result, 0);
            *resultptr++ = (byte)'Y';
            *resultptr++ = (byte)'a';
            *resultptr++ = (byte)'z';
            *resultptr++ = (byte)'0';
            *resultptr++ = (byte)((Data.Length >> 24) & 0xFF);
            *resultptr++ = (byte)((Data.Length >> 16) & 0xFF);
            *resultptr++ = (byte)((Data.Length >> 8) & 0xFF);
            *resultptr++ = (byte)((Data.Length >> 0) & 0xFF);
            for (int i = 0; i < 8; i++) *resultptr++ = 0;
            int length = Data.Length;
            int dstoffs = 16;
            int Offs = 0;
            while (true)
            {
                int headeroffs = dstoffs++;
                resultptr++;
                byte header = 0;
                for (int i = 0; i < 8; i++)
                {
                    int comp = 0;
                    int back = 1;
                    int nr = 2;
                    {
                        byte* ptr = dataptr - 1;
                        int maxnum = 0x111;
                        if (length - Offs < maxnum) maxnum = length - Offs;
                        //Use a smaller amount of bytes back to decrease time
                        int maxback = 0x400;//0x1000;
                        if (Offs < maxback) maxback = Offs;
                        maxback = (int)dataptr - maxback;
                        int tmpnr;
                        while (maxback <= (int)ptr)
                        {
                            if (*(ushort*)ptr == *(ushort*)dataptr && ptr[2] == dataptr[2])
                            {
                                tmpnr = 3;
                                while (tmpnr < maxnum && ptr[tmpnr] == dataptr[tmpnr]) tmpnr++;
                                if (tmpnr > nr)
                                {
                                    if (Offs + tmpnr > length)
                                    {
                                        nr = length - Offs;
                                        back = (int)(dataptr - ptr);
                                        break;
                                    }
                                    nr = tmpnr;
                                    back = (int)(dataptr - ptr);
                                    if (nr == maxnum) break;
                                }
                            }
                            --ptr;
                        }
                    }
                    if (nr > 2)
                    {
                        Offs += nr;
                        dataptr += nr;
                        if (nr >= 0x12)
                        {
                            *resultptr++ = (byte)(((back - 1) >> 8) & 0xF);
                            *resultptr++ = (byte)((back - 1) & 0xFF);
                            *resultptr++ = (byte)((nr - 0x12) & 0xFF);
                            dstoffs += 3;
                        }
                        else
                        {
                            *resultptr++ = (byte)((((back - 1) >> 8) & 0xF) | (((nr - 2) & 0xF) << 4));
                            *resultptr++ = (byte)((back - 1) & 0xFF);
                            dstoffs += 2;
                        }
                        comp = 1;
                    }
                    else
                    {
                        *resultptr++ = *dataptr++;
                        dstoffs++;
                        Offs++;
                    }
                    header = (byte)((header << 1) | ((comp == 1) ? 0 : 1));
                    if (Offs >= length)
                    {
                        header = (byte)(header << (7 - i));
                        break;
                    }
                }
                result[headeroffs] = header;
                if (Offs >= length) break;
            }
            while ((dstoffs % 4) != 0) dstoffs++;
            byte[] realresult = new byte[dstoffs];
            Array.Copy(result, realresult, dstoffs);
            return realresult;
        }

        //private static int[] FindAllMatches(ref List<byte> dictionary, byte match)
        //{
        //    List<int> matchPositons = new List<int>();

        //    for (int i = 0; i < dictionary.Count; i++)
        //        if (dictionary[i] == match)
        //            matchPositons.Add(i);

        //    return matchPositons.ToArray();
        //}

        //private static int[] FindLargestMatch(ref List<byte> dictionary, int[] matchesFound, ref byte[] file, int fileIndex, int maxMatch)
        //{
        //    int[] matchSizes = new int[matchesFound.Length];

        //    for (int i = 0; i < matchesFound.Length; i++)
        //    {
        //        int matchSize = 1;
        //        bool matchFound = true;

        //        while (matchFound && matchSize < maxMatch && (fileIndex + matchSize < file.Length) && (matchesFound[i] + matchSize < dictionary.Count)) //NOTE: This could be relevant to compression issues? I suspect it's more related to writing
        //        {
        //            if (file[fileIndex + matchSize] == dictionary[matchesFound[i] + matchSize])
        //                matchSize++;
        //            else
        //                matchFound = false;
        //        }

        //        matchSizes[i] = matchSize;
        //    }

        //    int[] bestMatch = new int[2];

        //    bestMatch[0] = matchesFound[0];
        //    bestMatch[1] = matchSizes[0];

        //    for (int i = 1; i < matchesFound.Length; i++)
        //    {
        //        if (matchSizes[i] > bestMatch[1])
        //        {
        //            bestMatch[0] = matchesFound[i];
        //            bestMatch[1] = matchSizes[i];
        //        }
        //    }

        //    return bestMatch;
        //}

        //private static byte[] BuildFinalBlocks(ref List<byte> layoutBits, ref List<byte> uncompressedData, ref List<int[]> offsetLengthPairs, int decompressedSize, int offset)
        //{
        //    List<byte> finalYAZ0Block = new List<byte>();
        //    List<byte> layoutBytes = new List<byte>();
        //    List<byte> compressedDataBytes = new List<byte>();
        //    List<byte> extendedLengthBytes = new List<byte>();

        //    //add Yaz0 magic number
        //    finalYAZ0Block.AddRange(Encoding.ASCII.GetBytes("Yaz0"));

        //    byte[] decompressedSizeArray = new byte[4];
        //    decompressedSizeArray[0] = (byte)((decompressedSize >> 24) & 0xFF);
        //    decompressedSizeArray[1] = (byte)((decompressedSize >> 16) & 0xFF);
        //    decompressedSizeArray[2] = (byte)((decompressedSize >> 8) & 0xFF);
        //    decompressedSizeArray[3] = (byte)((decompressedSize >> 0) & 0xFF);
            
        //    finalYAZ0Block.AddRange(decompressedSizeArray);

        //    //add 8 0's per format specification
        //    for (int i = 0; i < 8; i++)
        //        finalYAZ0Block.Add(0);

        //    //assemble layout bytes
        //    while (layoutBits.Count > 0)
        //    {
        //        while (layoutBits.Count < 8)
        //            layoutBits.Add(0);

        //        string layoutBitsString = layoutBits[0].ToString() + layoutBits[1].ToString() + layoutBits[2].ToString() + layoutBits[3].ToString()
        //                + layoutBits[4].ToString() + layoutBits[5].ToString() + layoutBits[6].ToString() + layoutBits[7].ToString();

        //        byte[] layoutByteArray = new byte[1];
        //        layoutByteArray[0] = Convert.ToByte(layoutBitsString, 2);
        //        layoutBytes.Add(layoutByteArray[0]);
        //        layoutBits.RemoveRange(0, (layoutBits.Count < 8) ? layoutBits.Count : 8);
        //    }

        //    //Final Calculations
        //    foreach (int[] offsetLengthPair in offsetLengthPairs)
        //    {
        //        //if < 18, set 4 bits -2 as matchLength
        //        //if >= 18, set matchLength == 0, write length to new byte - 0x12

        //        int adjustedOffset = offsetLengthPair[0];
        //        int adjustedLength = (offsetLengthPair[1] >= 18) ? 0 : offsetLengthPair[1] - 2; //critical, 4 bit range is 0-15. Number must be at least 3 (if 2, when -2 is done, it will think it is 3 byte format), -2 is how it can store up to 17 without an extra byte because +2 will be added on decompression

        //        if (adjustedLength == 0)
        //            extendedLengthBytes.Add((byte)(offsetLengthPair[1] - 18));

        //        int compressedInt = (adjustedLength << 12) | adjustedOffset - 1;

        //        byte[] compressed2Byte = new byte[2];
        //        compressed2Byte[0] = (byte)(compressedInt & 0XFF);
        //        compressed2Byte[1] = (byte)((compressedInt >> 8) & 0xFF);

        //        compressedDataBytes.Add(compressed2Byte[1]);
        //        compressedDataBytes.Add(compressed2Byte[0]);
        //    }

        //    //Finish
        //    for (int i = 0; i < layoutBytes.Count; i++)
        //    {
        //        finalYAZ0Block.Add(layoutBytes[i]);

        //        BitArray arrayOfBits = new BitArray(new byte[1] { layoutBytes[i] });

        //        for (int j = 7; ((j > -1) && ((uncompressedData.Count > 0) || (compressedDataBytes.Count > 0))); j--)
        //        {
        //            if (arrayOfBits[j] == true)
        //            {
        //                finalYAZ0Block.Add(uncompressedData[0]);
        //                uncompressedData.RemoveAt(0);
        //            }
        //            else
        //            {
        //                if (compressedDataBytes.Count > 0)
        //                {
        //                    int length = compressedDataBytes[0] >> 4;

        //                    finalYAZ0Block.Add(compressedDataBytes[0]);
        //                    finalYAZ0Block.Add(compressedDataBytes[1]);
        //                    compressedDataBytes.RemoveRange(0, 2);

        //                    if (length == 0)
        //                    {
        //                        finalYAZ0Block.Add(extendedLengthBytes[0]);
        //                        extendedLengthBytes.RemoveAt(0);
        //                    }
        //                }
        //            }
        //        }


        //    }

        //    return finalYAZ0Block.ToArray();
        //}

        private static byte[] Decomp(byte[] Data)
        {
            MemoryStream YAZ0 = new MemoryStream(Data);
            if (YAZ0.ReadString(4) != Magic)
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");

            uint DecompressedSize = BitConverter.ToUInt32(YAZ0.ReadReverse(0, 4), 0), CompressedDataOffset = BitConverter.ToUInt32(YAZ0.ReadReverse(0, 4), 0), UncompressedDataOffset = BitConverter.ToUInt32(YAZ0.ReadReverse(0, 4), 0);

            List<byte> Decoding = new List<byte>();
            while (Decoding.Count < DecompressedSize)
            {
                byte FlagByte = (byte)YAZ0.ReadByte();
                BitArray FlagSet = new BitArray(new byte[1] { FlagByte });

                for (int i = 7; i > -1 && (Decoding.Count < DecompressedSize); i--)
                {
                    if (FlagSet[i] == true)
                        Decoding.Add((byte)YAZ0.ReadByte());
                    else
                    {
                        byte Tmp = (byte)YAZ0.ReadByte();
                        int Offset = (((byte)(Tmp & 0x0F) << 8) | (byte)YAZ0.ReadByte()) + 1,
                            Length = (Tmp & 0xF0) == 0 ? YAZ0.ReadByte() + 0x12 : (byte)((Tmp & 0xF0) >> 4) + 2;

                        for (int j = 0; j < Length; j++)
                            Decoding.Add(Decoding[Decoding.Count - Offset]);
                    }
                }
            }
            return Decoding.ToArray();
        }
        
        struct Ret
        {
            public int srcPos, dstPos;
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

        private static byte[] Encode(byte[] src)
        {
            ByteCountA = 0;
            MatchPos = 0;
            PrevFlag = 0;
            List<byte> OutputFile = new List<byte>() { 0x59, 0x61, 0x7A, 0x30 };
            OutputFile.AddRange(BitConverter.GetBytes(src.Length).Reverse());
            OutputFile.AddRange(new byte[8]);
            Ret r = new Ret() { srcPos = 0, dstPos = 0 };
            byte[] dst = new byte[24];
            int dstSize = 0;
            //int percent = -1;

            uint validBitCount = 0;
            byte currCodeByte = 0;

            while (r.srcPos < src.Length)
            {
                uint numBytes;
                uint matchPos = 0;
                uint srcPosBak;

                numBytes = EncodeAdvanced(src, src.Length, r.srcPos, ref matchPos);
                if (numBytes < 3)
                {
                    //straight copy
                    dst[r.dstPos] = src[r.srcPos];
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
    }
}
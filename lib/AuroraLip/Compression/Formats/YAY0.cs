using AuroraLib.Common;
using System.Collections;
using System.Text;

namespace AuroraLib.Compression.Formats
{
    /*
     * Super Hackio Incorporated
     * IO Library for Compressing and Decompressing YAY0 files
     * "Copyright © Super Hackio Incorporated 2020-2021"
     * https://github.com/SuperHackio/Hack.io/blob/master/Hack.io.YAY0/YAY0.cs
     */

    /// <summary>
    /// Nintendo YAY0 compression algorithm
    /// </summary>
    public class YAY0 : ICompression, IMagicIdentify
    {
        public string Magic { get; } = "Yay0";

        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 4 && stream.MatchString(Magic);

        public void Compress(in byte[] source, Stream destination)
        {
            destination.Write(Compress(source));
        }

        //Based on https://github.com/Daniel-McCarthy/Mr-Peeps-Compressor/blob/master/PeepsCompress/PeepsCompress/Algorithm%20Classes/YAY0.cs
        public byte[] Compress(in byte[] data)
        {
            List<byte> layoutBits = new List<byte>();
            List<byte> dictionary = new List<byte>();

            List<byte> uncompressedData = new List<byte>();
            List<int[]> compressedData = new List<int[]>();

            int maxDictionarySize = 4096;
            int maxMatchLength = 255 + 0x12;
            int minMatchLength = 3;
            int decompressedSize = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (dictionary.Contains(data[i]))
                {
                    //check for best match
                    int[] matches = FindAllMatches(ref dictionary, data[i]);
                    int[] bestMatch = FindLargestMatch(ref dictionary, matches, in data, i, maxMatchLength);

                    if (bestMatch[1] >= minMatchLength)
                    {
                        //add to compressedData
                        layoutBits.Add(0);
                        bestMatch[0] = dictionary.Count - bestMatch[0]; //sets offset in relation to end of dictionary

                        for (int j = 0; j < bestMatch[1]; j++)
                        {
                            dictionary.Add(data[i + j]);
                        }

                        i = i + bestMatch[1] - 1;

                        compressedData.Add(bestMatch);
                        decompressedSize += bestMatch[1];
                    }
                    else
                    {
                        //add to uncompressed data
                        layoutBits.Add(1);
                        uncompressedData.Add(data[i]);
                        dictionary.Add(data[i]);
                        decompressedSize++;
                    }
                }
                else
                {
                    //uncompressed data
                    layoutBits.Add(1);
                    uncompressedData.Add(data[i]);
                    dictionary.Add(data[i]);
                    decompressedSize++;
                }

                if (dictionary.Count > maxDictionarySize)
                {
                    int overflow = dictionary.Count - maxDictionarySize;
                    dictionary.RemoveRange(0, overflow);
                }
            }

            return BuildYAY0CompressedBlock(ref layoutBits, ref uncompressedData, ref compressedData, decompressedSize, 0);
        }

        //Based on https://github.com/LordNed/WArchive-Tools/blob/master/ArchiveToolsLib/Compression/Yay0Decoder.cs
        public byte[] Decompress(Stream source)
        {
            if (source.ReadString(4) != Magic)
                throw new Exception($"{typeof(YAY0)}:Invalid Identifier");

            uint uncompressedSize = source.ReadUInt32(Endian.Big),
                linkTableOffset = source.ReadUInt32(Endian.Big),
                byteChunkAndCountModifiersOffset = source.ReadUInt32(Endian.Big);

            int maskBitCounter = 0, currentOffsetInDestBuffer = 0, currentMask = 0;

            byte[] uncompressedData = new byte[uncompressedSize];

            while (currentOffsetInDestBuffer < uncompressedSize)
            {
                // If we're out of bits, get the next mask.
                if (maskBitCounter == 0)
                {
                    currentMask = source.ReadInt32(Endian.Big);
                    maskBitCounter = 32;
                }

                // If the next bit is set, the chunk is non-linked and just copy it from the non-link table.
                // Do a copy otherwise.
                if (((uint)currentMask & (uint)0x80000000) == 0x80000000)
                {
                    long pauseposition = source.Position;
                    source.Position = byteChunkAndCountModifiersOffset++;
                    uncompressedData[currentOffsetInDestBuffer++] = (byte)source.ReadByte();
                    source.Position = pauseposition;
                }
                else
                {
                    // Read 16-bit from the link table
                    long pauseposition = source.Position;
                    source.Position = linkTableOffset;
                    ushort link = source.ReadUInt16(Endian.Big);
                    linkTableOffset += 2;
                    source.Position = pauseposition;

                    // Calculate the offset
                    int offset = currentOffsetInDestBuffer - (link & 0xfff);

                    // Calculate the count
                    int count = link >> 12;

                    if (count == 0)
                    {
                        pauseposition = source.Position;
                        source.Position = byteChunkAndCountModifiersOffset++;
                        byte countModifier = (byte)source.ReadByte();
                        source.Position = pauseposition;
                        count = countModifier + 18;
                    }
                    else
                        count += 2;

                    // Copy the block
                    int blockCopy = offset;

                    for (int i = 0; i < count; i++)
                        uncompressedData[currentOffsetInDestBuffer++] = uncompressedData[blockCopy++ - 1];
                }

                // Get the next bit in the mask.
                currentMask <<= 1;
                maskBitCounter--;
            }
            return uncompressedData;
        }

        #region Helper

        private static int[] FindAllMatches(ref List<byte> dictionary, byte match)
        {
            List<int> matchPositons = new List<int>();

            for (int i = 0; i < dictionary.Count; i++)
                if (dictionary[i] == match)
                    matchPositons.Add(i);

            return matchPositons.ToArray();
        }

        private static int[] FindLargestMatch(ref List<byte> dictionary, int[] matchesFound, in byte[] file, int fileIndex, int maxMatch)
        {
            int[] matchSizes = new int[matchesFound.Length];

            for (int i = 0; i < matchesFound.Length; i++)
            {
                int matchSize = 1;
                bool matchFound = true;

                while (matchFound && matchSize < maxMatch && (fileIndex + matchSize < file.Length) && (matchesFound[i] + matchSize < dictionary.Count)) //NOTE: This could be relevant to compression issues? I suspect it's more related to writing
                {
                    if (file[fileIndex + matchSize] == dictionary[matchesFound[i] + matchSize])
                        matchSize++;
                    else
                        matchFound = false;
                }

                matchSizes[i] = matchSize;
            }

            int[] bestMatch = new int[2];

            bestMatch[0] = matchesFound[0];
            bestMatch[1] = matchSizes[0];

            for (int i = 1; i < matchesFound.Length; i++)
            {
                if (matchSizes[i] > bestMatch[1])
                {
                    bestMatch[0] = matchesFound[i];
                    bestMatch[1] = matchSizes[i];
                }
            }

            return bestMatch;
        }

        public static byte[] BuildYAY0CompressedBlock(ref List<byte> layoutBits, ref List<byte> uncompressedData, ref List<int[]> offsetLengthPairs, int decompressedSize, int offset)
        {
            List<byte> finalYAY0Block = new List<byte>();
            List<byte> layoutBytes = new List<byte>();
            List<byte> compressedDataBytes = new List<byte>();
            List<byte> extendedLengthBytes = new List<byte>();

            int compressedOffset = 16 + offset; //header size
            int uncompressedOffset;

            //add Yay0 magic number
            finalYAY0Block.AddRange(Encoding.ASCII.GetBytes("Yay0"));

            byte[] decompressedSizeArray = new byte[4];
            decompressedSizeArray[0] = (byte)((decompressedSize >> 24) & 0xFF);
            decompressedSizeArray[1] = (byte)((decompressedSize >> 16) & 0xFF);
            decompressedSizeArray[2] = (byte)((decompressedSize >> 8) & 0xFF);
            decompressedSizeArray[3] = (byte)((decompressedSize >> 0) & 0xFF);

            finalYAY0Block.AddRange(decompressedSizeArray);

            //assemble layout bytes
            while (layoutBits.Count > 0)
            {
                while (layoutBits.Count < 8)
                    layoutBits.Add(0);

                string layoutBitsString = layoutBits[0].ToString() + layoutBits[1].ToString() + layoutBits[2].ToString() + layoutBits[3].ToString()
                        + layoutBits[4].ToString() + layoutBits[5].ToString() + layoutBits[6].ToString() + layoutBits[7].ToString();

                byte[] layoutByteArray = new byte[1];
                layoutByteArray[0] = Convert.ToByte(layoutBitsString, 2);
                layoutBytes.Add(layoutByteArray[0]);
                layoutBits.RemoveRange(0, (layoutBits.Count < 8) ? layoutBits.Count : 8);
            }

            //assemble offsetLength shorts
            foreach (int[] offsetLengthPair in offsetLengthPairs)
            {
                //if < 18, set 4 bits -2 as matchLength
                //if >= 18, set matchLength == 0, write length to new byte - 0x12

                int adjustedOffset = offsetLengthPair[0];
                int adjustedLength = (offsetLengthPair[1] >= 18) ? 0 : offsetLengthPair[1] - 2; //vital, 4 bit range is 0-15. Number must be at least 3 (if 2, when -2 is done, it will think it is 3 byte format), -2 is how it can store up to 17 without an extra byte because +2 will be added on decompression

                int compressedInt = ((adjustedLength << 12) | adjustedOffset - 1);

                byte[] compressed2Byte = new byte[2];
                compressed2Byte[0] = (byte)(compressedInt & 0xFF);
                compressed2Byte[1] = (byte)((compressedInt >> 8) & 0xFF);

                compressedDataBytes.Add(compressed2Byte[1]);
                compressedDataBytes.Add(compressed2Byte[0]);

                if (adjustedLength == 0)
                {
                    extendedLengthBytes.Add((byte)(offsetLengthPair[1] - 18));
                }
            }

            //pad layout bits if needed
            while (layoutBytes.Count % 4 != 0)
            {
                layoutBytes.Add(0);
            }

            compressedOffset += layoutBytes.Count;

            //add final compressed offset
            byte[] compressedOffsetArray = BitConverter.GetBytes(compressedOffset);
            Array.Reverse(compressedOffsetArray);
            finalYAY0Block.AddRange(compressedOffsetArray);

            //add final uncompressed offset
            uncompressedOffset = compressedOffset + compressedDataBytes.Count;
            byte[] uncompressedOffsetArray = BitConverter.GetBytes(uncompressedOffset);
            Array.Reverse(uncompressedOffsetArray);
            finalYAY0Block.AddRange(uncompressedOffsetArray);

            //add layout bits
            foreach (byte layoutByte in layoutBytes)                 //add layout bytes to file
            {
                finalYAY0Block.Add(layoutByte);
            }

            //add compressed data
            foreach (byte compressedByte in compressedDataBytes)     //add compressed bytes to file
            {
                finalYAY0Block.Add(compressedByte);
            }

            //non-compressed/additional-length bytes
            {
                for (int i = 0; i < layoutBytes.Count; i++)
                {
                    BitArray arrayOfBits = new BitArray(new byte[1] { layoutBytes[i] });

                    for (int j = 7; ((j > -1) && ((uncompressedData.Count > 0) || (compressedDataBytes.Count > 0))); j--)
                    {
                        if (arrayOfBits[j] == true)
                        {
                            finalYAY0Block.Add(uncompressedData[0]);
                            uncompressedData.RemoveAt(0);
                        }
                        else
                        {
                            if (compressedDataBytes.Count > 0)
                            {
                                int length = compressedDataBytes[0] >> 4;
                                compressedDataBytes.RemoveRange(0, 2);

                                if (length == 0)
                                {
                                    finalYAY0Block.Add(extendedLengthBytes[0]);
                                    extendedLengthBytes.RemoveAt(0);
                                }
                            }
                        }
                    }
                }
            }

            return finalYAY0Block.ToArray();
        }

        #endregion Helper
    }
}

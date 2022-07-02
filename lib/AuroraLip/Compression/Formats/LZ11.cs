using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Compression.Formats
{
    /*
     * Wexos 
     * Library for Compressing and Decompressing LZ11 files
     * https://wiki.tockdom.com/wiki/Wexos's_Toolbox
     */

    /// <summary>
    /// Nintendo LZ11 compression algorithm
    /// </summary>
    public class LZ11 : ICompression
    {
        public bool CanCompress { get; } = true;

        public bool CanDecompress { get; } = true;

        public byte[] Compress(in byte[] Data)
        {
            int length = (int)Data.Length;
            MemoryStream memoryStream = new MemoryStream(Data);
            MemoryStream memoryStream1 = new MemoryStream();
            byte[] numArray = new byte[length];
            memoryStream.Read(numArray, 0, length);

            int num = 0;
            int length1 = 4;
            LzWindowDictionary lzWindowDictionary = new LzWindowDictionary();
            lzWindowDictionary.SetWindowSize(4096);
            lzWindowDictionary.SetMaxMatchAmount(4096);
            if (length > 16777215)
            {
                memoryStream1.WriteByte(17);
                PTStream.WriteInt32(memoryStream1, length);
                length1 += 4;
            }
            else
            {
                PTStream.WriteInt32(memoryStream1, 17 | length << 8);
            }
            while (num < length)
            {
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    byte num1 = 0;
                    for (int i = 7; i >= 0; i--)
                    {
                        int[] numArray1 = lzWindowDictionary.Search(numArray, (uint)num, (uint)length);
                        if (numArray1[1] <= 0)
                        {
                            memoryStream2.WriteByte(numArray[num]);
                            lzWindowDictionary.AddEntry(numArray, num);
                            lzWindowDictionary.SlideWindow(1);
                            num++;
                        }
                        else
                        {
                            num1 = (byte)(num1 | (byte)(1 << (i & 31)));
                            if (numArray1[1] <= 16)
                            {
                                memoryStream2.WriteByte((byte)((numArray1[1] - 1 & 15) << 4 | (numArray1[0] - 1 & 4095) >> 8));
                                memoryStream2.WriteByte((byte)(numArray1[0] - 1 & 255));
                            }
                            else if (numArray1[1] > 272)
                            {
                                memoryStream2.WriteByte((byte)(16 | (numArray1[1] - 273 & 65535) >> 12));
                                memoryStream2.WriteByte((byte)((numArray1[1] - 273 & 4095) >> 4));
                                memoryStream2.WriteByte((byte)((numArray1[1] - 273 & 15) << 4 | (numArray1[0] - 1 & 4095) >> 8));
                                memoryStream2.WriteByte((byte)(numArray1[0] - 1 & 255));
                            }
                            else
                            {
                                memoryStream2.WriteByte((byte)((numArray1[1] - 17 & 255) >> 4));
                                memoryStream2.WriteByte((byte)((numArray1[1] - 17 & 15) << 4 | (numArray1[0] - 1 & 4095) >> 8));
                                memoryStream2.WriteByte((byte)(numArray1[0] - 1 & 255));
                            }
                            lzWindowDictionary.AddEntryRange(numArray, num, numArray1[1]);
                            lzWindowDictionary.SlideWindow(numArray1[1]);
                            num += numArray1[1];
                        }
                        if (num >= length)
                        {
                            break;
                        }
                    }
                    memoryStream1.WriteByte(num1);
                    memoryStream2.Position = (long)0;
                    while (memoryStream2.Position < memoryStream2.Length)
                    {
                        memoryStream1.WriteByte(PTStream.ReadByte(memoryStream2));
                    }
                    length1 = length1 + (int)memoryStream2.Length + 1;
                }
            }
            return memoryStream1.ToArray();
        }

        public byte[] Decompress(in byte[] Data)
        {
            int num, num1;
            uint data = (uint)(Data[1] | Data[2] << 8 | Data[3] << 16);
            byte[] numArray = new byte[data];
            int num2 = 4;
            int num3 = 0;
            while (true)
            {
                int num4 = num2;
                num2 = num4 + 1;
                byte data1 = Data[num4];
                for (int i = 0; i < 8; i++)
                {
                    if ((data1 & 128) != 0)
                    {
                        int num5 = num2;
                        num2 = num5 + 1;
                        byte data2 = Data[num5];
                        if (data2 >> 4 == 0)
                        {
                            int num6 = num2;
                            num2 = num6 + 1;
                            byte data3 = Data[num6];
                            int num7 = num2;
                            num2 = num7 + 1;
                            byte data4 = Data[num7];
                            num1 = ((data2 & 15) << 4 | data3 >> 4) + 17;
                            num = ((data3 & 15) << 8 | data4) + 1;
                        }
                        else if (data2 >> 4 != 1)
                        {
                            int num8 = num2;
                            num2 = num8 + 1;
                            byte data5 = Data[num8];
                            num = ((data2 & 15) << 8 | data5) + 1;
                            num1 = (data2 >> 4) + 1;
                        }
                        else
                        {
                            int num9 = num2;
                            num2 = num9 + 1;
                            byte data6 = Data[num9];
                            int num10 = num2;
                            num2 = num10 + 1;
                            byte data7 = Data[num10];
                            int num11 = num2;
                            num2 = num11 + 1;
                            byte data8 = Data[num11];
                            num1 = ((data2 & 15) << 12 | data6 << 4 | data7 >> 4) + 273;
                            num = ((data7 & 15) << 8 | data8) + 1;
                        }
                        for (int j = 0; j < num1; j++)
                        {
                            numArray[num3] = numArray[num3 - num];
                            num3++;
                        }
                    }
                    else
                    {
                        int num12 = num3;
                        num3 = num12 + 1;
                        int num13 = num2;
                        num2 = num13 + 1;
                        numArray[num12] = Data[num13];
                    }
                    if (num3 >= data)
                    {
                        return numArray;
                    }
                    data1 = (byte)(data1 << 1);
                }
            }
        }

        public bool IsMatch(in byte[] Data)
        {
            return Data.Length > 4 && Data[0] == 17;
        }

        private class LzWindowDictionary
        {
            private int WindowSize = 4096;

            private int WindowStart;

            private int WindowLength;

            private int MinMatchAmount = 3;

            private int MaxMatchAmount = 18;

            private int BlockSize;

            private readonly List<int>[] OffsetList;

            internal LzWindowDictionary()
            {
                OffsetList = new List<int>[256];
                for (int i = 0; i < OffsetList.Length; i++)
                {
                    OffsetList[i] = new List<int>();
                }
            }

            internal void AddEntry(byte[] DecompressedData, int offset)
            {
                OffsetList[DecompressedData[offset]].Add(offset);
            }

            internal void AddEntryRange(byte[] DecompressedData, int offset, int length)
            {
                for (int i = 0; i < length; i++)
                {
                    AddEntry(DecompressedData, offset + i);
                }
            }

            private void RemoveOldEntries(byte index)
            {
                int num = 0;
                while (num < OffsetList[index].Count && OffsetList[index][num] < WindowStart)
                {
                    OffsetList[index].RemoveAt(0);
                }
            }

            internal int[] Search(byte[] DecompressedData, uint offset, uint length)
            {
                RemoveOldEntries(DecompressedData[offset]);
                if (offset < MinMatchAmount || (length - offset) < MinMatchAmount)
                {
                    return new int[2];
                }
                int[] numArray = new int[2];
                for (int i = OffsetList[DecompressedData[offset]].Count - 1; i >= 0; i--)
                {
                    int item = OffsetList[DecompressedData[offset]][i];
                    int num = 1;
                    while (num < MaxMatchAmount && num < WindowLength && (item + num) < offset && offset + num < length && DecompressedData[checked(offset + num)] == DecompressedData[item + num])
                    {
                        num++;
                    }
                    if (num >= MinMatchAmount && num > numArray[1])
                    {
                        numArray = new int[] { (int)(offset - item), num };
                        if (num == MaxMatchAmount)
                        {
                            break;
                        }
                    }
                }
                return numArray;
            }

            internal void SetBlockSize(int size)
            {
                BlockSize = size;
                WindowLength = size;
            }

            internal void SetMaxMatchAmount(int amount)
            {
                MaxMatchAmount = amount;
            }

            internal void SetMinMatchAmount(int amount)
            {
                MinMatchAmount = amount;
            }

            internal void SetWindowSize(int size)
            {
                WindowSize = size;
            }

            internal void SlideBlock()
            {
                WindowStart += BlockSize;
            }

            internal void SlideWindow(int Amount)
            {
                if (WindowLength == WindowSize)
                {
                    WindowStart += Amount;
                    return;
                }
                if (WindowLength + Amount <= WindowSize)
                {
                    WindowLength += Amount;
                    return;
                }
                Amount -= (WindowSize - WindowLength);
                WindowLength = WindowSize;
                WindowStart += Amount;
            }
        }

        private static class PTStream
        {
            public static byte ReadByte(Stream source)
            {
                int num = source.ReadByte();
                if (num == -1)
                {
                    throw new EndOfStreamException();
                }
                return (byte)num;
            }

            public static void WriteInt32(Stream destination, int value)
            {
                destination.WriteByte((byte)(value & 255));
                destination.WriteByte((byte)(value >> 8 & 255));
                destination.WriteByte((byte)(value >> 16 & 255));
                destination.WriteByte((byte)(value >> 24 & 255));
            }
        }
    }
}

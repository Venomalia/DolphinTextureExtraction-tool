using AuroraLip.Common;
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

        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 4 && stream.ReadByte() == 17;

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
                byte flag = Data[num2++];
                for (int i = 0; i < 8; i++)
                {
                    if ((flag & 128) != 0)
                    {
                        byte data2 = Data[num2++];
                        if (data2 >> 4 == 0)
                        {
                            byte data3 = Data[num2++];
                            byte data4 = Data[num2++];
                            num1 = ((data2 & 15) << 4 | data3 >> 4) + 17;
                            num = ((data3 & 15) << 8 | data4) + 1;
                        }
                        else if (data2 >> 4 != 1)
                        {
                            byte data5 = Data[num2++];
                            num = ((data2 & 15) << 8 | data5) + 1;
                            num1 = (data2 >> 4) + 1;
                        }
                        else
                        {
                            byte data6 = Data[num2++];
                            byte data7 = Data[num2++];
                            byte data8 = Data[num2++];
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
                        numArray[num3++] = Data[num2++];
                    }
                    if (num3 >= data)
                    {

                        //has chunks?
                        if (Data.Length > num2)
                        {
                            //Padding
                            while (Data.Length - 1 > num2 && Data[num2] == 0)
                                num2++;

                            //new chunk?
                            if (Data[num2++] == 17)
                            {
                                Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{typeof(LZ11)} has chunks!");
                            }

                            if (Data.Length > num2)
                                Events.NotificationEvent?.Invoke(NotificationType.Info, $"{typeof(LZ11)} file steam contains {Data.Length - num2} unread bytes, starting at position {num2}.");
                        }
                        return numArray;
                    }
                    flag = (byte)(flag << 1);
                }
            }
        }

        private class LzWindowDictionary
        {
            private int WindowStart, WindowLength, BlockSize;

            private int WindowSize = 4096;

            private int MinMatchAmount = 3;

            private int MaxMatchAmount = 18;

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

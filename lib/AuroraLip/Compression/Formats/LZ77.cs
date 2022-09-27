using AuroraLip.Common;
using System;
using System.IO;

namespace AuroraLip.Compression.Formats
{
    /*
     * Wexos 
     * Library for Compressing and Decompressing LZ77 files
     * https://wiki.tockdom.com/wiki/Wexos's_Toolbox
     */

    /// <summary>
    /// LZ77 compression algorithm
    /// </summary>
    public class LZ77 : ICompression, IMagicIdentify
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string magic { get; } = "LZ77";

        public string Magic => magic;

        public bool IsMatch(Stream stream, in string extension = "")
        {
            if (stream.MatchString(magic) && stream.ReadByte() == 16)
                return true;
            
            stream.Seek(0, SeekOrigin.Begin);
            return stream.Length > 16 && stream.ReadByte() == 16;
        }

        public byte[] Compress(in byte[] Data)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(in byte[] Data)
        {
            if (Data[0] == 16)
                return Decompress(Data.AsSpan());
            else if (Data[4] == 16)
                return Decompress(Data.AsSpan(4));
            else
                throw new InvalidIdentifierException(Magic);
        }

        public byte[] Decompress(ReadOnlySpan<byte> Data)
        {
            uint data = (uint)(Data[1] | Data[2] << 8 | Data[3] << 16);
            byte[] numArray = new byte[data];

            if (data == 0)
                return numArray;

            MemoryStream ms = new MemoryStream();
            int num = 4;
            int num1 = 0;
            while (true)
            {
                int num2 = num;
                num = num2 + 1;
                byte data1 = Data[num2];
                for (int i = 0; i < 8; i++)
                {
                    if ((data1 & 128) != 0)
                    {
                        byte data2 = Data[num++];
                        byte data3 = Data[num++];
                        int num5 = ((data2 & 15) << 8 | data3) + 1;
                        int num6 = (data2 >> 4) + 3;
                        for (int j = 0; j < num6; j++)
                        {
                            numArray[num1] = numArray[num1 - num5];
                            num1++;
                        }
                    }
                    else
                    {
                        numArray[num1++] = Data[num++];
                    }
                    if (num1 >= data)
                    {
                        ms.Write(numArray);

                        //has chunks?
                        if (Data.Length > num)
                        {
                            //Padding
                            while (Data.Length - 1 > num && Data[num] == 0)
                                num++;

                            //new chunk?
                            if (Data[num++] == 16)
                            {
                                num1 = 0;
                                data = (uint)(Data[num++] | Data[num++] << 8 | Data[num++] << 16);

                                numArray = new byte[data];
                                break;
                            }

                            if (Data.Length > num)
                            {
                                num--;
                                Events.NotificationEvent?.Invoke(NotificationType.Info, $"{typeof(LZ77)} file steam contains {Data.Length - num} unread bytes, starting at position {num}.");

                                ms.Write(Data.Slice(num-1).ToArray());
                            }
                        }
                        return ms.ToArray();
                    }
                    data1 = (byte)(data1 << 1);
                }
            }
        }

    }
}

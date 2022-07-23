using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class LZ77 : ICompression
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public bool IsMatch(Stream stream, in string extension = "")
        {
            return stream.Length > 16 && stream.ReadByte() == 16;
        }

        public byte[] Compress(in byte[] Data)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(in byte[] Data)
        {
            uint data = (uint)(Data[1] | Data[2] << 8 | Data[3] << 16);
            byte[] numArray = new byte[data];
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
                        int num3 = num;
                        num = num3 + 1;
                        byte data2 = Data[num3];
                        int num4 = num;
                        num = num4 + 1;
                        byte data3 = Data[num4];
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
                        int num7 = num1;
                        num1 = num7 + 1;
                        int num8 = num;
                        num = num8 + 1;
                        numArray[num7] = Data[num8];
                    }
                    if (num1 >= data)
                    {
                        return numArray;
                    }
                    data1 = (byte)(data1 << 1);
                }
            }
        }
    }
}

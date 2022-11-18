using System;
using System.Collections.Generic;
using System.IO;
using AuroraLip.Common;

namespace AuroraLip.Compression.Formats
{

    /*
     * xdanieldzd & lue
     * Library for Decompressing LZSS files
     * https://github.com/xdanieldzd/N3DSCmbViewer/blob/master/N3DSCmbViewer/LZSS.cs
     * https://github.com/lue/MM3D/blob/master/src/lzs.cpp
     */

    /// <summary>
    /// LZSS Lempel–Ziv–Storer–Szymanski algorithm, a derivative of LZ77.
    /// </summary>
    public class LZSS : ICompression, IMagicIdentify
    {

        public string Magic { get; } = "LzS";

        public int MagicOffset { get; } = 0;

        public bool CanWrite { get; } = false;

        public bool CanRead { get; } = true;

        public const short WINDOW_START = 958;

        public const short WINDOW_SIZE = 1024;

        public byte[] Compress(in byte[] Data)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(in byte[] Data)
        {
            uint decompressedSize;

            if (IsMatch(in Data))
            {
                //string tag = Encoding.ASCII.GetString(Data, 0, 4);
                //uint unknown = BitConverter.ToUInt32(Data, 4);
                decompressedSize = BitConverter.ToUInt32(Data, 8);
                uint compressedSize = BitConverter.ToUInt32(Data, 12);
                if (Data.Length != compressedSize + 0x10) throw new Exception("compressed size mismatch");
            }
            else
            {
                decompressedSize = BitConverter.ToUInt32(Data, 0);
            }

            List<byte> outdata = new List<byte>();
            byte[] window_buffer = new byte[4096];

            for (int i = 0; i < window_buffer.Length; i++) window_buffer[i] = 0;
            byte code_word = 0;
            ushort writeidx = 4078;
            ushort window_offset = 0;
            uint fidx = 0x10;
            if (!IsMatch(in Data)) fidx = 4;

            while (fidx < Data.Length)
            {
                code_word = Data[fidx++];

                for (int i = 0; i < 8; i++)
                {
                    if ((code_word & 1) != 0)
                    {
                        outdata.Add(Data[fidx]);
                        window_buffer[writeidx++] = Data[fidx++];
                        writeidx %= 4096;
                    }
                    else
                    {
                        window_offset = Data[fidx++];
                        window_offset |= (ushort)((Data[fidx] & 0xF0) << 4);
                        for (int j = 0; j < (Data[fidx] & 0x0F) + 3; j++)
                        {
                            outdata.Add(window_buffer[window_offset]);
                            window_buffer[writeidx++] = window_buffer[window_offset++];
                            window_offset %= 4096;
                            writeidx %= 4096;
                        }
                        fidx++;
                    }
                    code_word >>= 1;
                    if (fidx >= Data.Length) break;
                }
            }

            if (decompressedSize != outdata.Count)
                throw new Exception($"Size mismatch: got {outdata.Count} bytes after decompression, expected {decompressedSize}.\n");

            return outdata.ToArray();
        }

        /// <summary>
        /// simple LZSS algorithm used in mario party games.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="decompressed_size"></param>
        /// <returns></returns>
        //Base: https://github.com/gamemasterplc/mpbintools/blob/master/bindump.c#L170
        public static byte[] Decompress(Stream stream, int decompressed_size)
        {
            byte[] window_buffer = new byte[WINDOW_SIZE];
            byte[] decompress_buffer = new byte[decompressed_size];
            int window_offset = WINDOW_START;
            int code_word = 0, dest_offset = 0;

            while (dest_offset < decompressed_size)
            {
                //Reads New Code Word from Compressed Stream if Expired
                if ((code_word & 0x100) == 0)
                {
                    code_word = stream.ReadUInt8();
                    code_word |= 0xFF00;
                }

                //Copies a Byte from the Source to the Destination and Window Buffer
                if ((code_word & 0x1) != 0)
                {
                    window_buffer[window_offset++] = decompress_buffer[dest_offset++] = stream.ReadUInt8();
                    window_offset %= WINDOW_SIZE;
                }
                else
                {
                    //Interpret Next 2 Bytes as an Offset and Length into the Window Buffer
                    byte byte1 = stream.ReadUInt8();
                    byte byte2 = stream.ReadUInt8();

                    int offset = ((byte2 & 0xC0) << 2) | byte1;
                    int copy_length = (byte2 & 0x3F) + 3;

                    //Copy Some Bytes from Window Buffer
                    for (int i = 0; i < copy_length; i++)
                    {
                        window_buffer[window_offset++] = decompress_buffer[dest_offset++] = window_buffer[offset++ % WINDOW_SIZE];
                        window_offset %= WINDOW_SIZE;
                    }
                }
                code_word >>= 1;
            }
            return decompress_buffer;
        }

        private bool IsMatch(in byte[] Data)
        {
            // is LzS
            return Data.Length > 16 && Data[0] == 76 && Data[1] == 122 && Data[2] == 83;
        }

        public bool IsMatch(Stream stream, in string extension = "")
        {
            if (stream.Length < 16 && stream.MatchString(Magic))
{
                stream.Position = 12;
                // compressed size match?
                uint compressedSize = BitConverter.ToUInt32(stream.Read(4), 0);
                return stream.Length == compressedSize + 0x10;

            }
            return false;

        }
    }
}

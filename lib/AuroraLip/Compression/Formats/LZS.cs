using AuroraLib.Common;
using AuroraLib.Common.Struct;

namespace AuroraLib.Compression.Formats
{
    /*
     * xdanieldzd & lue
     * Library for Decompressing LZSS files
     * https://github.com/xdanieldzd/N3DSCmbViewer/blob/master/N3DSCmbViewer/LZSS.cs
     * https://github.com/lue/MM3D/blob/master/src/lzs.cpp
     */

    /// <summary>
    /// LZS Lempel–Ziv–Stac algorithm, is based on LZSS.
    /// </summary>
    public class LZS : ICompression, IHasIdentifier
    {
        public bool CanWrite { get; } = false;

        public bool CanRead { get; } = true;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(new byte[] { (byte)'L', (byte)'z', (byte)'S', 0 });

        public void Compress(in byte[] source, Stream destination)
        {
            throw new NotImplementedException();
        }

        public byte[] Decompress(Stream source)
        {
            return Decompress(source.ToArray());
        }

        private byte[] Decompress(in byte[] Data)
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
            byte flags = 0;
            ushort writeidx = 4078;
            ushort window_offset = 0;
            uint fidx = 0x10;
            if (!IsMatch(in Data)) fidx = 4;

            while (fidx < Data.Length)
            {
                flags = Data[fidx++];

                for (int i = 0; i < 8; i++)
                {
                    if ((flags & 1) != 0)
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
                    flags >>= 1;
                    if (fidx >= Data.Length) break;
                }
            }

            if (decompressedSize != outdata.Count)
                throw new Exception($"Size mismatch: got {outdata.Count} bytes after decompression, expected {decompressedSize}.\n");

            return outdata.ToArray();
        }

        private bool IsMatch(in byte[] Data)
        {
            // is LzS
            return Data.Length > 16 && Data[0] == 76 && Data[1] == 122 && Data[2] == 83;
        }

        public bool IsMatch(Stream stream, in string extension = "")
        {
            if (stream.Length < 16 && stream.Match(_identifier))
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

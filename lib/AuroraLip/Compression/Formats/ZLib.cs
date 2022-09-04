using AuroraLip.Common;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.IO;

namespace AuroraLip.Compression.Formats
{
    /// <summary>
    /// ZLib, based on the DEFLATE compression algorithm.
    /// </summary>
    public class ZLib : ICompression, ICompressionLevel
    {
        public bool CanWrite { get; } = true;

        public bool CanRead { get; } = true;

        /// <summary>
        /// Gets the adler checksum. This is the checksum of all uncompressed bytes returned by Decompress() or Compress()
        /// </summary>
        public int Adler { get; private set; } = 0;

        public byte[] Decompress(in byte[] Data)
            => Decompress(Data, 4096).ToArray();

        public MemoryStream Decompress(in byte[] Data, int bufferSize = 4096, bool noHeader = false)
        {
            MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[bufferSize];

            Inflater inflater = new Inflater(noHeader);
            inflater.SetInput(Data);
            while (!inflater.IsFinished)
            {
                if (inflater.IsNeedingDictionary)
                {
                    throw new Exception("Need a dictionary");
                }
                if (inflater.IsNeedingInput)
                {
                    throw new Exception("Need more Input");
                }
                inflater.Inflate(buffer);
                ms.Write(buffer, 0, buffer.Length);
            }
            Adler = inflater.Adler;
            inflater.Reset();
            return ms;
        }
        public byte[] Compress(in byte[] Data)
            => Compress(Data, CompressionLevel.Optimal, 4096).ToArray();

        public byte[] Compress(byte[] Data, CompressionLevel level)
            => Compress(Data, level, 4096).ToArray();

        public MemoryStream Compress(byte[] Data, CompressionLevel level, int bufferSize = 4096, bool noHeader = false)
        {
            MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[bufferSize];

            int dlevel = -1;
            switch (level)
            {
                case CompressionLevel.NoCompression:
                    dlevel = 0;
                    break;
                case CompressionLevel.Optimal:
                    dlevel = -1;
                    break;
                case CompressionLevel.Fastest:
                    dlevel = 1;
                    break;
                case CompressionLevel.SmallestSize:
                    dlevel = 9;
                    break;
            }
            Deflater deflater = new Deflater(dlevel, noHeader);
            deflater.SetInput(Data);

            while (!deflater.IsFinished)
            {
                deflater.Deflate(buffer);
                ms.Write(buffer, 0, buffer.Length);
            }
            Adler = deflater.Adler;
            deflater.Reset();
            return ms;
        }

        private bool IsMatch(in byte[] Data)
        {
            byte num = (byte)(Data[0] & 15);
            float single = Data[0] << 8 | Data[1];

            return ((Data[0] & 15) != 8 || (Data[0] >> 4 & 15) <= 7) && single / 31f == (float)(single / 31) && num != 15;
        }

        public bool IsMatch(Stream stream, in string extension = "")
        {
            return IsMatch(stream.Read(2));
        }

    }
}

using AuroraLib.Common;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace AuroraLib.Compression.Formats
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

        public byte[] Decompress(Stream source)
        {
            return Decompress(source.ToArray(), 4096).ToArray();
        }

        public MemoryStream Decompress(in byte[] Data, int bufferSize = 4096, bool noHeader = false)
        {
            MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[bufferSize];

            Inflater inflater = new Inflater(noHeader);
            if (IsMatch(Data))
                inflater.SetInput(Data);
            else
                inflater.SetInput(Data.AsSpan().Slice(4).ToArray());

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
                int i = inflater.Inflate(buffer);
                ms.Write(buffer, 0, i);
            }
            Adler = inflater.Adler;
            if (inflater.RemainingInput != 0)
            {
                Events.NotificationEvent?.Invoke(NotificationType.Info, $"{typeof(ZLib)} file contains {inflater.RemainingInput} unread bytes.");
            }

            inflater.Reset();
            return ms;
        }

        public void Compress(in byte[] source, Stream destination)
            => destination.Write(Compress(source, CompressionLevel.Optimal, 4096).ToArray());

        public byte[] Compress(byte[] Data, CompressionLevel level)
        {
            return Compress(Data, level, 4096).ToArray();
        }

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

        private static bool IsMatch(in byte[] Data)
        {
            Header header = new(Data[0], Data[1]);
            return header.Validate();
        }

        public static bool Matcher(Stream stream, in string extension = "")
            => extension.ToLower() == "zlib" && (IsMatch(stream.Read(4)) || IsMatch(stream.Read(4)));

        public bool IsMatch(Stream stream, in string extension = "")
            => IsMatch(stream.Read(4)) || IsMatch(stream.Read(4));

        public struct Header
        {
            private byte cmf;
            private byte flg;

            public Header(byte cmf, byte flg)
            {
                this.cmf = cmf;
                this.flg = flg;
            }

            public enum CompressionMethod : byte
            {
                Deflate = 8
            }

            public CompressionMethod Method => (CompressionMethod)(cmf & 0x0F);

            public byte CompressionInfo => (byte)((cmf >> 4) & 0x0F);

            public ushort FletcherChecksum => (ushort)(((flg & 0xFF) << 8) | cmf);

            public bool HasDictionary => ((flg >> 5) & 0x01) != 0;

            public byte CompressionLevel => (byte)((flg >> 6) & 0x03);

            public bool Validate()
            {
                ushort checksum = FletcherChecksum;

                if (Method != CompressionMethod.Deflate || CompressionInfo < 7 || CompressionInfo > 15 || CompressionLevel > 9)
                    return false;

                return checksum % 31 != 0 || checksum % 255 != 0;
            }
        }


    }
}

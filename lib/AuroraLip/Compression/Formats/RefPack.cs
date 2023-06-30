using AuroraLib.Common;
using AuroraLib.Compression;

namespace AuroraLib.Compression.Formats
{
    /// <summary>
    /// RefPack is an LZ77/LZSS compression format made by Frank Barchard of EA Canada
    /// </summary>
    public class RefPack : ICompression
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public byte[] Decompress(Stream source)
        {
            Header header = new(source);
            return Decompress_ALG(source, header.UncompressedSize);
        }

        public void Compress(in byte[] source, Stream destination) => throw new NotImplementedException();

        public bool IsMatch(Stream stream, in string extension = "")
        {
            if (stream.Length < 0x10)
                return false;

            Header header = new(stream);
            return header.IsValid;
        }

        public static byte[] Decompress_ALG(Stream source, uint decomLength)
        {
            byte[] data = new byte[decomLength];
            uint offset = 0;
            while (true)
            {
                bool stop = false;
                uint plainSize;
                uint copySize = 0u;
                uint copyOffset = 0u;

                byte prefix = source.ReadUInt8();

                if (prefix < 0x80)
                {
                    byte data0 = source.ReadUInt8();

                    plainSize = (uint)(prefix & 0x03);
                    copySize = (uint)(((prefix & 0x1C) >> 2) + 3);
                    copyOffset = (uint)((((prefix & 0x60) << 3) | data0) + 1);
                }
                else if (prefix < 0xC0)
                {
                    byte data0 = source.ReadUInt8();
                    byte data1 = source.ReadUInt8();

                    plainSize = (uint)(data0 >> 6);
                    copySize = (uint)((prefix & 0x3F) + 4);
                    copyOffset = (uint)((((data0 & 0x3F) << 8) | data1) + 1);
                }
                else if (prefix < 0xE0)
                {
                    byte data0 = source.ReadUInt8();
                    byte data1 = source.ReadUInt8();
                    byte data2 = source.ReadUInt8();

                    plainSize = (uint)(prefix & 3);
                    copySize = (uint)((((prefix & 0x0C) << 6) | data2) + 5);
                    copyOffset = (uint)((((((prefix & 0x10) << 4) | data0) << 8) | data1) + 1);
                }
                else if (prefix < 0xFC)
                {
                    plainSize = (uint)(((prefix & 0x1F) + 1) * 4);
                }
                else
                {
                    plainSize = (uint)(prefix & 3);
                    stop = true;
                }

                if (plainSize > 0)
                {
                    if (source.Read(data, (int)offset, (int)plainSize) != (int)plainSize)
                    {
                        throw new EndOfStreamException("could not read data");
                    }

                    offset += plainSize;
                }

                if (copySize > 0)
                {
                    for (uint i = 0; i < copySize; i++)
                    {
                        data[offset + i] = data[(offset - copyOffset) + i];
                    }

                    offset += copySize;
                }

                if (stop)
                {
                    return data;
                }
            }
        }

        public struct Header
        {
            public uint UncompressedSize;
            public uint UncompressedSize2;

            public bool IsValid => UncompressedSize != 0;
            public bool HasMore => UncompressedSize2 != 0;
            public bool IsLong => ((UncompressedSize2 | UncompressedSize) & 0xFF000000) != 0;

            public Header(Stream stream)
            {
                UncompressedSize = UncompressedSize2 = 0;

                Span<byte> Header = stackalloc byte[2];
                stream.Read(Header);

                if ((Header[0] & 0x3E) != 0x10 || (Header[1] != 0xFB))
                {
                    return;
                }
                bool IsLong = ((Header[0] & 0x80) != 0);
                bool hasMore = ((Header[0] & 0x01) != 0);

                if (IsLong)
                {
                    UncompressedSize = stream.ReadUInt32(Endian.Big);
                    if (hasMore)
                    {
                        UncompressedSize2 = stream.ReadUInt32(Endian.Big);
                    }
                }
                else
                {
                    UncompressedSize = (uint)stream.ReadUInt24(Endian.Big);
                    if (hasMore)
                    {
                        UncompressedSize2 = (uint)stream.ReadUInt24(Endian.Big);
                    }
                }
            }
        }

    }
}

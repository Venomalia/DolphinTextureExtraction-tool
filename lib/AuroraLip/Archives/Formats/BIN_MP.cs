using AuroraLib.Common;
using AuroraLib.Compression;
using AuroraLib.Compression.Algorithms;

namespace AuroraLib.Archives.Formats
{
    // base on https://github.com/gamemasterplc/mpbintools/blob/master/bindump.c
    public class BIN_MP : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".bin";

        private static readonly LzProperties _Lz = new((byte)10, 6, 2);

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase))
            {
                uint files = stream.ReadUInt32(Endian.Big);
                if (files > 1000 || files == 0)
                    return false;
                uint[] offsets = new uint[files];
                for (int i = 0; i < files; i++)
                {
                    offsets[i] = stream.ReadUInt32(Endian.Big);
                }

                if (offsets[0] == stream.Position)
                {

                    uint lastoffset = (uint)stream.Position - 1;
                    for (int i = 0; i < files; i++)
                    {
                        if (offsets[i] <= lastoffset || stream.Length < offsets[i] + 10)
                            return false;
                        lastoffset = offsets[i] + 0x20;

                        stream.Seek(offsets[i], SeekOrigin.Begin);
                        uint DeSize = stream.ReadUInt32(Endian.Big);
                        uint type = stream.ReadUInt32(Endian.Big);

                        if (!Enum.IsDefined(typeof(CompressionType), type) || DeSize > 0xA00000 || DeSize == 0)
                            return false;
                    }

                    return true;
                }
            }

            return false;
        }

        protected override void Read(Stream stream)
        {
            uint files = stream.ReadUInt32(Endian.Big);
            uint[] offsets = new uint[files];
            for (int i = 0; i < files; i++)
            {
                offsets[i] = stream.ReadUInt32(Endian.Big);
            }

            ZLib zLib = new();
            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < files; i++)
            {
                stream.Seek(offsets[i], SeekOrigin.Begin);
                uint DeSize = stream.ReadUInt32(Endian.Big);
                CompressionType type = (CompressionType)stream.ReadUInt32(Endian.Big);

                long compressed = 0;
                if (i == offsets.Length - 1)
                {
                    compressed = stream.Length - offsets[i] - 8;
                }
                else
                {
                    compressed = offsets[i + 1] - offsets[i];
                }

                Stream DeStream;
                switch (type)
                {
                    case CompressionType.NONE:
                        DeStream = new SubStream(stream, DeSize);
                        break;

                    case CompressionType.LZSS:
                        DeStream = new MemoryPoolStream((int)DeSize);
                        LZSS.DecompressHeaderless(stream, DeStream, (int)DeSize, _Lz);
                        break;

                    case CompressionType.SLIDE:
                    case CompressionType.FSLIDE_ALT:
                    case CompressionType.FSLIDE:
                        uint temp_len = stream.ReadUInt32(Endian.Big);
                        DeStream = new MemoryPoolStream((int)DeSize);
                        LZHudson.DecompressHeaderless(stream, DeStream, (int)DeSize);
                        break;

                    case CompressionType.RLE:
                        Root.AddArchiveFile(stream, compressed, $"{i}_{type}.cmpres");
                        continue;
                    case CompressionType.INFLATE:
                        uint decompressed_size = stream.ReadUInt32(Endian.Big);
                        uint compressed_size = stream.ReadUInt32(Endian.Big);
                        DeStream = new MemoryPoolStream();
                        DeStream.SetLength((int)DeSize);
                        zLib.Decompress(stream, DeStream, (int)compressed_size);
                        break;

                    default:
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(CompressionType)}, unknown Compression type {type}.");
                        DeStream = new SubStream(stream, DeSize);
                        break;
                }

                DeStream.Seek(0, SeekOrigin.Begin);
                if (DeStream.ReadString(4) == "HSFV")
                {
                    Root.AddArchiveFile(DeStream, $"{i}_{type}.hsf");
                }
                else
                {
                    DeStream.Seek(12, SeekOrigin.Begin);
                    if (DeStream.ReadUInt32(Endian.Big) == 20)
                    {
                        Root.AddArchiveFile(DeStream, $"{i}_{type}.atb");
                    }
                    else
                    {
                        Root.AddArchiveFile(DeStream, $"{i}_{type}.bin");
                    }
                }
                DeStream.Seek(0, SeekOrigin.Begin);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        public enum CompressionType : uint
        {
            NONE = 0,
            LZSS = 1,
            SLIDE = 2,
            FSLIDE_ALT = 3,
            FSLIDE = 4,
            RLE = 5,
            INFLATE = 7,
        }
    }
}

using AuroraLip.Common;
using AuroraLip.Compression;
using AuroraLip.Compression.Formats;
using System;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    // base on https://github.com/gamemasterplc/mpbintools/blob/master/bindump.c
    public class BIN_MP : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".bin";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (!extension.ToLower().Equals(Extension))
                return false;

            uint files = stream.ReadUInt32(Endian.Big);
            if (files > 1000)
                return false;
            uint[] offsets = new uint[files];
            for (int i = 0; i < files; i++)
            {
                offsets[i] = stream.ReadUInt32(Endian.Big);
            }

            uint lastoffset = 0;
            for (int i = 0; i < files; i++)
            {
                if (offsets[i] < lastoffset || stream.Length < offsets[i] + 10)
                    return false;
                lastoffset = offsets[i];

                stream.Seek(offsets[i] + 4, SeekOrigin.Begin);
                uint type = stream.ReadUInt32(Endian.Big);

                if (!Enum.IsDefined(typeof(CompressionType), type))
                    return false;
            }

            return true;
        }

        protected override void Read(Stream stream)
        {
            uint files = stream.ReadUInt32(Endian.Big);
            uint[] offsets = new uint[files];
            for (int i = 0; i < files; i++)
            {
                offsets[i] = stream.ReadUInt32(Endian.Big);
            }

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
                    case CompressionType.SLIDE:
                    case CompressionType.FSLIDE_ALT:
                    case CompressionType.FSLIDE:
                    case CompressionType.RLE:
                        Root.AddArchiveFile(stream, compressed, $"{i}_{type}.cmpres");
                        continue;
                    case CompressionType.INFLATE:
                        uint decompressed_size = stream.ReadUInt32(Endian.Big);
                        uint compressed_size = stream.ReadUInt32(Endian.Big);
                        DeStream = new MemoryStream(Compression<ZLib>.Decompress(stream.Read((int)compressed_size)));
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

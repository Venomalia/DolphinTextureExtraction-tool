using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Compression;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.Buffers;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Hudson Soft Mario Party Archive
    /// </summary>
    // base on https://github.com/gamemasterplc/mpbintools/blob/master/bindump.c
    public sealed class BIN_MP : ArchiveNode
    {
        public override bool CanWrite => false;

        private static readonly LzProperties _Lz = new((byte)10, 6, 2);

        public BIN_MP()
        {
        }

        public BIN_MP(string name) : base(name)
        {
        }

        public BIN_MP(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (extension.Contains(".bin", StringComparison.InvariantCultureIgnoreCase))
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

        protected override void Deserialize(Stream source)
        {
            uint files = source.ReadUInt32(Endian.Big);
            using SpanBuffer<uint> offsets = new (files);
            source.Read<uint>(offsets,Endian.Big);

            ZLib zLib = new();
            for (int i = 0; i < files; i++)
            {
                source.Seek(offsets[i], SeekOrigin.Begin);
                uint DeSize = source.ReadUInt32(Endian.Big);
                CompressionType type = (CompressionType)source.ReadUInt32(Endian.Big);

                long compressed = 0;
                if (i == offsets.Length - 1)
                {
                    compressed = source.Length - offsets[i] - 8;
                }
                else
                {
                    compressed = offsets[i + 1] - offsets[i];
                }

                Stream DeStream;
                switch (type)
                {
                    case CompressionType.NONE:
                        DeStream = new SubStream(source, DeSize);
                        break;

                    case CompressionType.LZSS:
                        DeStream = new MemoryPoolStream((int)DeSize);
                        LZSS.DecompressHeaderless(source, DeStream, (int)DeSize, _Lz);
                        break;

                    case CompressionType.SLIDE:
                    case CompressionType.FSLIDE_ALT:
                    case CompressionType.FSLIDE:
                        uint temp_len = source.ReadUInt32(Endian.Big);
                        DeStream = new MemoryPoolStream((int)DeSize);
                        LZHudson.DecompressHeaderless(source, DeStream, (int)DeSize);
                        break;

                    case CompressionType.RLE:
                        Add(new FileNode($"{i}_{type}.cmpres", new SubStream(source, compressed)));
                        continue;
                    case CompressionType.INFLATE:
                        uint decompressed_size = source.ReadUInt32(Endian.Big);
                        uint compressed_size = source.ReadUInt32(Endian.Big);
                        DeStream = new MemoryPoolStream();
                        DeStream.SetLength((int)DeSize);
                        zLib.Decompress(source, DeStream, (int)compressed_size);
                        break;

                    default:
                        Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(CompressionType)}, unknown Compression type {type}.");
                        DeStream = new SubStream(source, DeSize);
                        break;
                }

                DeStream.Seek(0, SeekOrigin.Begin);
                if (DeStream.ReadString(4) == "HSFV")
                {
                    Add(new FileNode($"{i}_{type}.hsf", new SubStream(DeStream)));
                }
                else
                {
                    DeStream.Seek(12, SeekOrigin.Begin);
                    if (DeStream.ReadUInt32(Endian.Big) == 20)
                    {
                        Add(new FileNode($"{i}_{type}.atb", new SubStream(DeStream)));
                    }
                    else
                    {
                        Add(new FileNode($"{i}_{type}.bin", new SubStream(DeStream)));
                    }
                }
                DeStream.Seek(0, SeekOrigin.Begin);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

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

using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.IO;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Eighting FPK Archive
    /// </summary>
    public sealed class FPK : ArchiveNode
    {
        public override bool CanWrite => false;

        public FPK()
        {
        }

        public FPK(string name) : base(name)
        {
        }

        public FPK(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(".fpk", StringComparison.InvariantCultureIgnoreCase);

        protected override void Deserialize(Stream source)
        {
            Endian order = source.DetectByteOrder<uint>(4);
            int nameLength = source.At(0x2f, s => s.ReadUInt8()) == 0 ? 32 : 16; // GC 16 Wii 32

            Header header = source.Read<Header>(order);
            Entry[] entries = source.For((int)header.Entrys, s => new Entry(s, order, nameLength));

            for (int i = 0; i < entries.Length; i++)
            {
                Stream data = new SubStream(source, entries[i].CompressedSize, entries[i].Offset);
                FileNode file = new(Path.GetFileName(entries[i].Name), data);
                if (entries[i].CompressedSize != entries[i].UncompressedSize)
                {
                    //Use PRS BigEndian
                    MemoryPoolStream decom = new((int)entries[i].UncompressedSize);
                    PRS.DecompressHeaderless(file.Data, decom, Endian.Big);
                    file.Data.Dispose();
                    file.Data = decom;
                    file.Properties = "PRS";
                }
                AddPath(entries[i].Name, file);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private struct Header
        {
            public uint Hash;
            public uint Entrys;
            public uint Offset;
            public uint Size;
        }

        private struct Entry
        {
            public string Name;
            public uint Unk;
            public uint Offset;
            public uint CompressedSize;
            public uint UncompressedSize;

            public Entry(Stream stream, Endian order, int nameLength)
            {
                Name = stream.ReadString(nameLength);
                Unk = stream.ReadUInt32(order);
                Offset = stream.ReadUInt32(order);
                CompressedSize = stream.ReadUInt32(order);
                UncompressedSize = stream.ReadUInt32(order);
            }
        }
    }
}

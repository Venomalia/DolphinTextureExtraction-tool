using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Aqualead Archive
    /// </summary>
    // base on https://zenhax.com/viewtopic.php?t=16613
    // Todo: https://web.archive.org/web/20160811181703/http://fw.aqualead.co.jp/Document/Aqualead/Tool/ALMakeArc.html
    public sealed class ALAR : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("ALAR");

        public ALAR()
        {
        }

        public ALAR(string name) : base(name)
        {
        }

        public ALAR(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            byte version = source.ReadUInt8();
            EntryFlags entryFlags = source.Read<EntryFlags>();
            ushort entries = source.ReadUInt16(Endian.Big);
            uint lowId = source.ReadUInt32(Endian.Big);
            uint highId = source.ReadUInt32(Endian.Big);

            uint id, offset, size, pad;
            string path, name, lixo;
            switch (version)
            {
                case 2:
                    for (int i = 0; i < entries; i++)
                    {
                        id = source.ReadUInt32(Endian.Big);
                        offset = source.ReadUInt32(Endian.Big);
                        size = source.ReadUInt32(Endian.Big);
                        pad = source.ReadUInt32(Endian.Big);

                        long pos = source.Position;
                        source.Seek(offset - 0x22, SeekOrigin.Begin);
                        path = source.ReadString(0x20);
                        name = Path.GetFileName(path);

                        Stream data = new SubStream(source, size, offset);
                        AddPath(path, new FileNode(name, data) { ID = id });
                        source.Seek(pos, SeekOrigin.Begin);
                    }
                    break;

                case 3:
                    ushort dataTabelOffset = source.ReadUInt16(Endian.Big);
                    SpanBuffer<ushort> entrieOffsets = new(entries);
                    source.Read<ushort>(entrieOffsets, Endian.Big);
                    foreach (ushort entrieOffset in entrieOffsets)
                    {
                        source.Seek(entrieOffset, SeekOrigin.Begin);
                        id = source.ReadUInt32(Endian.Big);
                        offset = source.ReadUInt32(Endian.Big);
                        size = source.ReadUInt32(Endian.Big);
                        lixo = source.ReadString(6);
                        path = source.ReadCString();
                        name = Path.GetFileName(path);

                        Stream data = new SubStream(source, size, offset);
                        AddPath(path, new FileNode(name, data) { ID = id });
                    }
                    entrieOffsets.Dispose();
                    break;

                default:
                    throw new Exception($"{nameof(ALAR)} unknown version:{version}");
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        [Flags]
        public enum EntryFlags : byte
        {
            IsResident = 1,
            IsPrepare = 2,
            Unknown = 32,
            Unknown2 = 64,
            HasName = 128
        }
    }
}

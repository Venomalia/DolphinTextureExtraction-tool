using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Imageepoch Arc Rise Archive
    /// </summary>
    public sealed class RTDP : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("RTDP");

        public RTDP()
        {
        }

        public RTDP(string name) : base(name)
        {
        }

        public RTDP(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            int EOH = (int)source.ReadUInt32(Endian.Big);
            int NrEntries = (int)source.ReadUInt32(Endian.Big);
            int Size = (int)source.ReadUInt32(Endian.Big);
            source.Position = 0x20;

            List<RTDPEntry> Entries = new(NrEntries);
            for (int i = 0; i < NrEntries; i++)
            {
                Entries.Add(new RTDPEntry(source));
            }

            foreach (var Entry in Entries)
            {
                SubStream subStream = new(source, Entry.DataSize, Entry.DataOffset + EOH);
                XORStream xORStream = new(subStream, 0x55);
                FileNode file = new(Entry.Name, xORStream);
                //If Duplicate...
                if (Contains(file))
                    file.Name = Path.GetFileName(Entry.Name) + Entries.IndexOf(Entry) + Path.GetExtension(Entry.Name);

                Add(file);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        private class RTDPEntry
        {
            public string Name { get; set; }
            public int DataSize { get; set; }
            public int DataOffset { get; set; }

            public RTDPEntry(Stream stream)
            {
                Name = stream.ReadString(32);
                DataSize = (int)stream.ReadUInt32(Endian.Big);
                DataOffset = (int)stream.ReadUInt32(Endian.Big);
            }
        }
    }
}

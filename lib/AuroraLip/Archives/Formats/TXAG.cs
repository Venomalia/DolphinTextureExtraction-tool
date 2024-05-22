using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    public sealed class TXAG : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("TXAG");

        public TXAG()
        {
        }

        public TXAG(string name) : base(name)
        {
        }

        public TXAG(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            ushort unk = source.ReadUInt16(Endian.Big);
            ushort fileCount = source.ReadUInt16(Endian.Big);

            for (int i = 0; i < fileCount; i++)
            {
                uint offset = source.ReadUInt32(Endian.Big);
                uint length = source.ReadUInt32(Endian.Big);
                string fileName = source.ReadString(32);

                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = $"entry{i}.GVR";

                FileNode file = new(fileName, new SubStream(source, length, offset));
                Add(file);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

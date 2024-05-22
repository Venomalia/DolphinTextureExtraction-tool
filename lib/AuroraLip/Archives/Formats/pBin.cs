using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Natsume Harvest Moon: Animal Parade Archive
    /// </summary>
    public sealed class PBin : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("pBin");

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            uint unknown1 = source.ReadUInt32(Endian.Big);
            uint unknown2 = source.ReadUInt32(Endian.Big);
            uint unknown3 = source.ReadUInt32(Endian.Big);
            uint unknown4 = source.ReadUInt32(Endian.Big);
            uint count = source.ReadUInt32(Endian.Big);

            for (int i = 0; i < count; i++)
            {
                uint size = source.ReadUInt32(Endian.Big);
                uint offset = source.ReadUInt32(Endian.Big);
                string type = source.ReadString(4);
                uint unknown = source.ReadUInt32(Endian.Big);
                FileNode file = new($"Entry{i}_{type}", new SubStream(source, size, offset));
                Add(file);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

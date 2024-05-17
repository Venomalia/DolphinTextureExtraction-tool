using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Brawl ARC0 Archive
    /// </summary>
    // ref https://github.com/soopercool101/BrawlCrate/blob/a0e5638c34bba0de783ece169d483ad7e7dcb016/BrawlLib/SSBB/ResourceNodes/Archives/ARCNode.cs
    public sealed class ARC0 : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new((byte)'A', (byte)'R', (byte)'C', 0);

        public ARC0()
        {
        }

        public ARC0(string name) : base(name)
        {
        }

        public ARC0(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier) && stream.ReadByte() == 0;

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            ushort version = source.ReadUInt16(Endian.Big); //257
            ushort files = source.ReadUInt16(Endian.Big);
            ulong unk = source.ReadUInt64(Endian.Big);
            string name = source.ReadString(48);

            for (int i = 0; i < files; i++)
            {
                EntryType type = (EntryType)source.ReadUInt16(Endian.Big);
                ushort index = source.ReadUInt16(Endian.Big);
                uint size = source.ReadUInt32(Endian.Big);
                byte groupIndex = (byte)source.ReadByte();
                byte pad = (byte)source.ReadByte();
                short redirectIndex = source.ReadInt16(Endian.Big);

                uint[] padding = new uint[5];
                for (int p = 0; p < 5; p++)
                    padding[p] = source.ReadUInt32(Endian.Big);
                Add(new FileNode($"{type}_{i}.pcs", new SubStream(source, size)));

                source.Align(size, SeekOrigin.Current, 32);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        public enum EntryType : short
        {
            None = 0x0,
            MiscData = 0x1,
            ModelData = 0x2,
            TextureData = 0x3,
            AnimationData = 0x4,
            SceneData = 0x5,
            Type6 = 0x6,
            GroupedArchive = 0x7,
            EffectData = 0x8
        }
    }
}

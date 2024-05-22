using AuroraLib.Common.Node;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Archive use in the Sonic Storybook Series
    /// </summary>
    public sealed class ONE_SB : ArchiveNode
    {
        public override bool CanWrite => false;

        public int Version = -1;

        public ONE_SB()
        {
        }

        public ONE_SB(string name) : base(name)
        {
        }

        public ONE_SB(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(".one", StringComparison.InvariantCultureIgnoreCase) && stream.At(4, SeekOrigin.Begin, S => S.ReadUInt32(Endian.Big)) == 16;

        protected override void Deserialize(Stream source)
        {
            uint numEntries = source.ReadUInt32(Endian.Big);
            uint offset = source.ReadUInt32(Endian.Big); //16
            uint unk = source.ReadUInt32(Endian.Big);
            Version = source.ReadInt32(Endian.Big); // 0 Sonic and the Secret Rings or -1 for Sonic and the Black Knight

            source.Seek(offset, SeekOrigin.Begin);

            for (int i = 0; i < numEntries; i++)
            {
                string entryFilename = source.ReadString(32) + ".prs";

                uint entryIndex = source.ReadUInt32(Endian.Big);
                uint entryOffset = source.ReadUInt32(Endian.Big);
                uint entryLength = source.ReadUInt32(Endian.Big);
                uint entryUnk = source.ReadUInt32(Endian.Big);

                FileNode Sub = new(entryFilename, new SubStream(source, entryLength, entryOffset));
                Add(Sub);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

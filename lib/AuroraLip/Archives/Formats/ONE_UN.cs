using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Archive use in Sonic Unleashed
    /// </summary>
    public sealed class ONE_UN : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("one.");

        public ONE_UN()
        {
        }

        public ONE_UN(string name) : base(name)
        {
        }

        public ONE_UN(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier) && stream.At(4, S => stream.ReadUInt32()) <= 1024 * 4;

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            uint numEntries = source.ReadUInt32();
            for (int i = 0; i < numEntries; i++)
            {
                string entryFilename = source.ReadString(56);
                uint entryOffset = source.ReadUInt32();
                uint entryLength = source.ReadUInt32();

                FileNode Sub = new(entryFilename, new SubStream(source, entryLength, entryOffset));
                Add(Sub);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

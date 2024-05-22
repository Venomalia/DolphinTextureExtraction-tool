using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Cing Little King's Story Archive
    /// </summary>
    public sealed class PCKG : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("PCKG");

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        private const string Bres = "bresþÿ";

        public PCKG()
        {
        }

        public PCKG(string name) : base(name)
        {
        }

        public PCKG(FileNode source) : base(source)
        {
        }

        protected override void Deserialize(Stream source)
        {
            //PCKG_CING seem to contain only bres files
            while (source.Search(Bres))
            {
                long entrystart = source.Position;
                if (!source.Match(Bres))
                    continue;
                ushort Version = source.ReadUInt16(Endian.Big);
                uint TotalSize = source.ReadUInt32(Endian.Big);

                if (TotalSize > source.Length - entrystart)
                {
                    source.Search(Bres);
                    TotalSize = (uint)(source.Position - entrystart);
                }

                FileNode file = new($"entry_{Count + 1}.bres", new SubStream(source, TotalSize, entrystart));
                Add(file);

                source.Position = entrystart + TotalSize;
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

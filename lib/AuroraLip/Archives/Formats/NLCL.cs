using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Rune Factory (Tides) archive format
    /// </summary>
    public sealed class NLCL : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("NLCL");

        public NLCL()
        {
        }

        public NLCL(string name) : base(name)
        {
        }

        public NLCL(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            // This archive can have other things in it.  But it
            // isn't clear to me how the each file is sourced
            // there is a count but no offset...
            while (source.Search("HXTB"))
            {
                long entrystart = source.Position;
                if (!source.Match("HXTB"))
                    continue;
                source.Seek(0x14, SeekOrigin.Current);
                uint total_size = source.ReadUInt32(Endian.Big);

                if (total_size > source.Length - entrystart)
                {
                    source.Search("HXTB");
                    total_size = (uint)(source.Position - entrystart);
                }

                FileNode Sub = new($"entry_{Count + 1}.hxtb", new SubStream(source, total_size, entrystart));
                Add(Sub);

                source.Position = entrystart + total_size;
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

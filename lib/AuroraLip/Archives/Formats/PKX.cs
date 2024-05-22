using AuroraLib.Common.Node;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Genius Senority (Pokémon XD Gale of Darkness) PKX file (pokémon and some models?)
    /// </summary>
    public sealed class PKX : ArchiveNode
    {
        public override bool CanWrite => false;

        public PKX()
        {
        }

        public PKX(string name) : base(name)
        {
        }

        public PKX(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(".pkx", StringComparison.InvariantCultureIgnoreCase) && stream.At(0x1A, s => s.ReadUInt16(Endian.Big) == 0x0C);

        protected override void Deserialize(Stream source)
        {
            Header header = source.Read<Header>(Endian.Big);

            if (header.Unknown1A == 0x0C)
            {
                uint archive_begin = 0x84 + header.NumEntries * 208;
                archive_begin = (archive_begin + 31) & ~(uint)31; // Round to next 32-byte boundary
                archive_begin = (archive_begin + header.Unknown08 + 31) & ~(uint)31;

                FileNode Sub = new("thing.GSscene", new SubStream(source, header.ArchiveSize, archive_begin));
                Add(Sub);
            }
            else
            {
                throw new NotImplementedException($"Unknown header value {header.Unknown1A}");
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        public struct Header
        {
            public uint ArchiveSize;
            public uint Unknown04;
            public uint Unknown08;
            public uint Unknown0C;
            public uint NumEntries;
            public uint Unknown14;
            public ushort Unknown18;
            public ushort Unknown1A;
        }
    }
}

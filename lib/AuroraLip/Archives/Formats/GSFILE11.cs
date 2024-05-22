using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Genius Sonority "0x11" file format.
    /// Real file extension is unknown, number is from FSYS filetype field
    /// Based on Pokémon XD Gale of Darkness (GXXP01).
    /// </summary>
    public sealed class GSFILE11 : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(0x7B, 0x1E, 0xE3, 0xF2);

        public GSFILE11()
        {
        }

        public GSFILE11(string name) : base(name)
        {
        }

        public GSFILE11(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(".gsfile11", StringComparison.InvariantCultureIgnoreCase)
                && stream.At(0, s => s.ReadInt32(Endian.Big) == 0x7B1EE3F2);

        protected override void Deserialize(Stream stream)
        {
            Header header = stream.Read<Header>(Endian.Big);

            if (header.Magic != 0x7B1EE3F2)
            {
                // TODO: In GXXP01 T1_ancient_colo.fsys/02500200_T1_ancient_colo.GSFILE11 uses 0x7B1EE3F0, maybe check Colosseum code? XD just does nothing if magic doesn't match
                throw new Exception($"Magic does not match: {header.Magic:X8} (expected 7B1EE3F2)");
            }

            uint texture_entries_offset = 0x2C + 16u * header.NumEntries;

            for (uint i = 0; i < header.NumTextures; i++)
            {
                stream.Seek(texture_entries_offset + 8u * i, SeekOrigin.Begin);
                TextureEntry tex_entry = stream.Read<TextureEntry>(Endian.Big);
                FileNode file = new($"{tex_entry.TextureOffset:X8}.GTX", new SubStream(stream, stream.Length - tex_entry.TextureOffset, tex_entry.TextureOffset));
                Add(file);
                // TODO: FIlters are explicitly set to linear (and none for mipmaps), does this matter?
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        public struct Header
        {
            public uint Magic; // 0x7B1EE3F2
            public ushort NumTextures;
            public ushort NumEntries;
            public uint Unknown0C;
            public uint Unknown10;
            public uint Unknown14;
            public uint Unknown18;
            public uint Unknown1C;
            public uint Unknown20;
            public uint Unknown24;
            public uint Unknown28;
        }

        public struct TextureEntry
        {
            public ushort Index;
            public ushort Unknown02;
            public uint TextureOffset;
        }
    }
}

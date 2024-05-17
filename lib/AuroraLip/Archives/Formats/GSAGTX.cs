using AuroraLib.Common;
using AuroraLib.Common.Node;
using System.Xml.Linq;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Genius Sonority "GSAGTX" file format, although the real file extension is unknown.
    /// It basically contains an GTX texture with frames/"windows" into the texture.
    /// Based on Pokémon XD (GXXP01).
    /// </summary>
    public sealed class GSAGTX : ArchiveNode, IFileAccess
    {
        public override bool CanWrite => false;

        public GSAGTX()
        {
        }

        public GSAGTX(string name) : base(name)
        {
        }

        public GSAGTX(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.SequenceEqual(".GSAGTX") && stream.Length > 128;

        protected override void Deserialize(Stream stream)
        {
            Header header = stream.Read<Header>(Endian.Big);
            FileNode file = new("BaseTexture.GTX", new SubStream(stream, stream.Length - header.TextureOffset, header.TextureOffset));
            Add(file);
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        public struct Header
        {
            public uint Header00;

            /// <summary>
            /// Contrains entries to "windows" into the texture to display
            /// </summary>
            public uint FramesOffset;

            public uint Header08;

            /// <summary>
            /// Offset from the file to an GTX texture used as the base.
            /// </summary>
            public uint TextureOffset;
        }
    }
}

using AuroraLib.Archives;
using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    /// <summary>
    /// Genius Sonority "GSAGTX" file format, although the real file extension is unknown.
    /// It basically contains an GTX texture with frames/"windows" into the texture.
    /// Based on Pokémon XD (GXXP01).
    /// </summary>
    public class GSAGTX : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".GSAGTX";

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.SequenceEqual(Extension) && stream.Length > 128;

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

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };
            Header header = stream.Read<Header>(Endian.Big);
            Root.AddArchiveFile(stream, stream.Length - header.TextureOffset, header.TextureOffset, "BaseTexture.GTX");
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}

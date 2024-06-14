using AuroraLib.Common;
using AuroraLib.Core.Exceptions;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    // The actual file type is likely 'FIPA'
    // with a switch on subtypes (ex: 'FTEX')
    // but that would be more complicated
    // to deal with!
    public class FIPAFTEX : JUTTexture, IFileAccess, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public IIdentifier Identifier => magic;

        private static readonly Identifier64 magic = new("FIPAFTEX");

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (!stream.Match(magic))
                return false;

            return true;
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(magic);

            // These files contain one or more TPLs
            // so we use a helper function to parse the TPL
            // after finding the start of the stream
            while (stream.Search(TPL.Magic.AsSpan().ToArray()))
            {
                long HeaderStart = stream.Position;
                stream.Skip(4);  // skip TPL magic
                TPL.ProcessStream(stream, HeaderStart, this);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

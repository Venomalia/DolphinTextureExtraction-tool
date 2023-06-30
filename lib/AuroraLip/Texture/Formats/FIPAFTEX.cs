using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    // The actual file type is likely 'FIPA'
    // with a switch on subtypes (ex: 'FTEX')
    // but that would be more complicated
    // to deal with!
    public class FIPAFTEX : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "FIPAFTEX";

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (!stream.MatchString(magic))
                return false;

            return true;
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);

            // These files contain one or more TPLs
            // so we use a helper function to parse the TPL
            // after finding the start of the stream
            while (stream.Search(TPL.Magic.AsSpan().ToArray()))
            {
                long HeaderStart = stream.Position;
                stream.Read(4);  // skip TPL magic
                TPL.ProcessStream(stream, HeaderStart, this);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

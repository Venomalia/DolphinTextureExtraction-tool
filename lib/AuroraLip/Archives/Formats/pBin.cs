using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    // Used in Harvest Moon: Animal Parade
    public class pBin : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("pBin");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            uint unknown1 = stream.ReadUInt32(Endian.Big);
            uint unknown2 = stream.ReadUInt32(Endian.Big);
            uint unknown3 = stream.ReadUInt32(Endian.Big);
            uint unknown4 = stream.ReadUInt32(Endian.Big);
            uint count = stream.ReadUInt32(Endian.Big);

            Root = new ArchiveDirectory();
            for (int i = 0; i < count; i++)
            {
                uint size = stream.ReadUInt32(Endian.Big);
                uint offset = stream.ReadUInt32(Endian.Big);
                string type = stream.ReadString(4);
                uint unknown = stream.ReadUInt32(Endian.Big);
                Root.AddArchiveFile(stream, size, offset, "Entry" + i);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

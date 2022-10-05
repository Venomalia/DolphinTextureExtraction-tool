using AuroraLip.Common;
using System;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    // Used in Harvest Moon: Animal Parade
    public class pBin : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "pBin";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);

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

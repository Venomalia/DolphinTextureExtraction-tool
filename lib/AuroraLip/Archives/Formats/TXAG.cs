using AuroraLip.Common;

namespace AuroraLip.Archives.Formats
{
    public class TXAG : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "TXAG";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(Magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(Magic))
                throw new InvalidIdentifierException(Magic);

            ushort unk = stream.ReadUInt16(Endian.Big);
            ushort fileCount = stream.ReadUInt16(Endian.Big);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < fileCount; i++)
            {
                var offset = stream.ReadUInt32(Endian.Big);
                var length = stream.ReadUInt32(Endian.Big);
                var fileName = stream.ReadString(32);

                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = $"entry{i}.GVR";

                Root.AddArchiveFile(stream, length, offset, fileName);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    public class TXAG : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("TXAG");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

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

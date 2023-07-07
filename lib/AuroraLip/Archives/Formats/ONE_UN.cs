using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Archive use in Sonic Unleashed
    /// </summary>
    public class ONE_UN : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("one.");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier) && stream.At(4, S => stream.ReadUInt32()) <= 1024 * 4;

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            uint numEntries = stream.ReadUInt32();
            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < numEntries; i++)
            {
                string entryFilename = stream.ReadString(56);
                uint entryOffset = stream.ReadUInt32();
                uint entryLength = stream.ReadUInt32();

                if (Root.ItemKeyExists(entryFilename))
                {
                    Root.AddArchiveFile(stream, entryLength, entryOffset, entryFilename + i);
                }
                else
                {
                    Root.AddArchiveFile(stream, entryLength, entryOffset, entryFilename);
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

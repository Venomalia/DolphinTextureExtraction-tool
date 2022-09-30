using AuroraLip.Common;
using System;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    /// <summary>
    /// Archive use in Sonic Unleashed
    /// </summary>
    public class ONE_UN : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "one.";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!IsMatch(stream))
                throw new InvalidIdentifierException(Magic);

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

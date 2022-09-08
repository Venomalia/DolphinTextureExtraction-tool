using AuroraLip.Common;
using SevenZipExtractor;
using System;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    public class SevenZip : Archive, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public bool IsMatch(Stream stream, in string extension = "")
        {
            try
            {
                using (SevenZipExtractor.ArchiveFile archiveFile = new SevenZipExtractor.ArchiveFile(new SubStream(stream, stream.Length), null)) { }
                return true;
            }
            catch (Exception) { }
            return false;
        }

        protected override void Read(Stream ArchiveFile)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };

            //this protects our Stream from being closed by 7zip
            SubStream stream = new SubStream(ArchiveFile, ArchiveFile.Length);

            using (SevenZipExtractor.ArchiveFile archiveFile = new SevenZipExtractor.ArchiveFile(stream, null))
            {
                foreach (Entry entry in archiveFile.Entries)
                {
                    // extract to stream
                    MemoryStream memoryStream = new MemoryStream();
                    entry.Extract(memoryStream);
                    Root.AddArchiveFile(memoryStream, entry.FileName);
                }
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

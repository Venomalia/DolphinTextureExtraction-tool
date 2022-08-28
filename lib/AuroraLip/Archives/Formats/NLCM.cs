using AuroraLip.Common;
using System;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    // Rune Factory (Frontier) archive format
    // Cross-referenced with https://github.com/master801/Rune-Factory-Frontier-Tools
    public class NLCM : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "NLCM";

        private FileStream reference_stream;

        public NLCM() { }

        public NLCM(string filename) : base(filename) { }

        public NLCM(Stream stream, string filename = null) : base(stream, filename) { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (reference_stream != null)
                {
                    reference_stream.Dispose();
                }
            }
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
            uint table_offset = stream.ReadUInt32(Endian.Big);
            uint unknown2 = stream.ReadUInt32(Endian.Big);
            uint file_count = stream.ReadUInt32(Endian.Big);
            uint unknown3 = stream.ReadUInt32(Endian.Big);
            string reference_file = Path.Combine(Directory.GetParent(((System.IO.FileStream)stream).Name).FullName, stream.ReadString());
            stream.Seek(table_offset, SeekOrigin.Begin);
            reference_stream = new FileStream(reference_file,
                       FileMode.Open, FileAccess.Read, FileShare.Read);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (uint i = 0; i < file_count; i++)
            {
                uint size = stream.ReadUInt32(Endian.Big);
                uint padding = stream.ReadUInt32(Endian.Big);
                uint file_offset = stream.ReadUInt32(Endian.Big);
                uint padding2 = stream.ReadUInt32(Endian.Big);

                ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = i.ToString() };
                reference_stream.Seek(file_offset, SeekOrigin.Begin);
                Sub.FileData = new SubStream(reference_stream, size);
                Root.Items.Add(Sub.Name, Sub);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

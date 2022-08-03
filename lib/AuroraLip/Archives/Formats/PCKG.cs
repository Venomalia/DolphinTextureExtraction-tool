using AuroraLip.Common;
using System;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    class PCKG : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "PCKG";

        public PCKG() { }

        public PCKG(string filename) : base(filename) { }

        public PCKG(Stream stream, string filename = null) : base(stream, filename) { }

        public bool IsMatch(Stream stream, in string extension = "") => stream.MatchString(magic);

        private const string Bres = "bresþÿ";

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };

            //PCKG_CING seem to contain only bres files
            while (stream.Search(Bres))
            {
                long entrystart = stream.Position;
                if (!stream.MatchString(Bres))
                    continue;
                ushort Version = stream.ReadUInt16(Endian.Big);
                uint TotalSize = stream.ReadUInt32(Endian.Big);

                if (TotalSize > stream.Length - entrystart)
                {
                    stream.Search(Bres);
                    TotalSize = (uint)(stream.Position - entrystart);
                }

                ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = $"entry_{TotalFileCount + 1}.bres" };
                Sub.FileData = new SubStream(stream, TotalSize, entrystart);
                Root.Items.Add(Sub.Name, Sub);

                stream.Position = entrystart + TotalSize;
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}

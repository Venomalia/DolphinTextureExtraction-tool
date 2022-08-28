using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    public class RTDP : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "RTDP";

        public RTDP() { }

        public RTDP(string filename) : base(filename) { }

        public RTDP(Stream RARCFile, string filename = null) : base(RARCFile, filename) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
            int EOH = (int)stream.ReadUInt32(Endian.Big);
            int NrEntries = (int)stream.ReadUInt32(Endian.Big);
            int Size = (int)stream.ReadUInt32(Endian.Big);
            stream.Position = 0x20;

            Root = new ArchiveDirectory() { OwnerArchive = this };

            List<RTDPEntry> Entries = new List<RTDPEntry>();
            for (int i = 0; i < NrEntries; i++)
            {
                Entries.Add(new RTDPEntry(stream));
            }

            foreach (var Entry in Entries)
            {
                //If Duplicate...
                if (Root.Items.ContainsKey(Entry.Name)) Entry.Name = Path.GetFileName(Entry.Name) + Entries.IndexOf(Entry) + Path.GetExtension(Entry.Name);

                ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = Entry.Name };
                stream.Position = Entry.DataOffset + EOH;
                Sub.FileData = new MemoryStream(stream.Read((int)Entry.DataSize).DataXor(0x55));
                Root.Items.Add(Sub.Name, Sub);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private class RTDPEntry
        {
            public string Name { get; set; }
            public int DataSize { get; set; }
            public int DataOffset { get; set; }

            public RTDPEntry(Stream stream)
            {
                Name = stream.ReadString(32);
                DataSize = (int)stream.ReadUInt32(Endian.Big);
                DataOffset = (int)stream.ReadUInt32(Endian.Big);
            }
        }
    }
}

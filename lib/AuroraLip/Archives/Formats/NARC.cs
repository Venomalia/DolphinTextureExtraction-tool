using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    public class NARC : Archive, IMagicIdentify
    {
        public string Magic => magic;

        private const string magic = "NARC";

        public NARC() { }

        public NARC(string filename) : base(filename){}

        public NARC(Stream RARCFile, string filename = null) : base(RARCFile, filename){}

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");
            uint NrEntries = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            uint StringTableSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            uint DataTablePosition = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);

            Root = new ArchiveDirectory() { OwnerArchive = this };

            List<NARCEntry> Entries = new List<NARCEntry>();
            for (int i = 0; i < NrEntries; i++)
            {
                Entries.Add(new NARCEntry(stream));
            }
            long position = stream.Position;

            foreach (NARCEntry entry in Entries)
            {
                ArchiveFile Sub = new ArchiveFile() { Parent = Root };
                stream.Position = position + entry.NameOffset;
                Sub.Name = stream.ReadString();
                stream.Position = DataTablePosition + entry.DataOffset;
                Sub.FileData = stream.Read((int)entry.DataSize);
                Root.Items.Add(Sub.Name, Sub);
            }

        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        private class NARCEntry
        {
            public uint Unknown;
            public uint NameOffset;
            public uint DataOffset;
            public uint DataSize;

            public NARCEntry(Stream stream)
            {
                Unknown = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                NameOffset = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                DataOffset = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                DataSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            }
        }
    }
}

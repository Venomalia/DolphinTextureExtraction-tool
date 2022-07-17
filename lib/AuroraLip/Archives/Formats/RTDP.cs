using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    public class RTDP : Archive, IMagicIdentify
    {
        public string Magic => magic;

        private const string magic = "RTDP";

        public RTDP() { }

        public RTDP(string filename) : base(filename) { }

        public RTDP(Stream RARCFile, string filename = null) : base(RARCFile, filename) { }

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");
            int EOH = (int)BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            int NrEntries = (int)BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            int Size = (int)BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
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
                Sub.FileData = stream.Read((int)Entry.DataSize);
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
                DataSize = (int)BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                DataOffset = (int)BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            }
        }
    }
}

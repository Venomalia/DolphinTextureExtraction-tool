﻿using AuroraLip.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Archives.Formats
{
    public class RKV2 : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "RKV2";

        public RKV2() { }

        public RKV2(string filename) : base(filename) { }

        public RKV2(Stream stream, string filename = null) : base(stream, filename) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");
            uint FileCount = (uint)stream.ReadUInt32(Endian.Little);
            uint NameSize = (uint)stream.ReadUInt32(Endian.Little);
            uint FullName_Files = (uint)stream.ReadUInt32(Endian.Little);
            uint Dummy = (uint)stream.ReadUInt32(Endian.Little);
            uint Info_Offset = (uint)stream.ReadUInt32(Endian.Little);
            uint Dummy2 = (uint)stream.ReadUInt32(Endian.Little);

            uint NameOffset = FileCount * 20 + Info_Offset;

            uint FullName_Offset = FileCount * 16 + (NameOffset + NameSize);

            Root = new ArchiveDirectory() { OwnerArchive = this };

            stream.Position = Info_Offset;
            for (int i = 0; i < FileCount; i++)
            {
                uint NameOffsetForFile = (uint)stream.ReadUInt32(Endian.Little);
                uint DummyForFile = (uint)stream.ReadUInt32(Endian.Little);
                uint SizeForFile = (uint)stream.ReadUInt32(Endian.Little);
                uint OffsetForFile = (uint)stream.ReadUInt32(Endian.Little);
                uint CRCForFile = (uint)stream.ReadUInt32(Endian.Little);
                long FilePosition = stream.Position;

                stream.Position = NameOffsetForFile + NameOffset;
                string Name = stream.ReadString();

                //If Duplicate...
                if (Root.Items.ContainsKey(Name)) Name = Name + i.ToString();

                ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = Name };
                stream.Position = OffsetForFile;
                Sub.FileData = new SubStream(stream, SizeForFile);
                Root.Items.Add(Sub.Name, Sub);

                // Read the file, move on to the next one
                stream.Position = FilePosition;
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}
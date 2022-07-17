using AuroraLip.Common;
using System;
using System.IO;

//https://wiki.tockdom.com/wiki/BRRES_(File_Format)
namespace AuroraLip.Archives.Formats
{
    public class bres : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "bres";

        #region Fields and Properties

        public ushort ByteOrder { get; set; }

        #endregion

        public bres() { }

        public bres(string filename) : base(filename) { }

        public bres(Stream stream, string filename = null) : base(stream, filename) { }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new Exception($"Invalid Identifier. Expected \"{Magic}\"");
            ByteOrder = BitConverter.ToUInt16(stream.Read(2), 0); //65534 BigEndian
            if (ByteOrder != 65534)
            {
                throw new Exception($"ByteOrder: \"{ByteOrder}\" Not Implemented");
            }
            ushort Padding = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            uint TotalSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            ushort Offset = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            ushort sections = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
            stream.Position = Offset;
            //root sections
            if (!stream.MatchString("root"))
                throw new Exception($"Invalid Identifier. Expected \"root\"");
            uint RootSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            Root = new ArchiveDirectory() { Name = "root", OwnerArchive = this };
            ReadIndex(stream, (int)(stream.Position + RootSize - 8), Root);
            //Index Group

        }

        private void ReadIndex(Stream stream, in int EndOfRoot, ArchiveDirectory ParentDirectory)
        {
            //Index Group
            long StartOfGroup = stream.Position;
            uint GroupSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
            uint Groups = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);

            for (int i = 0; i < Groups + 1; i++)
            {
                ushort GroupID = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                ushort Unknown = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                ushort LeftIndex = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                ushort RightIndex = BitConverter.ToUInt16(stream.ReadBigEndian(2), 0);
                uint NamePointer = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                uint DataPointer = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                long EndOfGroup = stream.Position;
                string Name = String.Empty;
                if (NamePointer != 0)
                {
                    stream.Position = StartOfGroup + NamePointer;
                    Name = stream.ReadString(x => x != 0);

                    if (DataPointer != 0)
                    {
                        if (StartOfGroup + DataPointer >= EndOfRoot)
                        {
                            ArchiveFile Sub = new ArchiveFile() { Name = Name, Parent = ParentDirectory };
                            stream.Position = StartOfGroup + DataPointer;
                            string Magic = stream.ReadString(4);
                            uint FileSize = BitConverter.ToUInt32(stream.ReadBigEndian(4), 0);
                            stream.Position -= 8;
                            if (Magic != "RASD" && FileSize <= stream.Length - stream.Position)
                            {
                                Sub.FileData = stream.Read((int)FileSize);
                                ParentDirectory.Items.Add(Sub.Name, Sub);
                            }
                        }
                        else
                        {
                            stream.Position = StartOfGroup + DataPointer;
                            ArchiveDirectory Sub = new ArchiveDirectory(this, ParentDirectory) { Name = Name };
                            ReadIndex(stream, EndOfRoot, Sub);
                            ParentDirectory.Items.Add(Sub.Name, Sub);
                        }
                    }
                }
                stream.Position = EndOfGroup;
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

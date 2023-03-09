using AuroraLip.Common;

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
                throw new InvalidIdentifierException(Magic);
            ByteOrder = BitConverter.ToUInt16(stream.Read(2), 0); //65534 BigEndian
            if (ByteOrder != 65534)
            {
                throw new NotImplementedException($"ByteOrder: \"{ByteOrder}\"");
            }
            ushort Padding = stream.ReadUInt16(Endian.Big);
            uint TotalSize = stream.ReadUInt32(Endian.Big);
            ushort Offset = stream.ReadUInt16(Endian.Big);
            ushort sections = stream.ReadUInt16(Endian.Big);
            stream.Position = Offset;
            //root sections
            if (!stream.MatchString("root"))
                throw new InvalidIdentifierException("root");
            uint RootSize = stream.ReadUInt32(Endian.Big);
            Root = new ArchiveDirectory() { Name = "root", OwnerArchive = this };
            ReadIndex(stream, (int)(stream.Position + RootSize - 8), Root);
            //Index Group

        }

        private void ReadIndex(Stream stream, in int EndOfRoot, ArchiveDirectory ParentDirectory)
        {
            //Index Group
            long StartOfGroup = stream.Position;
            uint GroupSize = stream.ReadUInt32(Endian.Big);
            uint Groups = stream.ReadUInt32(Endian.Big);

            for (int i = 0; i < Groups + 1; i++)
            {
                ushort GroupID = stream.ReadUInt16(Endian.Big);
                ushort Unknown = stream.ReadUInt16(Endian.Big);
                ushort LeftIndex = stream.ReadUInt16(Endian.Big);
                ushort RightIndex = stream.ReadUInt16(Endian.Big);
                uint NamePointer = stream.ReadUInt32(Endian.Big);
                uint DataPointer = stream.ReadUInt32(Endian.Big);
                long EndOfGroup = stream.Position;
                string Name = String.Empty;
                if (NamePointer != 0)
                {
                    stream.Seek(StartOfGroup + NamePointer, SeekOrigin.Begin);
                    Name = stream.ReadString(x => x != 0);

                    if (DataPointer != 0)
                    {
                        if (StartOfGroup + DataPointer >= EndOfRoot)
                        {
                            ArchiveFile Sub = new ArchiveFile() { Name = Name, Parent = ParentDirectory, OwnerArchive = this };
                            stream.Seek(StartOfGroup + DataPointer, SeekOrigin.Begin);
                            string Magic = stream.ReadString(4);
                            uint FileSize = stream.ReadUInt32(Endian.Big);
                            stream.Position -= 8;
                            if (Magic != "RASD" && FileSize <= stream.Length - stream.Position)
                            {
                                Sub.FileData = new ArchiveFile.ArchiveFileStream(stream, FileSize) { Parent = Sub };
                                if (ParentDirectory.Items.ContainsKey(Sub.Name))
                                {
                                    for (int n = 1; true; n++)
                                    {
                                        if (!ParentDirectory.Items.ContainsKey($"{Sub.Name}_{n}"))
                                        {
                                            Sub.Name = $"{Sub.Name}_{n}";
                                            break;
                                        }
                                    }
                                }
                                ParentDirectory.Items.Add(Sub.Name, Sub);
                            }
                        }
                        else
                        {
                            stream.Seek(StartOfGroup + DataPointer, SeekOrigin.Begin);
                            ArchiveDirectory Sub = new ArchiveDirectory(this, ParentDirectory) { Name = Name };
                            ReadIndex(stream, EndOfRoot, Sub);
                            if (ParentDirectory.Items.ContainsKey(Sub.Name))
                            {
                                for (int n = 1; true; n++)
                                {
                                    if (!ParentDirectory.Items.ContainsKey($"{Sub.Name}_{n}"))
                                    {
                                        Sub.Name = $"{Sub.Name}_{n}";
                                        break;
                                    }
                                }
                            }
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

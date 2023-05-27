using AuroraLib.Common;

//https://wiki.tockdom.com/wiki/BRRES_(File_Format)
namespace AuroraLib.Archives.Formats
{
    public class Bres : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "bres";

        #region Fields and Properties

        public Endian ByteOrder { get; set; }

        #endregion Fields and Properties

        public Bres()
        { }

        public Bres(string filename) : base(filename)
        {
        }

        public Bres(Stream stream, string filename = null) : base(stream, filename)
        {
        }

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            Header header = new(stream);
            if (header.Magic != magic)
                throw new InvalidIdentifierException(Magic);
            ByteOrder = header.BOM;
            stream.Seek(header.RootOffset, SeekOrigin.Begin);
            //root sections
            if (!stream.MatchString("root"))
                throw new InvalidIdentifierException("root");
            uint RootSize = stream.ReadUInt32(ByteOrder);
            Root = new ArchiveDirectory() { Name = "root", OwnerArchive = this };
            ReadIndex(stream, (int)(stream.Position + RootSize - 8), Root);
            //Index Group
        }

        private void ReadIndex(Stream stream, in int EndOfRoot, ArchiveDirectory ParentDirectory)
        {
            //Index Group
            long StartOfGroup = stream.Position;
            uint GroupSize = stream.ReadUInt32(ByteOrder);
            uint Groups = stream.ReadUInt32(ByteOrder);

            IndexGroup[] groups = stream.For((int)Groups + 1, s => s.Read<IndexGroup>(ByteOrder));

            foreach (IndexGroup group in groups)
            {
                if (group.NamePointer != 0)
                {
                    stream.Seek(StartOfGroup + group.NamePointer, SeekOrigin.Begin);
                    string Name = stream.ReadString(x => x != 0);

                    if (group.DataPointer != 0)
                    {
                        stream.Seek(StartOfGroup + group.DataPointer, SeekOrigin.Begin);
                        if (StartOfGroup + group.DataPointer >= EndOfRoot)
                        {
                            ArchiveFile Sub = new() { Name = Name, Parent = ParentDirectory, OwnerArchive = this };
                            string Magic = stream.ReadString(4);
                            uint FileSize = stream.ReadUInt32(ByteOrder);
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
                            ArchiveDirectory Sub = new(this, ParentDirectory) { Name = Name };
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
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        public unsafe struct Header
        {
            private fixed char magic[4];
            public Endian BOM;
            public ushort Version;
            public uint Length;
            public ushort RootOffset;
            public ushort Sections;

            public Header(Stream stream)
            {
                string Magic = stream.ReadString(4);
                for (int i = 0; i < 4; i++)
                {
                    magic[i] = Magic[i];
                }
                BOM = stream.ReadBOM();
                Version = stream.ReadUInt16(BOM);
                Length = stream.ReadUInt32(BOM);
                RootOffset = stream.ReadUInt16(BOM);
                Sections = stream.ReadUInt16(BOM);
            }

            public string Magic
            {
                get
                {
                    fixed (char* magicPtr = magic)
                    {
                        return new string(magicPtr, 0, 4);
                    }
                }
                set
                {
                    for (int i = 0; i < 4; i++)
                    {
                        magic[i] = value[i];
                    }
                }
            }
        }

        public struct IndexGroup
        {
            public ushort GroupID;
            public ushort Unknown;
            public ushort LeftIndex;
            public ushort RightIndex;
            public uint NamePointer;
            public uint DataPointer;
        }
    }
}

using AuroraLib.Common;
using AuroraLib.Core.Exceptions;
using AuroraLib.Core.Interfaces;

//https://wiki.tockdom.com/wiki/BRRES_(File_Format)
namespace AuroraLib.Archives.Formats
{
    public class Bres : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new("bres");

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
            => stream.Match(Magic);

        protected override void Read(Stream stream)
        {
            Header header = new(stream);
            if (header.Magic != Magic)
                throw new InvalidIdentifierException(header.Magic, Magic);
            ByteOrder = header.BOM;
            stream.Seek(header.RootOffset, SeekOrigin.Begin);
            //root sections
            if (!stream.Match("root"))
                throw new InvalidIdentifierException("root");
            uint RootSize = stream.ReadUInt32(ByteOrder);
            Root = new ArchiveDirectory() { Name = "root", OwnerArchive = this };
            //Index Group
            ReadIndex(stream, (int)(stream.Position + RootSize - 8), Root);

            //is brtex & brplt pair
            if (Root.Items.Count == 1 && Root.ItemExists("Textures(NW4R)"))
            {
                //try to request an external file.
                string datname = Path.ChangeExtension(Path.GetFileNameWithoutExtension(FullPath), ".brplt");
                try
                {
                    reference_stream = FileRequest.Invoke(datname);
                    Bres plt = new(reference_stream, FullPath);
                    foreach (var item in plt.Root.Items)
                    {
                        Root.Items.Add(item.Key, item.Value);
                    }
                }
                catch (Exception)
                { }
            }
        }

        private void ReadIndex(Stream stream, in int EndOfRoot, ArchiveDirectory ParentDirectory)
        {
            //Index Group
            long StartOfGroup = stream.Position;
            uint GroupSize = stream.ReadUInt32(ByteOrder);
            uint Groups = stream.ReadUInt32(ByteOrder);

            IndexGroup[] groups = stream.Read<IndexGroup>(Groups + 1, ByteOrder);

            foreach (IndexGroup group in groups)
            {
                if (group.NamePointer != 0)
                {
                    stream.Seek(StartOfGroup + group.NamePointer, SeekOrigin.Begin);
                    string Name = stream.ReadString();

                    if (group.DataPointer != 0)
                    {
                        stream.Seek(StartOfGroup + group.DataPointer, SeekOrigin.Begin);
                        if (StartOfGroup + group.DataPointer >= EndOfRoot)
                        {
                            ArchiveFile Sub = new() { Name = Name, Parent = ParentDirectory, OwnerArchive = this };
                            SubFile subFile = stream.Peek<SubFile>(ByteOrder);

                            if (subFile.Type != SubFile.FileType.RASD && subFile.Size <= stream.Length - stream.Position)
                            {
                                Sub.FileData = new ArchiveFile.ArchiveFileStream(stream, subFile.Size) { Parent = Sub };
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

        private Stream reference_stream;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                reference_stream?.Dispose();
            }
        }

        public struct Header
        {
            public Identifier32 Magic;
            public Endian BOM;
            public ushort Version;
            public uint Length;
            public ushort RootOffset;
            public ushort Sections;

            public Header(Stream stream)
            {
                Magic = stream.Read<Identifier32>();
                BOM = stream.ReadBOM();
                Version = stream.ReadUInt16(BOM);
                Length = stream.ReadUInt32(BOM);
                RootOffset = stream.ReadUInt16(BOM);
                Sections = stream.ReadUInt16(BOM);
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

        public struct SubFile
        {
            public Identifier32 Magic;
            public uint Size;
            public uint version;
            public uint BresOffset;

            public FileType Type => (FileType)(uint)Magic;

            public enum FileType : uint
            {
                /// <summary>
                /// Model movement animation. 
                /// </summary>
                CHR0 = 810698819,
                /// <summary>
                /// Color changing animation.
                /// </summary>
                CLR0 = 810699843,
                /// <summary>
                /// Model file. 
                /// </summary>
                MDL0 = 810304589,
                /// <summary>
                /// Texture swapping animation. 
                /// </summary>
                PAT0 = 67110656,
                /// <summary>
                /// Scene setting.
                /// </summary>
                SCN0 = 810435411,
                /// <summary>
                /// Polygon shape morphing animation.
                /// </summary>
                SHP0 = 810567763,
                /// <summary>
                /// Texture movement animation. 
                /// </summary>
                SRT0 = 810832467,
                /// <summary>
                /// Texture file. 
                /// </summary>
                TEX0 = 811091284,
                /// <summary>
                /// Color Palette file.
                /// </summary>
                PLT0 = 810830928,
                /// <summary>
                /// Bone visibility
                /// </summary>
                VIS0 = 810764630,
                /// <summary>
                /// Linked file
                /// </summary>
                RASD = 1146306898,
            };
        }
    }
}

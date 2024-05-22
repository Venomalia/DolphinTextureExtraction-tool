using AuroraLib.Common.Node;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats.Nintendo
{
    /// <summary>
    /// BRRES Binary Revolution RESource are archive-like files containing object data, models, textures, and animations.
    /// </summary>
    // ref https://wiki.tockdom.com/wiki/BRRES_(File_Format)
    public sealed class Bres : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new("bres");

        private static readonly Identifier32 Root = new("root");

        public Endian ByteOrder { get; set; } = Endian.Big;
        public ushort Version { get; set; } = 5;

        public Bres()
        { }

        public Bres(string name) : base(name)
        { }

        public Bres(FileNode source) : base(source)
        { }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x30 && stream.Match(Magic);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(Magic);
            ByteOrder = source.ReadBOM();
            Version = source.ReadUInt16(ByteOrder);
            uint length = source.ReadUInt32(ByteOrder);
            ushort RootOffset = source.ReadUInt16(ByteOrder);
            ushort Sections = source.ReadUInt16(ByteOrder);

            source.Seek(RootOffset, SeekOrigin.Begin);
            //root sections
            source.MatchThrow(Root);
            uint RootSize = source.ReadUInt32(ByteOrder);
            //Index Group
            ReadIndex(source, (int)(source.Position + RootSize - 8), this);


            //HACK for brtex & brplt pair
            if (Count == 1 && Contains("Textures(NW4R)"))
            {
                //try to request an external file.
                string datname = Path.ChangeExtension(Name, ".brplt");
                if (TryGetRefFile(datname, out FileNode refFile))
                {
                    streamData = refFile.Data;
                    Bres plt = new(refFile.Name);
                    plt.Deserialize(refFile.Data);
                    foreach (var item in plt)
                    {
                        item.Value.MoveTo(this);
                    }
                }
            }
        }

        private void ReadIndex(Stream source, int EndOfRoot, DirectoryNode ParentDirectory)
        {
            //Index Group
            long StartOfGroup = source.Position;
            uint GroupSize = source.ReadUInt32(ByteOrder);
            uint Groups = source.ReadUInt32(ByteOrder);

            using SpanBuffer<IndexGroup> groups = new(Groups + 1);
            source.Read<IndexGroup>(groups, ByteOrder);

            foreach (IndexGroup group in groups)
            {
                if (group.NameOffset != 0)
                {
                    source.Seek(StartOfGroup + group.NameOffset, SeekOrigin.Begin);
                    string Name = source.ReadString();

                    if (group.DataOffset != 0)
                    {
                        source.Seek(StartOfGroup + group.DataOffset, SeekOrigin.Begin);
                        if (StartOfGroup + group.DataOffset >= EndOfRoot)
                        {
                            SubFile subFile = source.Peek<SubFile>(ByteOrder);

                            if (subFile.Type != SubFile.FileType.RASD && subFile.Size <= source.Length - source.Position)
                            {
                                FileNode Sub = new(Name, new SubStream(source, subFile.Size) { Name = Name, Root = this });
                                ParentDirectory.Add(Sub);
                            }
                        }
                        else
                        {
                            DirectoryNode Sub = new(Name);
                            ReadIndex(source, EndOfRoot, Sub);
                            ParentDirectory.Add(Sub);
                        }
                    }
                }
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        public struct Header
        {
            public Identifier32 Magic;
            public Endian BOM;
            public ushort Version;
            public uint Length;
            public ushort RootOffset;
            public ushort Sections;

            public Header(Stream source)
            {
                Magic = source.Read<Identifier32>();
                BOM = source.ReadBOM();
                Version = source.ReadUInt16(BOM);
                Length = source.ReadUInt32(BOM);
                RootOffset = source.ReadUInt16(BOM);
                Sections = source.ReadUInt16(BOM);
            }
        }

        public struct IndexGroup
        {
            public ushort GroupID;
            public ushort Unknown;
            public ushort LeftIndex;
            public ushort RightIndex;
            public uint NameOffset;
            public uint DataOffset;
        }

        public struct SubFile
        {
            public Identifier32 Magic;
            public uint Size;
            public uint version;
            public uint BresOffset;

            public readonly FileType Type => (FileType)(uint)Magic;

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

        private Stream streamData;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                streamData?.Dispose();
            }
        }

        //only temporary
        public class SubStream : Core.IO.SubStream
        {
            public string Name;
            public DirectoryNode Root { get; set; }

            public SubStream(Stream stream, long length) : base(stream, length)
            {
            }

            public SubStream(Stream stream, long length, long offset) : base(stream, length, offset)
            {
            }
        }
    }
}

using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;
using RenderWareNET.Structs;
using System.Runtime.CompilerServices;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// SEGA Shadow The Hedgehog Archive
    /// </summary>
    public sealed class ONE_SH : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new("One ");

        public ONE_SH()
        {
        }

        public ONE_SH(string name) : base(name)
        {
        }

        public ONE_SH(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0x20 && stream.ReadUInt32() == 0 && stream.At(0xC, s => s.Match(Magic));

        protected override void Deserialize(Stream source)
        {
            int rwHeaderSize = Unsafe.SizeOf<RWPluginHeader>();
            RWPluginHeader rwHeader = source.Read<RWPluginHeader>();
            Header header = source.Read<Header>();

            source.Seek(0xb0, SeekOrigin.Begin);
            if (header.Version.Higher == 808857136) // 0.60
            {
                Entry[] entrys = source.For(header.Entrys, s => new Entry(source));
                for (int i = 0; i < entrys.Length; i++)
                {
                    uint size = i + 1 == entrys.Length ? (uint)(source.Length - entrys[i].Offset - rwHeaderSize) : entrys[i + 1].Offset - entrys[i].Offset;

                    FileNode Sub = new(entrys[i].Name, new SubStream(source, size, entrys[i].Offset + rwHeaderSize));
                    if (entrys[i].Flag == 1)
                        Sub.Name += ".prs";
                    Add(Sub);
                }
            }
            else
            {
                Entry5_0[] entrys = source.For(header.Entrys, s => new Entry5_0(source));
                for (int i = 0; i < entrys.Length; i++)
                {
                    uint size = i + 1 == entrys.Length ? (uint)(source.Length - entrys[i].Offset - rwHeaderSize) : entrys[i + 1].Offset - entrys[i].Offset;
                    FileNode Sub = new(entrys[i].Name, new SubStream(source, size, entrys[i].Offset + rwHeaderSize));
                    if (entrys[i].Flag == 1)
                        Sub.Name += ".prs";
                    Add(Sub);
                }
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        public struct Header
        {
            public Identifier32 Identifier;
            public Identifier64 Version;
            public uint Null2;
            public uint Entrys;
        }

        private interface IEntry
        {
            string Name { get; set; }
            uint Size { get; set; }
            uint Offset { get; set; }
            uint Flag { get; set; }
        }

        private struct Entry : IEntry
        {
            public string Name { get; set; }
            public uint Unk;
            public uint Size { get; set; }
            public uint Offset { get; set; }
            public uint Flag { get; set; }

            public Entry(Stream stream)
            {
                Name = stream.ReadString(0x28);
                Unk = stream.ReadUInt32();
                Size = stream.ReadUInt32();
                Offset = stream.ReadUInt32();
                Flag = stream.ReadUInt32();
            }
        }
        private struct Entry5_0 : IEntry
        {
            public string Name { get; set; }
            public uint Size { get; set; }
            public uint Offset { get; set; }
            public uint Flag { get; set; }
            public uint Unk;
            public uint Unk2;
            public uint Unk3;

            public Entry5_0(Stream stream)
            {
                Name = stream.ReadString(0x20);
                Size = stream.ReadUInt32();
                Offset = stream.ReadUInt32();
                Flag = stream.ReadUInt32();
                Unk = stream.ReadUInt32();
                Unk2 = stream.ReadUInt32();
                Unk3 = stream.ReadUInt32();
            }
        }
    }
}

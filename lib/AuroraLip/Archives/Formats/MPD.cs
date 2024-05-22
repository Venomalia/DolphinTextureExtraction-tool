using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;
using AuroraLib.Texture.Formats;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Square Enix Crystal Bearers MPD data
    /// </summary>
    public sealed class MPD : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(new byte[] { (byte)'M', (byte)'P', (byte)'D', 0 });

        public MPD()
        {
        }

        public MPD(string name) : base(name)
        {
        }

        public MPD(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            MPDHeader mpd = source.Read<MPDHeader>(Endian.Big);
            source.Seek(mpd.EntryOffset, SeekOrigin.Begin);
            Entry[] entries = source.For((int)mpd.Entrys, s => s.Read<Entry>(Endian.Big));


            for (int i = 0; i < entries.Length; i++)
            {
                switch (entries[i].Magic)
                {
                    case 1146110285: //MAPD this is temporary
                        Add(new FileNode("MAP.header", new SubStream(source, 0x140, mpd.BaseOffset + entries[i].Offset)));
                        int psize = 0x140;
                        int index = 0;
                        do
                        {
                            source.Seek(mpd.BaseOffset + entries[i].Offset + psize, SeekOrigin.Begin);
                            int size = TPL.GetSize(source);
                            if (size == -1)
                            {
                                Add(new FileNode("MAP_data", new SubStream(source, entries[i].Size - psize, mpd.BaseOffset + entries[i].Offset + psize)));
                                break;
                            }
                            Add(new FileNode("MAP_Tex" + index++, new SubStream(source, size, mpd.BaseOffset + entries[i].Offset + psize)));
                            psize += size;

                        } while (psize < entries[i].Size);
                        break;
                    default:
                        Add(new FileNode("data." + entries[i].Magic.GetString(), new SubStream(source, entries[i].Size, mpd.BaseOffset + entries[i].Offset)));
                        break;
                }
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

        public struct MPDHeader
        {
            public Identifier32 magic;
            public uint Pad;
            public uint Pad1;
            public uint Entrys;
            public uint EntryOffset;
            public uint BaseOffset;
        }

        public struct Entry
        {
            public Identifier32 Magic;
            public uint Size;
            public uint Offset;
        }
    }
}

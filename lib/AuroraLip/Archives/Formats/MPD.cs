using AuroraLib.Common;
using AuroraLib.Core.Interfaces;
using AuroraLib.Texture.Formats;

namespace AuroraLib.Archives.Formats
{
    //use in FFCC
    public class MPD : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(new byte[] { (byte)'M', (byte)'P', (byte)'D', 0 });

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            MPDHeader mpd = stream.Read<MPDHeader>(Endian.Big);
            stream.Seek(mpd.EntryOffset, SeekOrigin.Begin);
            Entry[] entries = stream.For((int)mpd.Entrys, s => s.Read<Entry>(Endian.Big));

            Root = new ArchiveDirectory() { OwnerArchive = this };

            for (int i = 0; i < entries.Length; i++)
            {
                switch (entries[i].Magic)
                {
                    case 1146110285: //MAPD this is temporary
                        Root.AddArchiveFile(stream, 0x140, mpd.BaseOffset + entries[i].Offset, "MAP.header");
                        int psize = 0x140;
                        int index = 0;
                        do
                        {
                            stream.Seek(mpd.BaseOffset + entries[i].Offset + psize, SeekOrigin.Begin);
                            int size = TPL.GetSize(stream);
                            if (size == -1)
                            {
                                Root.AddArchiveFile(stream, entries[i].Size - psize, mpd.BaseOffset + entries[i].Offset + psize, "MAP_data");
                                break;
                            }
                            Root.AddArchiveFile(stream, size, mpd.BaseOffset + entries[i].Offset + psize, "MAP_Tex"+ index++);
                            psize += size;

                        } while (psize < entries[i].Size);
                        break;
                    default:
                        Root.AddArchiveFile(stream, entries[i].Size, mpd.BaseOffset + entries[i].Offset, "data." + entries[i].Magic.GetString());
                        break;
                }
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

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

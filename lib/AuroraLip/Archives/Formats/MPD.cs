using AuroraLib.Common;
using AuroraLib.Common.Struct;

namespace AuroraLib.Archives.Formats
{
    //use in FFCC
    public class MPD : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "MPD";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(Magic) && stream.ReadUInt8() == 0;

        protected override void Read(Stream stream)
        {
            MPDHeader mpd = stream.Read<MPDHeader>(Endian.Big);
            stream.Seek(mpd.EntryOffset, SeekOrigin.Begin);
            Entry[] entries = stream.For((int)mpd.Entrys, s => s.Read<Entry>(Endian.Big));

            Root = new ArchiveDirectory() { OwnerArchive = this };

            for (int i = 0; i < entries.Length; i++)
            {
                Root.AddArchiveFile(stream, entries[i].Size, mpd.BaseOffset + entries[i].Offset, entries[i].Magic.GetString());
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

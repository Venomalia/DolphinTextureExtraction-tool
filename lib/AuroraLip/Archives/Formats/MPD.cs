using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

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

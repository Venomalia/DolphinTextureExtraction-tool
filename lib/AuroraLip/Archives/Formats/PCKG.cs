using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    public class PCKG : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("PCKG");

        public PCKG()
        { }

        public PCKG(string filename) : base(filename)
        {
        }

        public PCKG(Stream stream, string filename = null) : base(stream, filename)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        private const string Bres = "bresþÿ";

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };

            //PCKG_CING seem to contain only bres files
            while (stream.Search(Bres))
            {
                long entrystart = stream.Position;
                if (!stream.Match(Bres))
                    continue;
                ushort Version = stream.ReadUInt16(Endian.Big);
                uint TotalSize = stream.ReadUInt32(Endian.Big);

                if (TotalSize > stream.Length - entrystart)
                {
                    stream.Search(Bres);
                    TotalSize = (uint)(stream.Position - entrystart);
                }

                ArchiveFile Sub = new ArchiveFile() { Parent = Root, Name = $"entry_{TotalFileCount + 1}.bres" };
                Sub.FileData = new SubStream(stream, TotalSize, entrystart);
                Root.Items.Add(Sub.Name, Sub);

                stream.Position = entrystart + TotalSize;
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}

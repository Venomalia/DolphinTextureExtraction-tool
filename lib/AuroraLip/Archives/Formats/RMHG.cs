using AuroraLib.Common;

namespace AuroraLib.Archives.Formats
{
    // base https://github.com/Zheneq/Noesis-Plugins/blob/b47579012af3b43c1e10e06639325d16ece81f71/fmt_fatalframe_rsl.py
    public class RMHG : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("RMHG");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            uint count = stream.ReadUInt32();
            uint DataOffset = stream.ReadUInt32();
            uint unknown2 = stream.ReadUInt32();
            uint dataSize = stream.ReadUInt32();

            stream.Seek(DataOffset, SeekOrigin.Begin);

            Root = new ArchiveDirectory();
            for (int i = 0; i < count; i++)
            {
                uint offset = stream.ReadUInt32();
                uint size = stream.ReadUInt32();
                uint[] unknown = new uint[6];// 0-2 unknown | 3-5 padding ?
                for (int r = 0; r < 6; r++)
                {
                    unknown[r] = stream.ReadUInt32();
                }
                if (size != 0)
                    Root.AddArchiveFile(stream, size, offset, "Entry" + i);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

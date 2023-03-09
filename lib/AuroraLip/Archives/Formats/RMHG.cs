using AuroraLip.Common;

namespace AuroraLip.Archives.Formats
{
    // base https://github.com/Zheneq/Noesis-Plugins/blob/b47579012af3b43c1e10e06639325d16ece81f71/fmt_fatalframe_rsl.py
    public class RMHG : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "RMHG";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);

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

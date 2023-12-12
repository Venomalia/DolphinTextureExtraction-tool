using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    public class BUG : BIG
    {

        public override IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new((byte)'B', (byte)'U', (byte)'G', 0);

        private static readonly byte[] _key = new[] { (byte)0xB3, (byte)0x98, (byte)0xCC, (byte)0x66 };

        protected override void Read(Stream ArchiveFile)
        {
            ArchiveFile.MatchThrow(Identifier);
            XORStream stream = new(ArchiveFile, _key);
            ReadData(stream);
        }

        protected override void Write(Stream ArchiveFile) => throw new NotImplementedException();
    }
}

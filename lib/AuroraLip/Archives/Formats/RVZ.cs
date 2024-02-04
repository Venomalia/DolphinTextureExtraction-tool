using AuroraLib.Common;
using AuroraLib.Common.Interfaces;
using AuroraLib.Core.Interfaces;
using AuroraLib.DiscImage.Dolphin;
using AuroraLib.DiscImage.Revolution;
using AuroraLib.DiscImage.RVZ;

namespace AuroraLib.Archives.Formats
{
    public partial class RVZ : Archive, IHasIdentifier, IFileAccess, IGameDetails
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        public GameID GameID => Header.GameID;

        public string GameName => Header.GameName;

        private static readonly Identifier32 _identifier = new(82, 86, 90, 1);

        private RvzStream rvzStream;

        public GameHeader Header;

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            rvzStream = new(stream);
            GCDisk reader;
            if (rvzStream.RVZDiscT.DiscType == DiscTypes.GameCube)
            {
                reader = new GCDisk();
                reader.Open(rvzStream);
            }
            else
            {
                HeaderBin wiiHeader = new(rvzStream)
                {
                    UseVerification = false,
                    UseEncryption = false
                };
                reader = new WiiDisk { Header = wiiHeader };
                ((WiiDisk)reader).ProcessPartitionData(rvzStream);
            }

            Root = reader.Root;
            reader.Root = null;
            Header = reader.Header;
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                rvzStream?.Dispose();
            }
        }
    }
}

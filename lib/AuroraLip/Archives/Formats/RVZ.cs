using AuroraLib.Archives.Formats.Nintendo;
using AuroraLib.Core.Interfaces;
using AuroraLib.DiscImage.Dolphin;
using AuroraLib.DiscImage.Revolution;
using AuroraLib.DiscImage.RVZ;

namespace AuroraLib.Archives.Formats
{
    public sealed partial class RVZ : WiiDisk, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(82, 86, 90, 1);

        private RvzStream rvzStream;

        public new GameHeader Header { get => header; set => header = value; }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            rvzStream = new(source);
            if (rvzStream.RVZDiscT.DiscType == DiscTypes.GameCube)
            {
                ProcessData(rvzStream, this);
            }
            else
            {
                Header = new HeaderBin(rvzStream)
                {
                    UseVerification = false,
                    UseEncryption = false
                };
                ProcessPartitionData(rvzStream);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

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

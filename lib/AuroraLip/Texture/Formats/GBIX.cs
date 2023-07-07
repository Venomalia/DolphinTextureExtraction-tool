using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class GBIX : GVRT
    {
        public override bool CanWrite => false;

        public override IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("GBIX");

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            uint startOfGVRT = stream.ReadUInt32();
            uint GlobalIndex = stream.ReadUInt32(Endian.Big);

            stream.Seek(startOfGVRT + 8, SeekOrigin.Begin);
            base.Read(stream); //GVRT
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}

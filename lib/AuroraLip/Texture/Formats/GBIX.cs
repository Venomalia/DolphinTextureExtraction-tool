using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class GBIX : GVRT
    {
        public override bool CanWrite => false;

        public override string Magic => magic;

        private const string magic = "GBIX";

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
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

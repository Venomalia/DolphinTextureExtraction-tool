using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class ALTX : ALIG
    {
        public override string Magic => magic;

        private const string magic = "ALTX";


        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);

            uint unk = stream.ReadUInt32();
            uint Offset = stream.ReadUInt32(Endian.Big);

            stream.Seek(Offset, SeekOrigin.Begin);

            base.Read(stream);
        }
    }
}

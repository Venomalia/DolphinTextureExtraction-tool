using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class ALTX : ALIG
    {
        public override IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("ALTX");


        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            uint unk = stream.ReadUInt32();
            uint Offset = stream.ReadUInt32(Endian.Big);

            stream.Seek(Offset, SeekOrigin.Begin);

            base.Read(stream);
        }
    }
}

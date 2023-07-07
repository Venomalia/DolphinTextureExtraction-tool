using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class GCT0 : JUTTexture, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("GCT0");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            GXImageFormat Format = (GXImageFormat)stream.ReadUInt32(Endian.Big);
            ushort Width = stream.ReadUInt16(Endian.Big);
            ushort Height = stream.ReadUInt16(Endian.Big);
            byte unkflag = (byte)stream.ReadByte(); //Flag?
            _ = stream.ReadInt24(Endian.Big);
            uint ImgOffset = stream.ReadUInt32(Endian.Big);
            _ = stream.ReadUInt64(Endian.Big);
            ushort unkmip = stream.ReadUInt16(Endian.Big); //mips?
            _ = stream.ReadUInt16(Endian.Big);
            uint unknown = stream.ReadUInt32(Endian.Big); //202

            // we calculate the mips
            int mips = Format.GetMipmapsFromSize((int)(stream.Length - ImgOffset), Width, Height);

            // Palette are not supported?
            if (Format.IsPaletteFormat())
            {
                throw new PaletteException($"{nameof(GCT0)} does not support palette formats.");
            }

            stream.Seek(ImgOffset, SeekOrigin.Begin);
            Add(new TexEntry(stream, null, Format, GXPaletteFormat.IA8, 0, Width, Height, mips)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = mips
            });
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}

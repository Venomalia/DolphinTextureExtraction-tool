using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class TEX_KS : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".tex";

        public TEX_KS()
        { }

        public TEX_KS(Stream stream) : base(stream)
        {
        }

        public TEX_KS(string filepath) : base(filepath)
        {
        }

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (!extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase))
                return false;

            uint Tex_Format = stream.ReadUInt32(Endian.Big);

            if (!TEX_ImageFormat.ContainsKey(Tex_Format))
                return false;

            uint ImageWidth = stream.ReadUInt32(Endian.Big);
            uint ImageHeight = stream.ReadUInt32(Endian.Big);
            return ImageWidth > 1 && ImageWidth <= 1024 && ImageHeight >= 1 && ImageHeight <= 1024 && TEX_ImageFormat[Tex_Format].CalculatedDataSize((int)ImageWidth, (int)ImageHeight) < stream.Length;
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        protected override void Read(Stream stream)
        {
            uint Tex_Format = stream.ReadUInt32(Endian.Big);

            GXImageFormat Format = TEX_ImageFormat[Tex_Format];
            uint ImageWidth = stream.ReadUInt32(Endian.Big);
            uint ImageHeight = stream.ReadUInt32(Endian.Big);
            uint Size = stream.ReadUInt32(Endian.Big);
            uint Unknown = stream.ReadUInt32(Endian.Big);
            uint MipMaps = stream.ReadUInt32(Endian.Big);
            uint Unknown2 = stream.ReadUInt32(Endian.Big);
            uint Unknown3 = stream.ReadUInt32(Endian.Big);

            TexEntry current = new TexEntry(stream, null, Format, GXPaletteFormat.IA8, 0, (int)ImageWidth, (int)ImageHeight)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = MipMaps
            };
            Add(current);
        }

        private static Dictionary<uint, GXImageFormat> TEX_ImageFormat = new Dictionary<uint, GXImageFormat>
        {
            { 0x00, GXImageFormat.RGBA32 },
            { 0x4b, GXImageFormat.IA8 },
            { 0x4c, GXImageFormat.CMPR }
        };

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}

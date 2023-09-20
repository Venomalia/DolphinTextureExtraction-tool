using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    // https://pikmintkb.com/wiki/TXE_file
    public class TXE : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".txe";

        public TXE()
        { }

        public TXE(Stream stream) : base(stream)
        {
        }

        public TXE(string filepath) : base(filepath)
        {
        }

        public TEXTyp Typ = TEXTyp.ModTEX;

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (stream.Length < 10 || !extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase))
                return false;

            ushort ImageWidth = stream.ReadUInt16(Endian.Big);
            ushort ImageHeight = stream.ReadUInt16(Endian.Big);
            if (!Enum.IsDefined(typeof(TEXTyp), stream.ReadUInt16(Endian.Big)))
                return false;
            TEXImageFormat Tex_Format = (TEXImageFormat)stream.ReadUInt16(Endian.Big);
            return ImageWidth > 1 && ImageWidth <= 1024 && ImageHeight >= 1 && ImageHeight <= 1024 && (int)Tex_Format <= 7 && ((GXImageFormat)Enum.Parse(typeof(GXImageFormat), Tex_Format.ToString())).CalculatedDataSize(ImageWidth, ImageHeight) < stream.Length;
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        protected override void Read(Stream stream)
        {
            ushort ImageWidth = stream.ReadUInt16(Endian.Big);
            ushort ImageHeight = stream.ReadUInt16(Endian.Big);
            Typ = (TEXTyp)stream.ReadUInt16(Endian.Big);

            int DataSize;
            float MaxLOD;
            GXImageFormat Format;
            if (Typ == TEXTyp.Old)
            {
                OLDTEXImageFormat Tex_Format = (OLDTEXImageFormat)stream.ReadUInt16(Endian.Big);
                Format = (GXImageFormat)Enum.Parse(typeof(GXImageFormat), Tex_Format.ToString());
                DataSize = stream.ReadInt32(Endian.Big);
                stream.Seek(20, SeekOrigin.Current);
            }
            else
            {
                TEXImageFormat Tex_Format = (TEXImageFormat)stream.ReadUInt16(Endian.Big);
                Format = (GXImageFormat)Enum.Parse(typeof(GXImageFormat), Tex_Format.ToString());
                MaxLOD = stream.ReadSingle(Endian.Big);
                stream.Seek(16, SeekOrigin.Current);
                DataSize = stream.ReadInt32(Endian.Big);
            }
            int Mipmaps = DataSize == 0 ? 0 : Format.GetMipmapsFromSize(DataSize, ImageWidth, ImageHeight);

            TexEntry current = new TexEntry(stream, null, Format, GXPaletteFormat.IA8, 0, ImageWidth, ImageHeight, Mipmaps)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = Mipmaps
            };
            Add(current);
        }

        public enum TEXTyp : ushort
        {
            ModTEX = 0,
            Old = 2
        }

        public enum TEXImageFormat : ushort
        {
            RGB565,
            CMPR,
            RGB5A3,
            I4,
            I8,
            IA4,
            IA8,
            RGBA32,
        }

        public enum OLDTEXImageFormat : ushort
        {
            RGB5A3,
            CMPR,
            RGB565,
            I4,
            I8,
            IA4,
            IA8,
            RGBA32,
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}

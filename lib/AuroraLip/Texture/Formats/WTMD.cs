using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    //bas on https://forum.xentax.com/viewtopic.php?t=9256
    public class WTMD : JUTTexture, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("WTMD");

        public WTMD()
        { }

        public WTMD(Stream stream) : base(stream)
        {
        }

        public WTMD(string filepath) : base(filepath)
        {
        }

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(Identifier);

            uint none = stream.ReadUInt16(Endian.Big);
            uint PalettePosition = stream.ReadUInt16(Endian.Big);
            int ImageWidth = stream.ReadUInt16(Endian.Big);
            int ImageHeight = stream.ReadUInt16(Endian.Big);
            GXImageFormat Format = (GXImageFormat)stream.ReadByte();
            WTMDPaletteFormat Palette = (WTMDPaletteFormat)stream.ReadByte();
            byte unknown1 = (byte)stream.ReadByte(); //2 0 3
            byte unknown2 = (byte)stream.ReadByte(); //1 2
            uint unknown3 = stream.ReadUInt16(Endian.Big);
            uint ImagePosition = stream.ReadUInt16(Endian.Big);
            uint unknown4 = stream.ReadUInt16(Endian.Big); //2 1 0
            uint padding1 = stream.ReadUInt32(Endian.Big);
            uint padding2 = stream.ReadUInt32(Endian.Big);
            uint padding3 = stream.ReadUInt16(Endian.Big);

            //the number of mips are not specified, we calculate them from the rest size.
            int size = (int)(stream.Length - ImagePosition);
            int mipmaps = Format.GetMipmapsFromSize(size, ImageWidth, ImageHeight);

            Span<byte> paletteData = Span<byte>.Empty;
            int paletteCount = 0;
            GXPaletteFormat paletteFormat = GXPaletteFormat.IA8;
            if (Format.IsPaletteFormat())
            {
                paletteFormat = (GXPaletteFormat)Enum.Parse(typeof(GXPaletteFormat), Palette.ToString());
                stream.Position = PalettePosition;
                int paletteSize = (int)ImagePosition - (int)PalettePosition;
                paletteCount = paletteSize / 2;
                paletteData = new byte[paletteSize];
                stream.Read(paletteData);
            }
            stream.Position = ImagePosition;
            TexEntry current = new TexEntry(stream, paletteData, Format, paletteFormat, paletteCount, ImageWidth, ImageHeight, mipmaps)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = mipmaps
            };
            while (stream.Position != stream.Length)
            {
                int i = (int)Math.Pow(2, current.Count);
                if (ImageWidth / i < 1 || ImageHeight / i < 1)
                    break;

                byte[] image = new byte[Format.CalculatedDataSize(ImageWidth, ImageHeight)];
                stream.Read(image);
                current.RawImages.Add(image);
            }
            Add(current);
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        public enum WTMDPaletteFormat : byte
        {
            none = 0,
            RGB565 = 1,
            RGB5A3 = 2,
            IA8 = 3
        }
    }
}

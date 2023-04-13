using AuroraLib.Common;
using AuroraLib.Texture;

namespace AuroraLip.Texture.Formats
{
    public class LFXT : JUTTexture, IMagicIdentify, IFileAccess
    {

        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "LFXT";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(magic);

        protected override void Read(Stream stream)
        {
            // NOTE: non-Nintendo consoles (PS2, XBox, PC) have the TXFL magic and use Little Endian
            // but that is out of scope for DTE
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);

            var _Name = stream.ReadString();
            var Properties = stream.Read<HeaderProperties>(Endian.Big);

            //To GXFormat
            var GXFormat = ToGXFormat(Properties.Format);
            var GXPalette = GXPaletteFormat.IA8;
            var Colours = 0;
            ReadOnlySpan<byte> palettedata = null;

            //is Palette
            if (GXFormat.IsPaletteFormat())
            {
                var startpos = stream.Position;
                stream.Seek(GXFormat.GetCalculatedTotalDataSize(Properties.Width, Properties.Height, Properties.Mipmaps), SeekOrigin.Current);

                Colours = GXFormat.GetMaxPaletteColours();
                palettedata = stream.Read(Colours * 2);
                GXPalette = Enum.Parse<GXPaletteFormat>(Properties.SubFormat.ToString());

                stream.Seek(startpos, SeekOrigin.Begin);
            }

            TexEntry current = new(stream, palettedata, GXFormat, GXPalette, Colours, Properties.Width, Properties.Height, Properties.Mipmaps)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = Properties.Mipmaps
            };
            Add(current);
            if (Properties.Format == LFXTFormat.NintendoCMPRAlpha)
            {
                current = new(stream, palettedata, GXFormat, GXPalette, Colours, Properties.Width, Properties.Height, Properties.Mipmaps)
                {
                    LODBias = 0,
                    MagnificationFilter = GXFilterMode.Nearest,
                    MinificationFilter = GXFilterMode.Nearest,
                    WrapS = GXWrapMode.CLAMP,
                    WrapT = GXWrapMode.CLAMP,
                    EnableEdgeLOD = false,
                    MinLOD = 0,
                    MaxLOD = Properties.Mipmaps
                };
                Add(current);
            }
        }

        protected override void Write(Stream stream) =>
            throw new NotImplementedException();

        private struct HeaderProperties
        {
            public LFXTFormat Format { get; set; }
            public ushort Width { get; set; }
            public ushort Height { get; set; }
            public LFXTPalette SubFormat { get; set; }
            private UInt24 pad { get; set; }
            public uint Flags { get; set; }
            public ushort Mipmaps { get; set; }

            public int PixelCount => Width * Height;
            public bool HasMipmaps => (Flags & 0x20) > 0;
        }

        private enum LFXTFormat : ushort
        {
            DXT1 = 0x048E,
            DXT3 = 0x0890,
            BytePalette = 0x088D,
            RGBA32 = 0x208C,
            NintendoRGBA32 = 0x0120,
            NintendoC4 = 0x8304,
            NintendoC8 = 0x8408,
            NintendoCMPR = 0x8104,
            NintendoCMPRAlpha = 0x8804,
            TwoPalette4 = 0x8904,
            TwoPalette8 = 0x8A08,
            SonyRGBA32 = 0x2001,
            SonyPalette4 = 0x0400,
            SonyPalette8 = 0x0800,
        }

        private static GXImageFormat ToGXFormat(LFXTFormat LFXT) => LFXT switch
        {
            LFXTFormat.DXT1 => throw new NotImplementedException($"Unsupported {LFXTFormat.DXT1} format"),
            LFXTFormat.DXT3 => throw new NotImplementedException($"Unsupported {LFXTFormat.DXT3} format"),
            LFXTFormat.BytePalette => throw new NotImplementedException($"Unsupported {LFXTFormat.BytePalette} format"),
            LFXTFormat.RGBA32 => throw new NotImplementedException($"Unsupported {LFXTFormat.RGBA32} format"),
            // FIXME: None of these work as-is, but they are the GX formats Dolphin ends up using
            LFXTFormat.NintendoRGBA32 => GXImageFormat.RGBA32,
            LFXTFormat.NintendoC4 => GXImageFormat.C4,
            LFXTFormat.NintendoC8 => GXImageFormat.C8,
            LFXTFormat.NintendoCMPR => GXImageFormat.CMPR,
            LFXTFormat.NintendoCMPRAlpha => GXImageFormat.CMPR,
            LFXTFormat.TwoPalette4 => GXImageFormat.C4,
            LFXTFormat.TwoPalette8 => GXImageFormat.C8,
            LFXTFormat.SonyRGBA32 => throw new NotImplementedException($"Unsupported {LFXTFormat.SonyRGBA32} format"),
            LFXTFormat.SonyPalette4 => throw new NotImplementedException($"Unsupported {LFXTFormat.SonyPalette4} format"),
            LFXTFormat.SonyPalette8 => throw new NotImplementedException($"Unsupported {LFXTFormat.SonyPalette8} format"),
            _ => throw new ArgumentOutOfRangeException(nameof(LFXT), LFXT, "Unknown format"),
        };

        private enum LFXTPalette : byte
        {
            RGB5A3 = 5,
            IA8 = 6,
        }
    }
}

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
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(Magic);
            string Name = stream.ReadString();
            var Properties = stream.Read<HeaderProperties>();

            //To GXFormat
            var GXFormat = ToGXFormat(Properties.Format);
            var GXPalette = GXPaletteFormat.IA8;
            int Colours = 0;
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
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        private struct HeaderProperties
        {
            public LFXTFormat Format { get; set; }
            public ushort Width { get; set; }
            public ushort Height { get; set; }
            public LFXTPalette SubFormat { get; set; }
            private UInt24 pad { get; set; }
            public uint Flags { get; set; }
            public ushort Mipmaps { get; set; }
            private int pad2 { get; set; }

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

        private static GXImageFormat ToGXFormat(LFXTFormat lFXT) => lFXT switch
        {
            LFXTFormat.DXT1 => throw new NotImplementedException(),
            LFXTFormat.DXT3 => throw new NotImplementedException(),
            LFXTFormat.BytePalette => throw new NotImplementedException(),
            LFXTFormat.RGBA32 => throw new NotImplementedException(),
            LFXTFormat.NintendoRGBA32 => GXImageFormat.RGBA32,
            LFXTFormat.NintendoC4 => GXImageFormat.C4,
            LFXTFormat.NintendoC8 => GXImageFormat.I8,
            LFXTFormat.NintendoCMPR => GXImageFormat.CMPR,
            LFXTFormat.NintendoCMPRAlpha => GXImageFormat.CMPR,
            LFXTFormat.TwoPalette4 => throw new NotImplementedException(),
            LFXTFormat.TwoPalette8 => throw new NotImplementedException(),
            LFXTFormat.SonyRGBA32 => throw new NotImplementedException(),
            LFXTFormat.SonyPalette4 => throw new NotImplementedException(),
            LFXTFormat.SonyPalette8 => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };

        private enum LFXTPalette : byte
        {
            RGB5A3 = 5,
            IA8 = 6,
        }
    }
}

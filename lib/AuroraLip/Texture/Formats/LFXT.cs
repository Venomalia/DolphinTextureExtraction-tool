using AuroraLib.Common;
using AuroraLib.Texture.BlockFormats;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.Formats
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
            string _Name = stream.ReadString();
            var Properties = stream.Read<HeaderProperties>(Endian.Big);

            //To GXFormat
            var GXFormat = ToGXFormat(Properties.Format);
            var GXPalette = ToGXFormat(Properties.SubFormat);
            ReadOnlySpan<byte> palettedata = null;

            if (Properties.Format == LFXTFormat.BGRA32)
            {
                MemoryStream ms = new();
                // BGRA32 to GXImageFormat.RGBA32 No idea why anyone does that XD
                for (int i = 0; i < Properties.Mipmaps + 1; i++)
                {
                    int size = GXFormat.CalculatedDataSize(Properties.Width, Properties.Height, i);
                    byte[] data = stream.Read(size);
                    //BGRA32 To RGBA32
                    for (int p = 0; p < data.Length; p += 4) //Swap R and B channel
                    {
                        (data[p], data[p + 2]) = (data[p + 2], data[p]);
                    }
                    //RGBA32 to GXImageFormat.RGBA32
                    Span<Rgba32> pixel = MemoryMarshal.Cast<byte, Rgba32>(data);
                    ms.Write(((IBlock<Rgba32>)new RGBA32Block()).EncodePixel(pixel, Properties.Width >> i, Properties.Height >> i));
                }
                stream = ms;
                stream.Seek(0, SeekOrigin.Begin);
            }
            //is Palette
            if (GXFormat.IsPaletteFormat())
            {
                long startpos = stream.Position;
                //The pallete is stored at the end of the image
                stream.Seek(GXFormat.GetCalculatedTotalDataSize(Properties.Width, Properties.Height, Properties.Mipmaps), SeekOrigin.Current);

                // Is it a two Palette format?
                if (Properties.Format == LFXTFormat.TwoPalette4 || Properties.Format == LFXTFormat.TwoPalette8)
                {
                    palettedata = stream.Read(Properties.Colours * 4);
                }
                else
                {
                    palettedata = stream.Read(Properties.Colours * 2);
                }
                stream.Seek(startpos, SeekOrigin.Begin);
            }

            TexEntry current = new(stream, palettedata, GXFormat, GXPalette, Properties.Colours, Properties.Width, Properties.Height, Properties.Mipmaps)
            {
                LODBias = 0,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = Properties.MaxLOD
            };
            Add(current);
            if (Properties.Format == LFXTFormat.NintendoCMPRAlpha)
            {
                current = new(stream, palettedata, GXFormat, GXPalette, Properties.Colours, Properties.Width, Properties.Height, Properties.Mipmaps)
                {
                    LODBias = 0,
                    EnableEdgeLOD = false,
                    MinLOD = 0,
                    MaxLOD = Properties.MaxLOD
                };
                Add(current);
            }
        }

        protected override void Write(Stream stream) =>
            throw new NotImplementedException();

        [StructLayout(LayoutKind.Explicit, Size = 20)]
        private struct HeaderProperties
        {
            [FieldOffset(0)]
            public LFXTFormat Format;

            [FieldOffset(2)]
            public ushort Width;

            [FieldOffset(4)]
            public ushort Height;

            [FieldOffset(6)]
            public LFXTPalette SubFormat;

            [FieldOffset(7)]
            private byte pad;

            [FieldOffset(8)]
            public ushort Colours;

            [FieldOffset(10)]
            public uint Flags;

            [FieldOffset(14)]
            public ushort MaxLOD;

            [FieldOffset(16)]
            private uint pad2;

            public int Mipmaps => MaxLOD != 0 ? MaxLOD - 1 : 0;
            public int PixelCount => Width * Height;
            public bool HasMipmaps => (Flags & 0x20) > 0;
        }

        private enum LFXTFormat : ushort
        {
            DXT1 = 0x048E,
            DXT3 = 0x0890,
            BytePalette = 0x088D,
            RGBA32 = 0x208C,
            BGRA32 = 0x0120,
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
            LFXTFormat.BGRA32 => GXImageFormat.RGBA32,
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
            None = 0,
            IA82 = 2,
            RGB5A3 = 5,
            IA8 = 6,
        }

        private static GXPaletteFormat ToGXFormat(LFXTPalette LFXT) => LFXT switch
        {
            LFXTPalette.None => GXPaletteFormat.IA8,
            LFXTPalette.IA82 => GXPaletteFormat.IA8,
            LFXTPalette.RGB5A3 => GXPaletteFormat.RGB5A3,
            LFXTPalette.IA8 => GXPaletteFormat.IA8,
            _ => throw new ArgumentOutOfRangeException(nameof(LFXT), LFXT, "Unknown palette format"),
        };
    }
}

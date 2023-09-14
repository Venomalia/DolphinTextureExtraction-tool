using AuroraLib.Common;
using AuroraLib.Texture.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Tga;
using System.Runtime.InteropServices;

namespace AuroraLib.Texture.Formats
{
    //https://www.dca.fee.unicamp.br/~martino/disciplinas/ea978/tgaffs.pdf
    public class TGA : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string[] Extensions = new string[]{ ".tga", ".vda", ".icb", ".vst" };

        public bool IsMatch(Stream stream, in string extension = "")
        {
            Header header = stream.Read<Header>();
            return header.ColorMapType <=1 && header.Width != 0 && header.Height != 0 && header.Width != 20 && header.Height != 0 && Enum.IsDefined(header.ImageType) && header.PixelDepth != 0 && header.PixelDepth <= 32 && Extensions.Contains(extension.ToLower());
        }

        protected override void Read(Stream stream)
        {
            Header header = stream.Peek<Header>();
            switch (header.ImageType)
            {
                case TgaImageType.NoImageData:
                    break;
                case TgaImageType.TrueColor:
                case TgaImageType.RleTrueColor:
                case TgaImageType.BlackAndWhite:
                case TgaImageType.RleBlackAndWhite:
                    TgaDecoder decoder = TgaDecoder.Instance;
                    DecoderOptions options = new();
                    Image image = header.PixelDepth switch
                    {
                        8 => decoder.Decode<I8>(options, stream),
                        //15 => throw new NotImplementedException,
                        //16 => throw new NotImplementedException(),
                        //24 => throw new NotImplementedException(),
                        32 => decoder.Decode<Rgba32>(options, stream),
                        _ => throw new NotImplementedException($"{nameof(TGA)} with {header.PixelDepth} PixelDepth."),
                    };
                    Add(new(image));
                    image.Dispose();
                    break;
                case TgaImageType.ColorMapped:
                    //SixLabors.ImageSharp does not support reading the pallaten data so we can calculate the hash, so we have to do it ourselves.

                    if (header.ColorMapDepth != 32)
                        throw new NotImplementedException($"{nameof(TGA)} with {header.ColorMapDepth} ColorMapDepth.");

                    stream.Seek(0x12 + header.ColorMapStart, SeekOrigin.Begin);
                    Span<byte> palette = stackalloc byte[header.ColorMapLength * (header.ColorMapDepth / 8)];
                    stream.Read(palette);

                    //BGR to RGB
                    for (int i = 0; i < palette.Length; i += 4)
                    {
                        (palette[i], palette[i + 2]) = (palette[i + 2], palette[i]);
                    }

                    //reduces the colors from RGBA32 to RGB5A3.
                    Span<Rgba32> paletteRGBA = MemoryMarshal.Cast<byte, Rgba32>(palette);
                    Span<RGB5A3> paletteRGB5A3 = MemoryMarshal.Cast<byte, RGB5A3>(palette);
                    for (int c = 0; c < header.ColorMapLength; c++)
                    {
                        paletteRGB5A3[c].FromRgba32(paletteRGBA[c]);
                        paletteRGB5A3[c].PackedValue = BitConverterX.Swap(paletteRGB5A3[c].PackedValue);
                    }

                    TexEntry tex = new(stream, AImageFormats.C8, header.Width, header.Height)
                    {
                        LODBias = 0,
                        MagnificationFilter = GXFilterMode.Nearest,
                        MinificationFilter = GXFilterMode.Nearest,
                        WrapS = GXWrapMode.CLAMP,
                        WrapT = GXWrapMode.CLAMP,
                        EnableEdgeLOD = false,
                        MinLOD = 0,
                        MaxLOD = 0,
                        PaletteFormat = GXPaletteFormat.RGB5A3
                    };
                    tex.Palettes.Add(palette[..(header.ColorMapLength * 2)].ToArray());
                    Add(tex);
                    break;
                case TgaImageType.RleColorMapped:
                    throw new NotImplementedException($"{nameof(TGA)} with {header.ImageType}.");
                default:
                    throw new NotImplementedException($"{nameof(TGA)} ImageType {header.ImageType}.");
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public byte IdLength;
            public byte ColorMapType;
            public TgaImageType ImageType;
            public short ColorMapStart;
            public short ColorMapLength;
            public byte ColorMapDepth;
            public short OffsetX;
            public short OffsetY;
            public short Width;
            public short Height;
            public byte PixelDepth;
            public byte ImageDescriptor;
        }
    }
}

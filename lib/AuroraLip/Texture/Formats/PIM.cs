using AuroraLib.Common;
using AuroraLib.Texture.PixelFormats;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;

namespace AuroraLib.Texture
{
    public class PIM : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".PIM";

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.SequenceEqual(Extension);

        protected override void Read(Stream stream)
        {
            PIMHeader header = stream.Read<PIMHeader>();
            stream.Seek(header.POffset, SeekOrigin.Begin);
            Span<byte> palette = stackalloc byte[header.Colors * 4];
            stream.Read(palette);
            stream.Seek(header.IOffset, SeekOrigin.Begin);

            //The game reduces the colors from RGBA32 to RGB5A3 in real time.
            Span<Rgba32> paletteRGBA = MemoryMarshal.Cast<byte, Rgba32>(palette);
            Span<RGB5A3> paletteRGB5A3 = MemoryMarshal.Cast<byte, RGB5A3>(palette);
            for (int c = 0; c < header.Colors; c++)
            {
                //PS2 Alpha channel must be normalized.
                paletteRGBA[c].A = (byte)Math.Min(paletteRGBA[c].A * 2, 255);
                paletteRGB5A3[c].FromRgba32(paletteRGBA[c]);
                paletteRGB5A3[c].PackedValue = BitConverterX.Swap(paletteRGB5A3[c].PackedValue);
            }

            AImageFormats format = header.BPP switch
            {
                4 => AImageFormats.C4,
                8 => AImageFormats.C8,
                32 => AImageFormats.PS2RGBA32,
                _ => throw new NotImplementedException(),
            };

            TexEntry tex = new(stream, format, header.Width, header.Height)
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
            tex.Palettes.Add(palette[..(header.Colors * 2)].ToArray());
            Add(tex);
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        private struct PIMHeader
        {
            public ushort Width;
            public ushort Height;
            public ushort BPP;
            public ushort Colors;
            public uint POffset;
            public uint IOffset;
        }
    }
}

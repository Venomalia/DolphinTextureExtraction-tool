using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class GTX : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".GTX";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension == Extension && stream.Length > 128 && stream.At(0x28, s => s.ReadUInt32(Endian.Big) == 128);

        protected override void Read(Stream stream)
        {
            GTXHeader header = stream.Read<GTXHeader>(Endian.Big);

            GXImageFormat GXFormat = GTX2GXImageFormat(header.Format);
            GXPaletteFormat GXPalette = GXPaletteFormat.IA8;
            byte[] PaletteData = null;
            int PaletteCount = 0;
            if (GXFormat.IsPaletteFormat())
            {
                GXPalette = (GXPaletteFormat)Enum.Parse(typeof(GXPaletteFormat), header.PaletteFormat.ToString()); ;
                stream.Seek(header.PaletteOffset, SeekOrigin.Begin);
                PaletteCount = GXFormat.GetMaxPaletteColours();
                PaletteData = stream.Read(PaletteCount * 2);
            }

            GXFilterMode MagFilter = header.MagFilter == GTXFilterMode.Linear ? GXFilterMode.Linear : GXFilterMode.Nearest;
            GXFilterMode MinFilter = GXFilterMode.Nearest;
            if (header.MipmapFilter == GTXFilterMode.None)
            {
                MinFilter = header.MinFilter == GTXFilterMode.Linear ? GXFilterMode.Linear : GXFilterMode.Nearest;
            }
            else if (header.MipmapFilter == GTXFilterMode.Nearest)
            {
                MinFilter = header.MinFilter == GTXFilterMode.Linear ? GXFilterMode.LinearMipmapNearest : GXFilterMode.NearestMipmapNearest;
            }
            else if (header.MipmapFilter == GTXFilterMode.Linear)
            {
                MinFilter = header.MinFilter == GTXFilterMode.Linear ? GXFilterMode.LinearMipmapLinear : GXFilterMode.NearestMipmapLinear;
            }

            stream.Seek(header.Offset, SeekOrigin.Begin);
            Add(new TexEntry(stream, PaletteData, GXFormat, GXPalette, PaletteCount, header.Width, header.Height, header.NumEntries - 1)
            {
                WrapS = (GXWrapMode)header.WrapS,
                WrapT = (GXWrapMode)header.WrapT,
                MinificationFilter = MinFilter,
                MagnificationFilter = MagFilter,
                MinLOD = 0.0f,
                // MaxLOD = , // TODO: Something with NumEntries-1 is going on here
                LODBias = 0.0f,
                EnableEdgeLOD = false,
            });
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        protected GXImageFormat GTX2GXImageFormat(GTXFormat format)
        {
            switch (format)
            {
                case GTXFormat.I8_A0:
                case GTXFormat.I8_A1:
                case GTXFormat.I8_A2:
                case GTXFormat.I8_A3:
                    return GXImageFormat.I8;

                default:
                    return (GXImageFormat)Enum.Parse(typeof(GXImageFormat), format.ToString());
            }
        }

        public struct GTXHeader
        {
            //h0
            public ushort Width;

            public ushort Height;
            public byte Bpp;

            /// <summary>
            /// Base image + mipmaps, so minimum 1, maximum 8.
            /// </summary>
            public byte NumEntries;

            /// <summary>
            /// Mainly used for pool allocator, set to true while loading from file.
            /// </summary>
            public byte Used;

            /// <summary>
            /// Mainly set to 1 after GX calls, so overwritten during loading anyway.
            /// </summary>
            public byte InitedGX;

            /// <summary>
            /// Format of the texture.
            /// </summary>
            public GTXFormat Format;

            /// <summary>
            /// Format of the palette. Should be filled out when the texture uses palette.
            /// </summary>
            public GTXPaletteFormat PaletteFormat;

            //h10
            public uint WrapS;

            public uint WrapT;
            public GTXFilterMode MinFilter;
            public GTXFilterMode MagFilter;

            //h20
            public GTXFilterMode MipmapFilter;

            /// <summary>
            /// Used for runtime-allocated textures to free() the image data, 0 in files.
            /// </summary>
            public uint AllocatedOffset;

            /// <summary>
            /// Pointer to the texture with mipmaps stored sequentially like the GPU expects.
            /// </summary>
            public uint Offset;

            public uint Offset1; // Mipmap

            //h30
            public uint Offset2; // Mipmap

            public uint Offset3; // Mipmap
            public uint Offset4; // Mipmap
            public uint Offset5; // Mipmap

            //h40
            public uint Offset6; // Mipmap

            public uint Offset7; // Mipmap

            /// <summary>
            /// Pointer to palette data, size is the maximum the texture format can address.
            /// </summary>
            public uint PaletteOffset;

            /// <summary>
            /// Size of the texture with mipmaps in bytes.
            /// </summary>
            public uint Size;

            //h50
            /// <summary>
            /// Appears to be some sort of reference-like count, but doesn't free() when unreffing.
            /// Not relevant for extracting textures.
            /// </summary>
            public ushort UsageCount;

            /// <summary>
            /// Seemingly unused, only set to 0 when allocating from the pool, but ignored afterwards.
            /// </summary>
            public ushort Unknown54;

            // Space for GXTexObj and GXTlutObj follows, filled at runtime
        }

        public enum GTXFormat : uint
        {
            C4 = 0x00,
            C8 = 0x01,
            C14X2 = 0x30,
            I4 = 0x40,
            IA4 = 0x41,

            // There is a GX to GTX texture format conversion function (GXXP01 @ 0x8010471C)
            // which maps GX 0x27-0x2a (all unknown) to GTX 0xa0-0xa3, but not the other way around.
            // But there is also a GTX to something texture format conversion function (GXXP01 @ 0x801030c0)
            // that also maps each to a different value if a passed bool is unset.
            I8 = 0x42,

            I8_A0 = 0xa0,
            I8_A1 = 0xa1,
            I8_A2 = 0xa2,
            I8_A3 = 0xa3,
            IA8 = 0x43,
            RGB565 = 0x44,
            RGBA32 = 0x45,
            RGB5A3 = 0x90,
            CMPR = 0xB0,
        }

        public enum GTXPaletteFormat : uint
        {
            NONE = 0x00,
            IA8 = 0x01,
            RGB565 = 0x02,
            RGB5A3 = 0x03
        }

        public enum GTXFilterMode : uint
        {
            None = 0x00,
            Nearest = 0x01,
            Linear = 0x02
        }
    }
}

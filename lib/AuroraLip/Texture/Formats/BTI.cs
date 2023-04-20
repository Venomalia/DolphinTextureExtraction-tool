using AuroraLib.Common;
using System.Runtime.CompilerServices;

namespace AuroraLib.Texture.Formats
{

    public class BTI : JUTTexture, IFileAccess
    {

        public const string Extension = ".bti";

        public bool CanRead => true;

        public bool CanWrite => true;

        public bool IsMatch(Stream stream, in string extension = "")
            => extension.ToLower() == Extension;

        public JUTTransparency AlphaSetting { get; set; } = JUTTransparency.OPAQUE;
        public bool ClampLODBias { get; set; } = true;
        public byte MaxAnisotropy { get; set; } = 0;

        public BTI() { }

        public BTI(Stream stream) => Read(stream);

        public override void Save(Stream stream)
        {
            long BaseDataOffset = stream.Position + 0x20;
            Write(stream, ref BaseDataOffset);
        }
        public void Save(Stream BTIFile, ref long DataOffset) => Write(BTIFile, ref DataOffset);

        protected override void Read(Stream stream)
        {
            long HeaderStart = stream.Position;
            ImageHeader ImageHeader = stream.Read<ImageHeader>(Endian.Big);

            AlphaSetting = ImageHeader.AlphaSetting;
            ClampLODBias = ImageHeader.ClampLODBias;
            MaxAnisotropy = ImageHeader.MaxAnisotropy;

            ReadOnlySpan<byte> PaletteData = null;
            if (ImageHeader.IsPaletteFormat)
            {
                stream.Position = HeaderStart + ImageHeader.PaletteDataAddress;
                PaletteData = stream.Read(ImageHeader.PaletteCount * 2);
            }
            stream.Seek(HeaderStart + ImageHeader.ImageDataAddress, SeekOrigin.Begin);

            TexEntry current = new(stream, PaletteData, ImageHeader.Format, ImageHeader.PaletteFormat, ImageHeader.PaletteCount, ImageHeader.Width, ImageHeader.Height, ImageHeader.Mipmaps)
            {
                LODBias = ImageHeader.LODBias,
                MagnificationFilter = ImageHeader.MagnificationFilter,
                MinificationFilter = ImageHeader.MinificationFilter,
                WrapS = ImageHeader.WrapS,
                WrapT = ImageHeader.WrapT,
                EnableEdgeLOD = ImageHeader.EnableEdgeLOD,
                MinLOD = ImageHeader.MinLOD,
                MaxLOD = ImageHeader.MaxLOD
            };
            Add(current);
        }

        protected override void Write(Stream stream)
        {
            long DataOffset = Unsafe.SizeOf<ImageHeader>();
            Write(stream, ref DataOffset);
        }

        protected void Write(Stream stream, ref long DataOffset)
        {
            long HeaderStart = stream.Position;

            ImageHeader ImageHeader = new()
            {
                Format = this[0].Format,
                AlphaSetting = AlphaSetting,
                Width = (ushort)this[0].Width,
                Height = (ushort)this[0].Height,
                WrapS = this[0].WrapS,
                WrapT = this[0].WrapT,
                IsPaletteFormat = this[0].Format.IsPaletteFormat(),
                PaletteFormat = this[0].PaletteFormat,
                PaletteCount = (ushort)this[0].Palettes.Sum(p => p.Size),
                PaletteDataAddress = (uint)(DataOffset - HeaderStart),
                EnableMipmaps = Count > 1,
                EnableEdgeLOD = this[0].EnableEdgeLOD,
                ClampLODBias = ClampLODBias,
                MaxAnisotropy = MaxAnisotropy,
                MagnificationFilter = this[0].MagnificationFilter,
                MinificationFilter = this[0].MinificationFilter,
                MinLOD = this[0].MinLOD,
                MaxLOD = this[0].MaxLOD,
                ImageCount = (byte)this[0].Count,
                unknown = 0,
                LODBias = this[0].LODBias,
                ImageDataAddress = (uint)(DataOffset + this[0].Palettes.Sum(p => p.Size) - HeaderStart),
            };
            stream.WriteObjekt(ImageHeader, Endian.Big);

            long Pauseposition = stream.Position;
            stream.Position = DataOffset;

            foreach (Palette.JUTPalette bytes in this[0].Palettes)
                stream.Write(bytes.GetBytes());

            foreach (byte[] bytes in this[0].RawImages)
                stream.Write(bytes);

            DataOffset = stream.Position;
            stream.Position = Pauseposition;
        }

        private struct ImageHeader
        {
            public GXImageFormat Format;
            public JUTTransparency AlphaSetting;
            public ushort Width;
            public ushort Height;
            public GXWrapMode WrapS;
            public GXWrapMode WrapT;
            private byte isPaletteValue;
            public GXPaletteFormat PaletteFormat;
            public ushort PaletteCount;
            public uint PaletteDataAddress;
            private byte enableMipsValue;
            private byte edgeLOD;
            private byte clampLODBiasValue;
            public byte MaxAnisotropy;
            public GXFilterMode MagnificationFilter;
            public GXFilterMode MinificationFilter;
            private sbyte minLODValue;
            private sbyte maxLODValue;
            private byte imageCount;
            public byte unknown;
            private short lODBiasValue;
            public uint ImageDataAddress;

            public int Mipmaps => (EnableMipmaps && ImageCount != 0) ? ImageCount - 1 : 0;

            public bool IsPaletteFormat
            {
                get => isPaletteValue > 0;
                set => isPaletteValue = (byte)(value ? 1 : 0);
            }
            public bool EnableEdgeLOD
            {
                get => edgeLOD > 0;
                set => edgeLOD = (byte)(value ? 1 : 0);
            }
            public bool EnableMipmaps
            {
                get => enableMipsValue > 0;
                set => enableMipsValue = (byte)(value ? 1 : 0);
            }
            public bool ClampLODBias
            {
                get => clampLODBiasValue > 0;
                set => clampLODBiasValue = (byte)(value ? 1 : 0);
            }
            public float MinLOD
            {
                get => minLODValue / 8.0f;
                set => minLODValue = (sbyte)(value * 8.0f);
            }
            public float MaxLOD
            {
                get => EnableMipmaps ? maxLODValue / 8.0f : 0;
                set => maxLODValue = (sbyte)(value * 8.0f);
            }
            public byte ImageCount
            {
                get => imageCount != 0 ? imageCount : (byte)MaxLOD;
                set => imageCount = value;
            }
            public float LODBias
            {
                get => lODBiasValue / 100.0f;
                set => lODBiasValue = (short)(value * 100.0f);
            }
        }
    }
}

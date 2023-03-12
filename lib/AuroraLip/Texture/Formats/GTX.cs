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

            GXImageFormat GXFormat = (GXImageFormat)Enum.Parse(typeof(GXImageFormat), header.Format.ToString());
            GXPaletteFormat GXPalette = GXPaletteFormat.IA8;
            byte[] PaletteData = null;
            int PaletteCount = 0;
            if (GXFormat.IsPaletteFormat())
            {
                GXPalette = (GXPaletteFormat)Enum.Parse(typeof(GXPaletteFormat), header.PaletteFormat.ToString()); ;
                stream.Seek(header.PalletOffset, SeekOrigin.Begin);
                PaletteCount = GXFormat.GetMaxPaletteColours();
                PaletteData = stream.Read(PaletteCount * 2);
            }

#if DEBUG
            if (header.unk1 != 0 | header.unk2 != 0 | header.unk3 != 0 | header.unk4 != 0 | header.unk5 != 0 | header.unk6 != 0 | header.unk7 != 0)
            {
                Events.NotificationEvent.Invoke(NotificationType.Info, $"{nameof(GTX)}:{header.unk1}_{header.unk2}_{header.unk3}_{header.unk4}_{header.unk5}_{header.unk6}_{header.unk7}");
            }
#endif
            stream.Seek(header.Offset, SeekOrigin.Begin);
            Add(new TexEntry(stream, PaletteData, GXFormat, GXPalette, PaletteCount, header.Width, header.Height, 0)
            {
                WrapS = (GXWrapMode)header.WrapS,
                WrapT = (GXWrapMode)header.WrapT,
            });
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        public struct GTXHeader
        {
            //h0
            public ushort Width;
            public ushort Height;
            public byte Bpp;
            public byte Entries;
            public ushort unk1; //0
            public GTXFormat Format;
            public GTXPaletteFormat PaletteFormat;
            //h10
            public uint WrapS;
            public uint WrapT;
            public uint unk2; //0
            public uint MagFilter;
            //h20
            public uint MinFilter;
            public uint unk3; //0
            public uint Offset;
            public uint unk4; //0
            //h30
            public long unk5; //0
            public long unk6; //0
            //h40
            public long unk7; //0
            public uint PalletOffset;
        }

        public enum GTXFormat : uint
        {
            I4 = 0x40,
            I8 = 0x41,
            IA4 = 0x42,
            IA8 = 0x43 | 0xa0 | 0xa1 | 0xa2 | 0xa3,
            RGB565 = 0x44,
            RGBA32 = 0x45,
            RGB5A3 = 0x90,
            C4 = 0x00,
            C8 = 0x01,
            C14X2 = 0x30,
            CMPR = 0xB0,
        }

        public enum GTXPaletteFormat : uint
        {
            NONE = 0x00,
            IA8 = 0x01,
            RGB565 = 0x02,
            RGB5A3 = 0x03
        }

    }
}

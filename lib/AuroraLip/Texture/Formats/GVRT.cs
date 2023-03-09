using AuroraLip.Common;

namespace AuroraLip.Texture.Formats
{
    public class GVRT : JUTTexture, IMagicIdentify, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public virtual string Magic => magic;

        private const string magic = "GVRT";

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.MatchString(Magic);

        protected override void Read(Stream stream)
        {
            if (!stream.MatchString(magic))
                throw new InvalidIdentifierException(magic);

            uint size = stream.ReadUInt32(); //Remaining Size
            if (size != stream.Length - stream.Position)
                Events.NotificationEvent.Invoke(NotificationType.Info, $"Remaining Size Mismatch: {stream.Length - stream.Position - size}");

            stream.Seek(2, SeekOrigin.Current);

            var paletteFormatAndFlags = stream.ReadByte();
            DataFlags Flags = (DataFlags)(paletteFormatAndFlags & 0xF); // Flags uses the lower 4 bits
            GXPaletteFormat PaletteFormat = (GXPaletteFormat)(paletteFormatAndFlags >> 4); // Palette format uses the upper 4 bits
            GXImageFormat PixelFormat = (GXImageFormat)stream.ReadByte();
            ushort Width = stream.ReadUInt16(Endian.Big);
            ushort Height = stream.ReadUInt16(Endian.Big);

            byte[] PaletteData = null;
            int Colors = 16;
            if (PixelFormat.IsPaletteFormat())
            {
                if (Flags.HasFlag(DataFlags.InternalPalette))
                {
                    if (PixelFormat != GXImageFormat.C4)
                        Colors = 256;

                    PaletteData = stream.Read(Colors * 2);
                }
                else
                {
                    //Events.NotificationEvent.Invoke(NotificationType.Warning, $"Requires external pallet!");
                    throw new PaletteException("Requires external pallet!");
                }
            }

            int mipmaps = 0;
            if (Flags.HasFlag(DataFlags.Mipmaps))
                mipmaps = PixelFormat.GetMipmapsFromSize((int)size - 6, Width, Height);


            TexEntry current = new TexEntry(stream, PaletteData, PixelFormat, PaletteFormat, Colors, Width, Height, mipmaps)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = mipmaps - 1
            };
            Add(current);
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        [Flags]
        public enum DataFlags : byte
        {
            None = 0x0,
            Mipmaps = 0x1,
            ExternalPalette = 0x2,
            InternalPalette = 0x8,
            Palette = ExternalPalette | InternalPalette,
        }
    }
}

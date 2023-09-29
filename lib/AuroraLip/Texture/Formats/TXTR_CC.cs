using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    public class TXTRCC : JUTTexture, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("TXTR");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0xD0 && stream.Match(_identifier) && stream.ReadUInt32(Endian.Big) + 0x10 == stream.Length;

        protected override void Read(Stream stream)
        {
            string name = string.Empty;
            Span<byte> palette = Span<byte>.Empty;
            FMT_Data fmt = default;
            uint width = 0;
            uint height = 0;

            CCPropertieNote txtr = stream.Read<CCPropertieNote>(Endian.Big);
            long contentSize = stream.Position + txtr.ContentSize;
            while (stream.Position < contentSize)
            {
                CCPropertieNote propertie = stream.Read<CCPropertieNote>(Endian.Big);
                switch (propertie.Identifier)
                {
                    case 1162690894: //NAME
                        name = stream.ReadString((int)propertie.ContentSize);
                        break;
                    case 542395718:  //FMT
                        fmt = stream.Read<FMT_Data>(Endian.Big);
                        break;
                    case 1163544915: //SIZE
                        width = stream.ReadUInt32(Endian.Big);
                        height = stream.ReadUInt32(Endian.Big);
                        break;
                    case 1414283600: //PALT
                        palette = new byte[propertie.ContentSize];
                        stream.Read(palette);
                        break;
                    case 1195461961: //IMAG
                        //some are placeholders
                        if (propertie.ContentSize == 0 || propertie.ContentSize < fmt.GetGXFormat().GetCalculatedTotalDataSize((int)width, (int)height, 0))
                        {
                            return;
                        }

                        TexEntry current = new(stream, palette, fmt.GetGXFormat(), GXPaletteFormat.IA8, palette.Length / 4, (int)width, (int)height, 0)
                        {
                            LODBias = 0,
                            MagnificationFilter = GXFilterMode.Nearest,
                            MinificationFilter = GXFilterMode.Nearest,
                            WrapS = GXWrapMode.CLAMP,
                            WrapT = GXWrapMode.CLAMP,
                            EnableEdgeLOD = false,
                            MinLOD = 0,
                            MaxLOD = 0,
                        };
                        Add(current);
                        return;
                    case 538976288: //placeholder
                        break;
                    default:
                        throw new NotImplementedException();
                }
                stream.Align(0x10);
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        internal struct CCPropertieNote
        {
            public Identifier32 Identifier;
            public uint ContentSize;
            public uint Content;
            public uint Null { get; private set; }
        }

        private struct FMT_Data
        {
            public CCImageFormat Format;
            public byte UNK0;
            public byte UNK1;
            public byte UNK2;
            public uint UNK3;

            public enum CCImageFormat : byte
            {
                RGBA32 = 0x0,
                RGB565 = 0x1,
                C8 = 0x2,
                C4 = 0x3,
                I4 = 0x5,
                CMPR = 0x6,
                IA8 = 0x7,
                IA82 = 0x8,
                I8 = 0x9,
            }

            public GXImageFormat GetGXFormat() => Format switch
            {
                CCImageFormat.C8 => GXImageFormat.C8,
                CCImageFormat.C4 => GXImageFormat.C4,
                CCImageFormat.CMPR => GXImageFormat.CMPR,
                CCImageFormat.RGBA32 => GXImageFormat.RGBA32,
                CCImageFormat.RGB565 => GXImageFormat.RGB565,
                CCImageFormat.I4 => GXImageFormat.I4,
                CCImageFormat.IA8 => GXImageFormat.IA8,
                CCImageFormat.I8 => GXImageFormat.I8,
                CCImageFormat.IA82 => GXImageFormat.IA8,
                _ => throw new NotImplementedException(),
            };
        }
    }
}

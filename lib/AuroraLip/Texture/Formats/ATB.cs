using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    // base on https://github.com/gamemasterplc/mpatbtools/blob/master/atbdump.c
    public class ATB : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".atb";

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
        {
            if (stream.Length < 0x10 || !extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase))
                return false;

            stream.Seek(12, SeekOrigin.Begin);
            uint pattern_offset = stream.ReadUInt32(Endian.Big);
            return pattern_offset == 20;
        }

        public List<PatternEntry> Patterns = new List<PatternEntry>();

        public List<List<FrameEntry>> Banks = new List<List<FrameEntry>>();

        protected override void Read(Stream stream)
        {
            ushort num_banks = stream.ReadUInt16(Endian.Big);
            ushort num_patterns = stream.ReadUInt16(Endian.Big);
            ushort num_textures = stream.ReadUInt16(Endian.Big);
            ushort num_references = stream.ReadUInt16(Endian.Big); //0
            uint bank_offset = stream.ReadUInt32(Endian.Big);
            uint pattern_offset = stream.ReadUInt32(Endian.Big); // 20
            uint texture_offset = stream.ReadUInt32(Endian.Big);

            // Read patterns
            stream.Seek(pattern_offset, SeekOrigin.Begin);
            for (int i = 0; i < num_patterns; i++)
            {
                Patterns.Add(new PatternEntry(stream));
            }

            // Read banks
            stream.Seek(bank_offset, SeekOrigin.Begin);
            Span<Bank> blocks = stackalloc Bank[num_banks];
            stream.Read(blocks, Endian.Big);
            for (int i = 0; i < num_banks; i++)
            {
                Banks.Add(new List<FrameEntry>());
                stream.Seek(blocks[i].Offset, SeekOrigin.Begin);
                for (int fi = 0; fi < blocks[i].Frames; fi++)
                {
                    Banks[i].Add(new FrameEntry(stream));
                }
            }

            // Read textures
            stream.Seek(texture_offset, SeekOrigin.Begin);
            Span<TexturEntry> entries = stackalloc TexturEntry[num_textures];
            stream.Read(entries, Endian.Big);

            for (int i = 0; i < num_textures; i++)
            {
                GXImageFormat GXFormat = entries[i].AsGXImageFormat();

                Span<byte> PaletteData = Span<byte>.Empty;
                if (entries[i].IsPaletteFormat())
                {
                    stream.Seek(entries[i].PaletteOffset, SeekOrigin.Begin);
                    PaletteData = new byte[entries[i].PaletteSize * 2];
                    stream.Read(PaletteData);
                }

                stream.Seek(entries[i].ImageOffset, SeekOrigin.Begin);
                TexEntry current = new(stream, PaletteData, GXFormat, GXPaletteFormat.RGB5A3, entries[i].PaletteSize, entries[i].Width, entries[i].Height)
                {
                    LODBias = 0,
                    MagnificationFilter = GXFilterMode.Nearest,
                    MinificationFilter = GXFilterMode.Nearest,
                    WrapS = GXWrapMode.CLAMP,
                    WrapT = GXWrapMode.CLAMP,
                    EnableEdgeLOD = false,
                    MinLOD = 0,
                    MaxLOD = 0
                };
                Add(current);
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        private readonly struct Bank
        {
            public readonly ushort Frames;
            public readonly ushort Padding;
            public readonly uint Offset;
        }

        public readonly struct TexturEntry
        {
            public readonly byte Bpp;
            public readonly ATB_Format Format;
            public readonly ushort PaletteSize;
            public readonly ushort Width;
            public readonly ushort Height;
            public readonly uint ImageSize;
            public readonly uint PaletteOffset;
            public readonly uint ImageOffset;

            public bool IsPaletteFormat() => PaletteOffset != ImageOffset;

            public GXImageFormat AsGXImageFormat() => Format switch
            {
                ATB_Format.RGBA32 => GXImageFormat.RGBA32,
                ATB_Format.RGB5A3 => GXImageFormat.RGB5A3,
                ATB_Format.RGB5A3_DUPE => GXImageFormat.RGB5A3,
                ATB_Format.C8 => Bpp == 24 ? throw new NotSupportedException("BPP 24") : GXImageFormat.C8,
                ATB_Format.C4 => GXImageFormat.C4,
                ATB_Format.IA8 => GXImageFormat.IA8,
                ATB_Format.IA4 => GXImageFormat.IA4,
                ATB_Format.I8 => GXImageFormat.I8,
                ATB_Format.I4 => GXImageFormat.I4,
                ATB_Format.A8 => GXImageFormat.RGBA32, //?
                ATB_Format.CMPR => GXImageFormat.CMPR,
                _ => throw new NotImplementedException(),
            };
        }

        public class PatternEntry
        {
            public ushort Layers;
            public ushort Center_x;
            public ushort Center_y;
            public ushort Width;
            public ushort Height;
            public uint LayerOffset;

            public PatternEntry(Stream stream)
            {
                Layers = stream.ReadUInt16(Endian.Big);
                Center_x = stream.ReadUInt16(Endian.Big);
                Center_y = stream.ReadUInt16(Endian.Big);
                Width = stream.ReadUInt16(Endian.Big);
                Height = stream.ReadUInt16(Endian.Big);
                LayerOffset = stream.ReadUInt32(Endian.Big);
            }
        }

        public class FrameEntry
        {
            public ushort Pattern_index;
            public ushort Frame_length;
            public ushort Shift_x;
            public ushort Shift_y;
            public ushort Flip;

            public FrameEntry(Stream stream)
            {
                Pattern_index = stream.ReadUInt16(Endian.Big);
                Frame_length = stream.ReadUInt16(Endian.Big);
                Shift_x = stream.ReadUInt16(Endian.Big);
                Shift_y = stream.ReadUInt16(Endian.Big);
                Flip = stream.ReadUInt16(Endian.Big);
            }
        }

        public enum ATB_Format : byte
        {
            RGBA32,
            RGB5A3,
            RGB5A3_DUPE,
            C8,
            C4,
            IA8,
            IA4,
            I8,
            I4,
            A8,
            CMPR
        }
    }
}

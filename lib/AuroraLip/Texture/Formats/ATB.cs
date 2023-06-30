using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    // base on https://github.com/gamemasterplc/mpatbtools/blob/master/atbdump.c
    public class ATB : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".atb";

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (stream.Length < 0x10 || extension.ToLower() != Extension)
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
            bank[] blocks = new bank[num_banks];
            for (int i = 0; i < num_banks; i++)
            {
                blocks[i].frames = stream.ReadUInt16(Endian.Big);
                stream.ReadUInt16(Endian.Big);
                blocks[i].frame_offset = stream.ReadUInt32(Endian.Big);
            }

            for (int i = 0; i < num_banks; i++)
            {
                Banks.Add(new List<FrameEntry>());
                stream.Seek(blocks[i].frame_offset, SeekOrigin.Begin);
                for (int fi = 0; fi < blocks[i].frames; fi++)
                {
                    Banks[i].Add(new FrameEntry(stream));
                }
            }

            // Read textures
            stream.Seek(texture_offset, SeekOrigin.Begin);
            TexturEntry[] entries = new TexturEntry[num_textures];
            for (int i = 0; i < num_textures; i++)
            {
                entries[i] = new TexturEntry(stream);
            }

            for (int i = 0; i < num_textures; i++)
            {
                if (entries[i].Format == ATB_Format.RGB5A3_DUPE)
                    entries[i].Format = ATB_Format.RGB5A3;
                if (entries[i].Format == ATB_Format.A8)
                    entries[i].Format = ATB_Format.RGBA32;

                GXImageFormat GXFormat = (GXImageFormat)Enum.Parse(typeof(GXImageFormat), entries[i].Format.ToString());

                byte[] PaletteData = null;
                if (GXFormat.IsPaletteFormat())
                {
                    stream.Seek(entries[i].PaletteOffset, SeekOrigin.Begin);
                    PaletteData = stream.Read(entries[i].PaletteSize * 2);
                }

                stream.Seek(entries[i].ImageOffset, SeekOrigin.Begin);
                TexEntry current = new TexEntry(stream, PaletteData, GXFormat, GXPaletteFormat.RGB5A3, entries[i].PaletteSize, entries[i].Width, entries[i].Height)
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

        private struct bank
        {
            public ushort frames;
            public uint frame_offset;
        }

        public class TexturEntry
        {
            public byte Bpp;
            public ATB_Format Format;
            public ushort PaletteSize;
            public ushort Width;
            public ushort Height;
            public uint ImageSize;
            public uint PaletteOffset;
            public uint ImageOffset;

            public TexturEntry(Stream stream)
            {
                Bpp = (byte)stream.ReadByte();
                Format = (ATB_Format)stream.ReadByte();
                PaletteSize = stream.ReadUInt16(Endian.Big);
                Width = stream.ReadUInt16(Endian.Big);
                Height = stream.ReadUInt16(Endian.Big);
                ImageSize = stream.ReadUInt32(Endian.Big);
                PaletteOffset = stream.ReadUInt32(Endian.Big);
                ImageOffset = stream.ReadUInt32(Endian.Big);
            }
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

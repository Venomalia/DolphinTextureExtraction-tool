using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    /// <summary>
    /// TPL Verson 0, is used by few early Gamecube games.
    /// </summary>
    public class TPL_0 : JUTTexture, IFileAccess
    {

        public bool CanRead => true;

        public bool CanWrite => false;

        public bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
        {
            if (extension.ToLower().Equals(".tpl") && stream.ReadUInt32(Endian.Big) >= 1)
            {
                Entry entry = stream.At(4, S => S.Read<Entry>(Endian.Big));
                return Enum.IsDefined(typeof(GXImageFormat), entry.Format) && entry.ImageOffset > 10 && entry.Width > 2 && entry.Height > 2 && entry.Width < 1024 && entry.Height < 1024 && entry.MaxLOD != 0;
            }
            return false;
        }

        protected override void Read(Stream stream)
        {
            uint ImageCount = stream.ReadUInt32(Endian.Big);
            Entry[] entrys = stream.For((int)ImageCount, S => S.Read<Entry>(Endian.Big));
            foreach (var entry in entrys)
            {
                //some entries are just dummy
                if (entry.ImageOffset == 0 || entry.Height == 0 | entry.Width == 0)
                    continue;

                stream.Seek(entry.ImageOffset, SeekOrigin.Begin);

                TexEntry current = new TexEntry(stream, null, entry.Format, GXPaletteFormat.IA8, 16, entry.Width, entry.Height, entry.MaxLOD - 1)
                {
                    LODBias = 0,
                    MagnificationFilter = GXFilterMode.Linear,
                    MinificationFilter = GXFilterMode.Linear,
                    WrapS = GXWrapMode.CLAMP,
                    WrapT = GXWrapMode.CLAMP,
                    EnableEdgeLOD = false,
                    MinLOD = entry.MinLOD,
                    MaxLOD = entry.MaxLOD
                };
                Add(current);
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        private struct Entry
        {
            public GXImageFormat Format
            {
                get => (GXImageFormat)format;
                set => format = (uint)value;
            }
            private uint format { get; set; }
            public uint ImageOffset { get; set; }
            public ushort Width { get; set; }
            public ushort Height { get; set; }
            public byte MinLOD { get; set; }
            public byte MaxLOD { get; set; }
            public ushort Unknown { get; set; }
        }
    }
}

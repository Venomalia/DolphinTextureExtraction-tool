using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Texture.Formats
{
    /// <summary>
    /// Luigi's Mansion model files.
    /// </summary>
    // ref https://github.com/KillzXGaming/MdlConverter/tree/master/Plugins/GCNLibrary/LM/MDL
    public class MDL_LM : JUTTexture, IFileAccess, IHasIdentifier
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new(4, 180, 0, 0);

        public const string Extension = ".mdl";

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(Extension, StringComparison.InvariantCultureIgnoreCase) && stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            Header header = stream.Read<Header>(Endian.Big);
            //Read Texturs
            stream.Seek(header.TextureOffset, SeekOrigin.Begin);
            uint[] offsets = stream.For(header.TextureCount, s => s.ReadUInt32(Endian.Big));
            foreach (var offset in offsets)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                Add(Tex.Read(stream));
            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        private struct Header
        {
            public Identifier32 Magic;
            public ushort FaceCount;
            private readonly ushort Padding;
            public ushort NodeCount;
            public ushort ShapePacketCount;
            public ushort WeightCount;
            public ushort JointCount;
            public ushort VertexCount;
            public ushort NormalCount;
            public ushort ColorCount;
            public ushort TexcoordCount;
            private readonly ulong Padding2;
            public ushort TextureCount;
            private readonly ushort Padding3;
            public ushort TextureObjectCount;
            public ushort DrawElementsCount;
            public ushort MaterialCount;
            public ushort ShapeCount;
            private readonly uint Padding4;
            public uint NodeOffset;
            public uint ShapePacketOffset;
            public uint MatrixOffset;
            public uint WeightOffset;
            public uint JointIndexOffset;
            public uint WeightCountTableOffset;
            public uint VertexOffset;
            public uint NormalOffset;
            public uint ColorOffset;
            public uint TexcoordOffset;
            private readonly ulong Padding5;
            public uint TextureOffset;
            private readonly uint Padding6;
            public uint MaterialOffset;
            public uint TextureObjectOffset;
            public uint ShapeOffset;
            public uint DrawElementOffset;
            private readonly ulong Padding7;

            public bool IsValid()
                => _identifier == Magic;
        }

        private static class Tex
        {
            public static TexEntry Read(Stream stream)
            {
                Format format = (Format)stream.ReadUInt8();
                int pad = stream.ReadByte();
                ushort width = stream.ReadUInt16(Endian.Big);
                ushort height = stream.ReadUInt16(Endian.Big);
                stream.Skip(26);
                return new TexEntry(stream, null, (GXImageFormat)Enum.Parse(typeof(GXImageFormat), format.ToString()), GXPaletteFormat.IA8, 16, width, height, 0);
            }

            public enum Format : byte
            {
                I4 = 0x03,
                I8 = 0x04,
                IA4 = 0x05,
                IA8 = 0x06,
                RGB565 = 0x07,
                RGB5A3 = 0x08,
                RGBA32 = 0x09,
                CMPR = 0x0A
            }
        }
    }
}

using AuroraLib.Common;
using AuroraLib.Texture;
using static AuroraLib.Texture.Formats.PTLG;

namespace AuroraLib.Archives.Formats
{
    public class TexPO : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public const string Extension = ".TexPO";

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.SequenceEqual(Extension);

        protected override void Read(Stream stream)
        {
            Header header = stream.Read<Header>(Endian.Big);
            stream.Seek(0x60, SeekOrigin.Begin);
            int size = (int)(stream.Length - 0x60) - (header.ImageFormat.IsPaletteFormat() ? header.PaletteColors * 2 : 0);
            int mipmaps = header.ImageFormat.GetMipmapsFromSize(size, (int)header.Width, (int)header.Height);

            Add(new TexEntry(stream, header.ImageFormat, GXPaletteFormat.RGB5A3, (int)header.Width, (int)header.Height, mipmaps)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = mipmaps,
            });
        }

        protected override void Write(Stream ArchiveFile) => throw new NotImplementedException();

        internal readonly struct Header
        {
            public readonly uint Hash;
            public readonly ushort Width;
            public readonly ushort Height;
            private readonly ushort pad1;
            public readonly byte unk1;
            public readonly byte unk2;
            private readonly byte Format0;
            private readonly PTLGImageFormat Format1;
            private readonly byte Format2;
            private readonly byte Format3;
            public readonly uint unk5;
            public readonly uint Offset;
            public readonly uint Magic;
            public readonly uint PaletteOffset;
            public readonly ushort PaletteColors;

            public readonly GXImageFormat ImageFormat => Format1 switch
            {
                PTLGImageFormat.I4 => GXImageFormat.I4,
                PTLGImageFormat.I8 => GXImageFormat.I8,
                PTLGImageFormat.IA4 => GXImageFormat.IA4,
                PTLGImageFormat.RGB5A3 => Format3 switch
                {
                    0 => GXImageFormat.C8,
                    3 => GXImageFormat.RGB5A3,
                    4 => GXImageFormat.C8,
                    _ => throw new NotImplementedException(),
                },
                PTLGImageFormat.CMPR => GXImageFormat.CMPR,
                PTLGImageFormat.RGB565 => GXImageFormat.RGB565,
                PTLGImageFormat.RGBA32 => GXImageFormat.RGBA32,
                _ => throw new NotImplementedException(),
            };
        }
    }
}

using AuroraLib.Common;

namespace AuroraLib.Texture.Formats
{
    public class S3G : JUTTexture, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public static string Extension => ".s3g";

        public bool IsMatch(Stream stream, in string extension = "")
        {
            if (stream.Length <= 0x20 || extension.ToLower() != extension)
            {
                return false;
            }

            Header header = stream.Read<Header>();
            int size = GXImageFormat.CMPR.CalculatedDataSize((int)header.Width, (int)header.Height);
            return size + stream.Position == stream.Length;
        }

        protected override void Read(Stream stream)
        {
            Header header = stream.Read<Header>();
            Add(new(stream, Span<byte>.Empty, GXImageFormat.CMPR, GXPaletteFormat.RGB5A3, 0, (int)header.Width, (int)header.Height,0)
            {
                LODBias = 0,
                MagnificationFilter = GXFilterMode.Nearest,
                MinificationFilter = GXFilterMode.Nearest,
                WrapS = GXWrapMode.CLAMP,
                WrapT = GXWrapMode.CLAMP,
                EnableEdgeLOD = false,
                MinLOD = 0,
                MaxLOD = 0
            });
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        public struct Header
        {
            public uint unk;
            public uint unk2; //Images
            public uint Width;
            public uint Height;
            public uint unk3; // mips?
        }
    }
}

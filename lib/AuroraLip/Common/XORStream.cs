using AuroraLib.Core.Buffers;

namespace AuroraLib.Common
{
    public class XORStream : Stream
    {
        public readonly Stream Base;
        public readonly byte[] Key;

        public XORStream(Stream stream, byte key)
        {
            Base = stream;
            Key = new byte[] { key };
        }

        public XORStream(Stream stream, ReadOnlySpan<byte> key)
        {
            Base = stream;
            Key = new byte[key.Length * 2];
            for (int i = 0; i < Key.Length; i++)
            {
                Key[i] = key[i % key.Length];
            }
        }

        public override bool CanRead => Base.CanRead;
        public override bool CanSeek => Base.CanSeek;
        public override bool CanWrite => Base.CanWrite;
        public override long Length => Base.Length;
        public override long Position { get => Base.Position; set => Base.Position = value; }
        public override void Flush() => Base.Flush();
        public override long Seek(long offset, SeekOrigin origin) => Base.Seek(offset, origin);
        public override void SetLength(long value) => Base.SetLength(value);
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            int r = Base.Read(buffer);
            if (Key.Length == 1)
            {
                buffer.DataXor(Key[0]);
            }
            else
            {
                int offset = (int)(Base.Position % (Key.Length / 2));
                buffer.DataXor(Key.AsSpan(offset, (Key.Length / 2)));
            }
            return r;
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            using SpanBuffer<byte> rbuffer = new(buffer);
            if (Key.Length == 1)
            {
                rbuffer.Span.DataXor(Key[0]);
            }
            else
            {
                int offset = (int)(Base.Position % (Key.Length / 2));
                rbuffer.Span.DataXor(Key.AsSpan(offset, (Key.Length / 2)));
            }
            Base.Write(rbuffer);
        }

        protected override void Dispose(bool disposing)
        {
            Base.Dispose();
            base.Dispose(disposing);
        }
    }
}

using AuroraLib.Core.Buffers;

namespace AuroraLib.Common
{
    public class XORStream : Stream
    {
        public readonly Stream Base;
        public readonly byte Key;

        public XORStream(Stream stream, byte key)
        {
            Base = stream;
            Key = key;
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
            buffer.DataXor(Key);
            return r;
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            SpanBuffer<byte> rbuffer = new(buffer);
            rbuffer.Span.DataXor(Key);
            Base.Write(rbuffer);
        }

        protected override void Dispose(bool disposing)
        {
            Base.Dispose();
            base.Dispose(disposing);
        }
    }
}

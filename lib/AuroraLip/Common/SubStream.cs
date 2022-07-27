using System;
using System.IO;

namespace AuroraLip.Common
{
    /// <summary>
    /// a partial stream of an underlying stream.
    /// </summary>
    public class SubStream : Stream
    {
        /// <summary>
        /// Returns the underlying stream.
        /// </summary>
        public Stream BaseStream
        {
            get
            {
                if (basestream == null)
                    throw new ObjectDisposedException(GetType().Name);

                return basestream;
            }
        }
        private Stream basestream;

        /// <summary>
        /// Offset to the underlying stream.
        /// </summary>
        public readonly long Offset;

        private readonly bool ProtectBaseStream;

        public override bool CanRead => basestream != Null && BaseStream.CanRead;

        public override bool CanSeek => basestream != Null && BaseStream.CanSeek;

        public override bool CanWrite => false;

        public override long Length { get; }

        public override long Position
        {
            get => position;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (value > Length)
                    value = Length;
                position = value;
            }
        }
        private long position = 0;

        /// <summary>
        /// Creates a new substream instance of the specified stream at the specified offset with the specified length.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        /// <param name="protectBaseStream"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SubStream(Stream stream, long length, long offset, bool protectBaseStream = true)
        {
            basestream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || length > BaseStream.Length - offset) throw new ArgumentOutOfRangeException(nameof(length));

            Length = length;
            Offset = offset;
            ProtectBaseStream = protectBaseStream;
        }

        public SubStream(Stream stream, long length, bool protectBaseStream = true) : this(stream, length, stream.Position, protectBaseStream) { }


        public override void Flush()
            => BaseStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            long remaining = Length - Position;
            if (remaining <= 0) return 0;
            if (remaining < count) count = (int)remaining;

            BaseStream.Seek(Position + Offset, SeekOrigin.Begin);
            int r = BaseStream.Read(buffer, offset, count);
            Position += r;
            return r;
        }

        public override long Seek(long offset, SeekOrigin seekOrigin)
        {
            switch (seekOrigin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
            => BaseStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            long remaining = Length - Position;
            if (Length - Position < count)
                throw new ArgumentException(nameof(count));

            BaseStream.Position = Position + Offset;
            BaseStream.Write(buffer, offset, count);
            Position += count;
        }

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (basestream != null && !ProtectBaseStream)
                {
                    try { basestream.Dispose(); }
                    catch { }
                }
            }
            basestream = null;
        }
        #endregion
    }
}

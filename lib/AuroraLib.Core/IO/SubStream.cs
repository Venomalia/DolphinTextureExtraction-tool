using System.Diagnostics;

namespace AuroraLib.Core.IO
{
    /// <summary>
    /// Represents a <see cref="SubStream"/> that provides a view into a portion of an underlying <see cref="Stream"/>.
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
                if (_basestream == null)
                    throw new ObjectDisposedException(GetType().Name);

                return _basestream;
            }
        }
        private readonly Stream _basestream;

        /// <summary>
        /// Offset to the underlying stream.
        /// </summary>
        public readonly long Offset;

        private readonly bool ProtectBaseStream;

        /// <inheritdoc/>
        public override bool CanRead => _basestream != Null && BaseStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _basestream != Null && BaseStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _length;
        private readonly long _length;

        /// <inheritdoc/>
        public override long Position
        {
            [DebuggerStepThrough]
            get => _position;
            [DebuggerStepThrough]
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (value > Length)
                    value = Length;
                _position = value;
            }
        }
        protected long _position = 0;

        /// <summary>
        /// Creates a new <see cref="SubStream"/> instance of the specified <paramref name="stream"/> at the specified <paramref name="offset"/> with the specified <paramref name="length"/>.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <param name="length">The length of the substream.</param>
        /// <param name="offset">The offset within the underlying stream where the substream starts.</param>
        /// <param name="protectBaseStream">Specifies whether the base stream should be protected from being closed when the substream is closed.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [DebuggerStepThrough]
        public SubStream(Stream stream, long length, long offset, bool protectBaseStream = true)
        {
            _basestream = stream ?? throw new ArgumentNullException(nameof(stream));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || length > BaseStream.Length - offset) throw new ArgumentOutOfRangeException(nameof(length));

            _length = length;
            Offset = offset;
            ProtectBaseStream = protectBaseStream;
        }

        /// <summary>
        /// Creates a new <see cref="SubStream"/> instance of the specified <paramref name="stream"/> from the current Position with the specified <paramref name="length"/>.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <param name="length">The length of the substream.</param>
        /// <param name="protectBaseStream">Specifies whether the base stream should be protected from being closed when the substream is closed.</param>
        [DebuggerStepThrough]
        public SubStream(Stream stream, long length, bool protectBaseStream = true) : this(stream, length, stream.Position, protectBaseStream)
        {
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public override void Flush()
            => BaseStream.Flush();

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public override int Read(byte[] buffer, int offset, int count)
        {
            long remaining = Length - _position;
            if (remaining <= 0) return 0;
            if (remaining < count) count = (int)remaining;

            lock (_basestream)
            {
                BaseStream.Seek(_position + Offset, SeekOrigin.Begin);
                int r = BaseStream.Read(buffer, offset, count);
                _position += r;
                return r;
            }
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public override long Seek(long offset, SeekOrigin origin) => origin switch
        {
            SeekOrigin.Begin => Position = offset,
            SeekOrigin.Current => Position += offset,
            SeekOrigin.End => Position = Length + offset,
            _ => throw new ArgumentException($"Origin {origin} is invalid."),
        };

        /// <inheritdoc/>
        public override void SetLength(long value)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            long remaining = Length - _position;
            if (Length - _position < count)
                throw new ArgumentOutOfRangeException(nameof(count));

            BaseStream.Position = _position + Offset;
            BaseStream.Write(buffer, offset, count);
            _position += count;
        }

        /// <inheritdoc/>
        public override string ToString() => $"[0x{Offset:X8}] [0x{Length:X8}]";

        #region Dispose
        /// <inheritdoc/>
        [DebuggerStepThrough]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_basestream != null && !ProtectBaseStream)
                {
                    try { _basestream.Dispose(); }
                    catch { }
                }
            }
        }
        #endregion Dispose
    }
}

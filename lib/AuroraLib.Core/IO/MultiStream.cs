namespace AuroraLib.Core.IO
{
    /// <summary>
    /// a virtual stream consists of several streams.
    /// </summary>
    public class MultiStream : Stream
    {
        public IList<Stream> BaseStreams
        {
            get
            {
                if (_basestreams == null)
                    throw new ObjectDisposedException(GetType().Name);

                return _basestreams;
            }
        }
        private readonly List<Stream> _basestreams;

        public override bool CanRead
        {
            get
            {
                foreach (var Stream in BaseStreams)
                    if (Stream.CanRead == false)
                        return false;
                return true;
            }
        }

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                long length = 0;
                foreach (var Stream in BaseStreams)
                    length += Stream.Length;
                return length;
            }
        }

        /// <inheritdoc/>
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

        public MultiStream(List<Stream> Streams)
        {
            _basestreams = Streams;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            foreach (var Stream in BaseStreams)
                Stream.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            long remaining = Length - Position;
            if (remaining <= 0) return 0;
            if (remaining < count) count = (int)remaining;

            long p = position;
            foreach (var Stream in BaseStreams)
            {
                if (Stream.Length <= p)
                {
                    p -= Stream.Length;
                    continue;
                }
                lock (Stream)
                {
                    Stream.Seek(p, SeekOrigin.Begin);
                    int r = Stream.Read(buffer, offset, count);
                    Position += r;
                    return r;
                }
            }
            return -1;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => origin switch
        {
            SeekOrigin.Begin => Position = offset,
            SeekOrigin.Current => Position += offset,
            SeekOrigin.End => Position = Length + offset,
            _ => throw new ArgumentException($"Origin {origin} is invalid."),
        };

        /// <inheritdoc/>
        public override void SetLength(long value)
            => throw new NotImplementedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotImplementedException();

        #region Dispose

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_basestreams != null)
                {
                    foreach (var Stream in BaseStreams)
                        Stream.Dispose();
                }
            }
        }

        #endregion Dispose
    }
}

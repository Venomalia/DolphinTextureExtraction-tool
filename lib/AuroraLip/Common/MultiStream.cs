using System;
using System.Collections.Generic;
using System.IO;

namespace AuroraLip.Common
{
    /// <summary>
    /// a virtual stream consists of several streams.
    /// </summary>
    public class MultiStream : Stream
    {

        public IList<Stream> BaseStream
        {
            get
            {
                if (basestream == null)
                    throw new ObjectDisposedException(GetType().Name);

                return basestream;
            }
        }
        private List<Stream> basestream;

        public override bool CanRead
        {
            get
            {
                foreach (var Stream in BaseStream)
                    if (Stream.CanRead == false)
                        return false;
                return true;
            }
        }

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                long length = 0;
                foreach (var Stream in BaseStream)
                    length += Stream.Length;
                return length;
            }
        }

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
            basestream = Streams;
        }

        public override void Flush()
        {
            foreach (var Stream in BaseStream)
                Stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long remaining = Length - Position;
            if (remaining <= 0) return 0;
            if (remaining < count) count = (int)remaining;

            long p = position;
            foreach (var Stream in BaseStream)
            {
                if (Stream.Length <= p)
                {
                    p -= Stream.Length;
                    continue;
                }
                lock (basestream)
                {
                    Stream.Seek(p, SeekOrigin.Begin);
                    int r = Stream.Read(buffer, offset, count);
                    Position += r;
                    return r;
                }
            }
            return -1;
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
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (basestream != null)
                {
                    foreach (var Stream in BaseStream)
                        Stream.Dispose();
                }
            }
        }
        #endregion
    }
}

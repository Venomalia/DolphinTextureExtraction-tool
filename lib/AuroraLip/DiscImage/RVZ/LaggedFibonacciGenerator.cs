using AuroraLib.Core.Buffers;
using System.Runtime.InteropServices;

namespace AuroraLib.DiscImage.RVZ
{
    //https://github.com/dolphin-emu/dolphin/blob/master/Source/Core/DiscIO/LaggedFibonacciGenerator.cpp
    public class LaggedFibonacciGenerator : Stream
    {
        private const int LFI = 521;
        private const int LFIx4 = LFI * 4;

        private readonly SpanBuffer<uint> buffer;
        public override long Position
        {
            get => position;
            set
            {
                if (value < 0)
                {
                    Backward();
                }
                else if (value >= LFIx4)
                {
                    for (int i = 0; i < value / LFIx4; i++)
                    {
                        Forward();
                    }
                    position = (int)value % LFIx4;
                }
                else
                {
                    position = (int)value;
                }
            }
        }
        private int position;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => long.MaxValue;

        public LaggedFibonacciGenerator() => buffer = new SpanBuffer<uint>(LFI);

        public void Initialize(Stream stream)
        {
            stream.Read(buffer.Slice(0, 17), Endian.Big);
            for (int i = 17; i < LFI; i++)
            {
                buffer[i] = (buffer[i - 17] << 23) ^ (buffer[i - 16] >> 9) ^ buffer[i - 1];
            }
            // Instead of doing the "shift by 18 instead of 16" oddity when actually outputting the data,
            // we can do the shifting (and byteswapping) at this point to make the read code simpler.
            for (int i = 0; i < LFI; i++)
            {
                buffer[i] = BitConverterX.Swap((buffer[i] & 0xFF00FFFF) | ((buffer[i] >> 2) & 0x00FF0000));
            }

            Position = 0;
            for (int i = 0; i < 4; i++)
                Forward();
        }

        private void Forward()
        {
            for (int i = 0; i < 32; i++)
                buffer[i] ^= buffer[i + LFI - 32];

            for (int i = 32; i < LFI; i++)
                buffer[i] ^= buffer[i - 32];
        }

        private void Backward()
            => throw new NotImplementedException("LaggedFibonacciGenerator.Backward");

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> outbuffer)
        {
            int outPosition = 0;
            Span<byte> byteBuffer = MemoryMarshal.Cast<uint, byte>(buffer);
            while (outPosition < outbuffer.Length)
            {
                int CopyCount = Math.Min(outbuffer.Length - outPosition, LFIx4 - position);
                byteBuffer.Slice(position, CopyCount).CopyTo(outbuffer[outPosition..]);
                Position += CopyCount;
                outPosition += CopyCount;
            }
            return outbuffer.Length;
        }

        public override long Seek(long offset, SeekOrigin origin) => origin switch
        {
            SeekOrigin.Begin => Position = offset,
            SeekOrigin.Current => Position += offset,
            SeekOrigin.End => Position = Length + offset,
            _ => throw new NotImplementedException(),
        };

        public override void SetLength(long value) { }
        public override void Flush() => throw new NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        private bool disposedValue;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposedValue)
            {
                if (disposing)
                {
                    buffer.Dispose();
                }
                disposedValue = true;
            }
        }
    }
}

using System.Diagnostics;

namespace AuroraLib.Common
{
    /// <summary>
    /// Reads bits from a stream.
    /// </summary>
    public class BitReader : IDisposable
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
        private readonly Stream basestream;

        /// <summary>
        /// A long value representing the length of the stream in bytes.
        /// </summary>
        public long Length => basestream.Length;

        /// <summary>
        /// The current position within the stream.
        /// </summary>
        public long Position
        {
            get => _buffered ? basestream.Position - 1 : basestream.Position;
            set
            {
                basestream.Seek(value, SeekOrigin.Begin);
                ResetBuffer();
            }
        }

        /// <summary>
        /// The current position within the byte.
        /// </summary>
        public int BitPosition
        {
            get => _bit;
            set
            {
                if (value >= 8 || value < 0)
                {
                    int shift = value / 8;

                    if (_buffered)
                        shift--;

                    ResetBuffer();
                    value %= 8;
                    basestream.Seek(shift, SeekOrigin.Current);
                }
                _bit = value;
            }
        }
        private int _bit = 0;

        /// <summary>
        /// Byte order, in which bytes are read and written
        /// Endian.Big [0,1,2,3,4,5,6,7][8,9,10,11,12,13,14,15]...->
        /// Endian.Little <-...[15,14,13,12,11,10,9,8][7,6,5,4,3,2,1,0]
        /// </summary>
        public Endian ByteOrder;

        protected long _buffer = 0;
        private bool _buffered = false;
        private readonly bool _protectbase;

        public BitReader(Stream stream, Endian byteorder = Endian.Big, bool leaveOpen = false)
        {
            basestream = stream ?? throw new ArgumentNullException(nameof(stream));
            _protectbase = leaveOpen;
            ByteOrder = byteorder;
        }

        /// <summary>
        /// Returns an arbitrary number of bits as int, are read from the current position.
        /// </summary>
        /// <param name="length">bits to read</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public virtual long ReadInt(int length = 1)
            => ReadInt(length, ByteOrder);

        /// <summary>
        /// Returns an arbitrary number of bits as int, are read from the current position.
        /// </summary>
        /// <param name="length">bits to read</param>
        /// <param name="byteorder"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public virtual long ReadInt(int length, Endian byteorder)
        {
            int startbit = _bit;
            FillBuffer(length);

            if (byteorder == Endian.Big)
            {
                startbit = (length + startbit + 7 & -8) - startbit;
                return _buffer.GetBits(startbit, length);
            }
            else
            {
                return _buffer.Swap().GetBits(64 - startbit, length);
            }
        }

        /// <summary>
        /// Returns a bool, read 1 bit at the current position.
        /// </summary>
        /// <returns></returns>
        [DebuggerStepThrough]
        public bool ReadBit()
            => ReadInt(1) != 0;

        [DebuggerStepThrough]
        public sbyte ReadInt8(Endian byteorder)
            => (sbyte)ReadInt(8, byteorder);

        [DebuggerStepThrough]
        public byte ReadUInt8(Endian byteorder)
            => (byte)ReadInt(8, byteorder);

        [DebuggerStepThrough]
        public short ReadInt16(Endian byteorder)
            => (short)ReadInt(16, byteorder);

        [DebuggerStepThrough]
        public ushort ReadUInt16(Endian byteorder)
            => (ushort)ReadInt(16, byteorder);

        [DebuggerStepThrough]
        public Int24 ReadInt24(Endian byteorder)
            => (Int24)ReadInt(24, byteorder);

        [DebuggerStepThrough]
        public UInt24 ReadUInt24(Endian byteorder)
            => (UInt24)ReadInt(24, byteorder);

        [DebuggerStepThrough]
        public int ReadInt32(Endian byteorder)
            => (int)ReadInt(32, byteorder);

        [DebuggerStepThrough]
        public uint ReadUInt32(Endian byteorder)
            => (uint)ReadInt(32, byteorder);

        [DebuggerStepThrough]
        public long ReadInt64(Endian byteorder)
            => ReadInt(64, byteorder);

        [DebuggerStepThrough]
        public ulong ReadUInt64(Endian byteorder)
            => (ulong)ReadInt(64, byteorder);

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <param name="bitposition"></param>
        [DebuggerStepThrough]
        public virtual void Seek(long offset, SeekOrigin origin, short bitposition = 0)
        {
            basestream.Seek(offset, origin);
            ResetBuffer();
            BitPosition = bitposition;
        }

        protected void FillBuffer(int value = 1)
        {
            value += BitPosition;
            _bit = (short)(value % 8);
            if (_buffered)
                value -= 8;

            while (value > 0)
            {
                _buffer <<= 8;
                _buffer += basestream.ReadUInt8();
                value -= 8;
            }
            if (value < 0)
                _buffered = true;
            else
                _buffered = false;
        }

        protected void ResetBuffer()
        {
            _buffer = 0;
            _buffered = false;
        }


        #region Dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (basestream != null && !_protectbase)
                {
                    try { basestream.Dispose(); }
                    catch { }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

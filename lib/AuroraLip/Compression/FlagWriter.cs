using AuroraLib.Common;

namespace AuroraLib.Compression
{
    /// <summary>
    /// Represents a flag writer used for compressing data. It provides methods to write individual bits.
    /// </summary>
    public class FlagWriter
    {
        private byte CurrentFlag;
        public byte BitsLeft { get; private set; }
        public readonly Stream Base;
        public readonly MemoryStream Buffer;
        public readonly Endian Order;

        public FlagWriter(Stream destination, MemoryStream buffer, Endian order)
        {
            Base = destination;
            Order = order;
            Buffer = buffer;
        }

        /// <summary>
        /// Writes a single bit as a flag. The bits are accumulated in a byte and flushed to the destination stream when necessary.
        /// </summary>
        /// <param name="bit">The bit value to write (true for 1, false for 0).</param>
        public void WriteBit(bool bit)
        {
            if (BitsLeft == 0)
            {
                CurrentFlag = 0;
                BitsLeft = 8;
            }

            if (bit)
            {
                if (Order == Endian.Little)
                    CurrentFlag |= (byte)(1 << (8 - BitsLeft));
                else
                    CurrentFlag |= (byte)(1 << (BitsLeft - 1));
            }

            BitsLeft--;

            if (BitsLeft == 0)
            {
                Base.WriteByte(CurrentFlag);
                Buffer.WriteTo(Base);
                Buffer.SetLength(0);
            }
        }

        /// <summary>
        /// Writes an integer value as a sequence of bits with the specified number of bits. The bits are written from the most significant bit to the least significant bit.
        /// </summary>
        /// <param name="value">The integer value to write.</param>
        /// <param name="bits">The number of bits to write (default is 1).</param>
        public void WriteInt(int value, int bits = 1)
        {
            for (int i = bits - 1; i >= 0; i--)
            {
                int bit = (value >> i) & 1;
                WriteBit(bit == 1);
            }
        }

        /// <summary>
        /// Flushes any remaining bits in the buffer to the underlying stream.
        /// </summary>
        public void Flush()
        {
            if (BitsLeft != 0)
            {
                Base.WriteByte(CurrentFlag);
                BitsLeft = 0;
            }
            if (Buffer.Length != 0)
            {
                Buffer.WriteTo(Base);
                Buffer.SetLength(0);
            }
        }
    }
}

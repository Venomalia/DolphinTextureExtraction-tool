namespace AuroraLib.Compression
{
    /// <summary>
    /// Reads individual bits from a stream and provides methods for interpreting the flag values.
    /// </summary>
    public class FlagReader
    {
        private byte CurrentFlag;
        public byte BitsLeft { get; private set; }
        public readonly Stream Base;
        public readonly Endian Order;

        public FlagReader(Stream source, Endian order)
        {
            Base = source;
            Order = order;
        }

        /// <summary>
        /// Reads a single bit from the stream.
        /// </summary>
        /// <returns>The value of the read bit.</returns>
        public bool Readbit()
        {
            if (BitsLeft == 0)
            {
                CurrentFlag = Base.ReadUInt8();
                BitsLeft = 8;
            }

            bool flag;
            if (Order == Endian.Little)
            {
                flag = (CurrentFlag & 1) == 1;
                CurrentFlag >>= 1;
            }
            else
            {
                flag = (CurrentFlag & 128) == 128;
                CurrentFlag <<= 1;
            }
            BitsLeft--;
            return flag;
        }

        /// <summary>
        /// Reads an integer value with the specified number of bits from the stream.
        /// </summary>
        /// <param name="bits">The number of bits to read.</param>
        /// <returns>The integer value read from the stream.</returns>
        public int ReadInt(int bits = 1)
        {
            int vaule = 0;
            for (int i = 0; i < bits; i++)
            {
                vaule <<= 1;
                if (Readbit())
                {
                    vaule |= 1;
                }
            }
            return vaule;
        }
    }
}

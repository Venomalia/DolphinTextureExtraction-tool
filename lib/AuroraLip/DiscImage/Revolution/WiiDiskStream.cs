using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;

namespace AuroraLib.DiscImage.Revolution
{
    /// <summary>
    /// Nintendo Wii AES Encrypted Disc Stream
    /// </summary>
    public class WiiDiskStream : SubStream
    {
        private readonly byte[] BlockBuffer;
        private readonly byte[] IVBuffer;

        private readonly Aes AES;

        public override bool CanWrite => false;

        private const int _ClustersSize = 0x8000;
        private const int _DataSize = 31744;
        public int BlockNumber { get; private set; }
        public int BlockOffset { get; private set; }
        private int BufferedBlock = -1;

        public override long Position
        {
            get => BlockNumber * _DataSize + BlockOffset;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (value > Length)
                    value = Length;
                BlockNumber = (int)(value / _DataSize);
                BlockOffset = (int)(value % _DataSize);
            }
        }

        public WiiDiskStream(Stream stream, long length, long offset, byte[] key) : base(stream, length, offset, true)
        {
            BlockBuffer = ArrayPool<byte>.Shared.Rent(_DataSize);
            IVBuffer = new byte[16];
            AES = Aes.Create();
            AES.KeySize = key.Length * 8;
            AES.Mode = CipherMode.CBC;
            AES.Padding = PaddingMode.Zeros;
            AES.Key = key;
        }

        public override int Read(Span<byte> buffer)
        {
            /*
             Partition data is encrypted using a key, which can be obtained from the partition header and the master key.
             The actual partition data starts at an offset into the partition, and it is formatted in "clusters" of size 0x8000 (32768).
             Each one of these blocks consists of 0x400(1024) bytes of encrypted SHA-1 hash data, followed by 0x7C00(31744) bytes of encrypted user data.
             The 0x400 bytes SHA-1 data is encrypted using AES-128-CBC, with the partition key and a null (all zeroes) IV. Clusters are aggregated into subgroups of 8
             */

            if (BufferedBlock != BlockNumber)
            {
                BufferedBlock = BlockNumber;
                long blockStart = (long)BlockNumber * _ClustersSize;

                //Get this Block IV
                base._position = blockStart + 976;
                base.Read(IVBuffer);
                AES.IV = IVBuffer;

                //Read Encrypted Block
                base._position = blockStart + 1024;
                int i = base.Read(BlockBuffer.AsSpan(0, _DataSize));
                //Decrypt Block
                AES.CreateDecryptor().TransformBlock(BlockBuffer, 0, i, BlockBuffer, 0);
            }

            int CopyCount = _DataSize - BlockOffset;
            if (buffer.Length <= CopyCount)
                CopyCount = buffer.Length;

            BlockBuffer.AsSpan(BlockOffset, CopyCount).CopyTo(buffer);
            Position += CopyCount;
            if (buffer.Length > CopyCount)
                return Read(buffer[CopyCount..]) + CopyCount;
            else
                return CopyCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        [DebuggerStepThrough]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ArrayPool<byte>.Shared.Return(BlockBuffer);
            }
        }
    }
}

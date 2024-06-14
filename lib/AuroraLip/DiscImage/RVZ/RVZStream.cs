using AuroraLib.Compression;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Compression.Interfaces;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AuroraLib.DiscImage.RVZ
{
    // ToDo, if we want to re-encode the data, we have to recalculate H0, H1 and H2.
    /// <summary>
    /// Represents a stream for reading RVZ files.
    /// </summary>
    public class RvzStream : Stream
    {
        private const int WiiChunkSize = 0x8000;
        private const int WiiDataSize = 0x7C00;

        private readonly byte[] ChunkBuffer;
        public readonly Header RVZHeader;
        public readonly DiscT RVZDiscT;
        private readonly Stream RVZFileStream;
        private readonly ICompressionDecoder Decoder;
        private readonly LaggedFibonacciGenerator PRNG;
        private int CurrentChunkIndex;
        private bool IsGroupWii;
        private RawDataT CurrentGroupList;

        // Stream overrides
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => RVZHeader.IsoFileSize;
        public override long Position { get; set; }

        public RvzStream(Stream rvzFileStream)
        {
            RVZHeader = rvzFileStream.Read<Header>();
            RVZDiscT = rvzFileStream.Read<DiscT>();

            if (RVZHeader.VersionCompatible.CompareTo(new(1)) > 0)
            {
                throw new NotImplementedException($"RVZ {RVZHeader.VersionCompatible} not supported");
            }

            ChunkBuffer = ArrayPool<byte>.Shared.Rent((int)RVZDiscT.ChunkSize);
            RVZFileStream = rvzFileStream;
            Decoder = RVZDiscT.GetDecoder();
            PRNG = new();
            Position = 0;
            CurrentGroupList = RVZDiscT.RawData[0];
            CurrentChunkIndex = -1;
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            if (Position >= Length)
                return 0;

            // Check if we need a new GroupList.
            if (Position < CurrentGroupList.DataOffset || Position >= CurrentGroupList.DataEndOffset)
                GetCurrentGroupList();

            // When we read decoded wii data, the chunks have less data because the hashes are missing.
            uint chunkSize = IsGroupWii ? RVZDiscT.ChunkSize / WiiChunkSize * WiiDataSize : RVZDiscT.ChunkSize;
            int chunk = (int)(CurrentGroupList.GroupIndex + (Position - CurrentGroupList.DataOffset) / chunkSize);
            if (chunk != CurrentChunkIndex)
            {
                CurrentChunkIndex = chunk;
                DecodeCurrentChunk();
            }

            int chunkOffset = (int)((Position - CurrentGroupList.DataOffset) % chunkSize);
            int CopyCount = (int)Math.Min(buffer.Length, Length - Position);
            CopyCount = Math.Min((int)Math.Min(chunkSize - chunkOffset, CurrentGroupList.DataSize - (Position - CurrentGroupList.DataOffset)), CopyCount);
            ChunkBuffer.AsSpan(chunkOffset, CopyCount).CopyTo(buffer);
            Position += CopyCount;

            // If the read data is in more than one chunk.
            if (buffer.Length > CopyCount)
                return Read(buffer[CopyCount..]) + CopyCount;
            else
                return CopyCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GetCurrentGroupList()
        {
            for (int i = 0; i < RVZDiscT.RawData.Length; i++)
            {
                RawDataT dataT = RVZDiscT.RawData[i];

                if (Position >= dataT.DataOffset && Position < dataT.DataOffset + dataT.DataSize)
                {
                    IsGroupWii = false;
                    CurrentGroupList = dataT;
                    return;
                }
            }

            // Read Wii partition data.
            for (int i = 0; i < RVZDiscT.Parts.Length; i++)
            {
                uint startSector = RVZDiscT.Parts[i].PartData[0].FirstSector;
                long partitionOffset = (long)startSector * WiiChunkSize;
                for (int p = 0; p < 2; p++)
                {
                    // We read Wii disc partition decoded, which is why we have to adjust the offsets and size.
                    PartDataT partT = RVZDiscT.Parts[i].PartData[p];
                    long offset = partitionOffset + ((long)partT.FirstSector - startSector) * WiiDataSize;
                    long size = (long)partT.Sectors * WiiDataSize;
                    RawDataT dataT = new(offset, size, partT.GroupIndex, partT.Groups);

                    if (Position < offset + size)
                    {
                        IsGroupWii = true;
                        CurrentGroupList = dataT;
                        return;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void DecodeCurrentChunk()
        {
            RvzGroupT groupT = RVZDiscT.Groups[CurrentChunkIndex];

            // special case where all bytes are zero.
            if (groupT.DataSize == 0)
            {
                Array.Clear(ChunkBuffer);
                return;
            }

            Stream rvzChunk = new SubStream(RVZFileStream, groupT.DataSize, groupT.DataOffset);
            if (groupT.IsCompressed)
            {
                rvzChunk = Decoder.Decompress(rvzChunk);
            }

            if (IsGroupWii)
            {
                // ToDo, find out why 2 bytes have to be read here.
                if (!groupT.IsCompressed)
                    _ = rvzChunk.Read<ushort>();

                // These are exceptions that are required when re-encoding Wii discs, we don't need them at the moment.
                ushort exceptions = rvzChunk.Read<ushort>(Endian.Big);
                List<ExceptionT> exceptionsList = new(exceptions);
                for (int i = 0; i < exceptions; i++)
                {
                    exceptionsList.Add(rvzChunk.Read<ExceptionT>());
                }
            }

            // Is RVZ packaging used?
            if (groupT.PackedSize == 0)
            {
                rvzChunk.Read(ChunkBuffer);
            }
            else
            {
                int bufferPosition = 0, size;
                while (rvzChunk.Position < rvzChunk.Length)
                {
                    size = rvzChunk.ReadInt32(Endian.Big);
                    Span<byte> bufferSpan = ChunkBuffer.AsSpan(bufferPosition, size & 0x7FFFFFFF);

                    // Is the PRNG algorithm used?
                    if ((size & 0x80000000) != 0)
                    {
                        PRNG.Initialize(rvzChunk);
                        PRNG.Position += bufferPosition % WiiChunkSize;
                        PRNG.Read(bufferSpan);
                    }
                    else
                    {
                        rvzChunk.Read(bufferSpan);
                    }
                    bufferPosition += bufferSpan.Length;
                }
            }
            rvzChunk.Close();
        }

        public override long Seek(long offset, SeekOrigin origin) => origin switch
        {
            SeekOrigin.Begin => Position = offset,
            SeekOrigin.Current => Position += offset,
            SeekOrigin.End => Position = Length + offset,
            _ => throw new NotImplementedException(),
        };

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() { }

        [DebuggerStepThrough]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ArrayPool<byte>.Shared.Return(ChunkBuffer);

                PRNG.Dispose();
            }
        }
    }
}

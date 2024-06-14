using AuroraLib.Compression;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Compression.Interfaces;
using AuroraLib.Core.Interfaces;
using System.Buffers;
using System.Runtime.InteropServices;

namespace AuroraLib.DiscImage.RVZ
{
    public class DiscT : IBinaryObject
    {
        public DiscTypes DiscType;
        public CompressionTypes Compression;
        public int CompressionLevel;
        public uint ChunkSize;
        // The first 0x80 bytes of the disc image.
        public readonly byte[] DiscHead;
        public PartT[] Parts;
        public readonly byte[] PartsHash;
        public RawDataT[] RawData;
        public RvzGroupT[] Groups;
        public byte[] AlgorithmData;

        public DiscT()
        {
            PartsHash = new byte[0x14];
            DiscHead = new byte[0x80];
        }

        public DiscT(Stream source) : this() => BinaryDeserialize(source);

        public ICompressionDecoder GetDecoder() => Compression switch
        {
            CompressionTypes.Zstandard => new Zstd(),
            _ => throw new NotImplementedException($"{Compression} compression is currently not supported."),
        };
        public ICompressionEncoder GetEncoder() => Compression switch
        {
            CompressionTypes.Zstandard => new Zstd(),
            _ => throw new NotImplementedException(),
        };


        public void BinaryDeserialize(Stream source)
        {
            source.Seek(0x48, SeekOrigin.Begin);
            DiscType = source.Read<DiscTypes>(Endian.Big);
            Compression = source.Read<CompressionTypes>(Endian.Big);
            CompressionLevel = source.Read<int>(Endian.Big);
            ChunkSize = source.Read<uint>(Endian.Big);
            source.Read(DiscHead); // The first 0x80 bytes of the disc image.
            uint partsLength = source.Read<uint>(Endian.Big); // The number of wia_part_t structs.
            uint partSize = source.Read<uint>(Endian.Big);
            long partsOffset = source.Read<long>(Endian.Big);
            source.Read(PartsHash);
            uint rawDataStruct = source.Read<uint>(Endian.Big);
            long rawOffset = source.Read<long>(Endian.Big);
            uint rawComSize = source.Read<uint>(Endian.Big);
            uint groups = source.Read<uint>(Endian.Big);
            long groupsOffset = source.Read<long>(Endian.Big);
            uint groupsComSize = source.Read<uint>(Endian.Big);
            uint algorithmDataSize = source.Read<uint>(Endian.Big);
            AlgorithmData = new byte[algorithmDataSize];
            source.Read(AlgorithmData);

            long endPose = source.Position;
            ICompressionDecoder decoder = GetDecoder();

            //read Partd data
            source.Seek(partsOffset, SeekOrigin.Begin);
            Parts = new PartT[partsLength];
            if (partSize == 0x30)
            {
                for (int i = 0; i < partsLength; i++)
                    Parts[i] = source.Read<PartT>();
            }
            else
            {
                throw new NotImplementedException();
            }

            using MemoryPoolStream rawData = decoder.Decompress(new SubStream(source, rawComSize, rawOffset));
            RawData = new RawDataT[rawDataStruct];
            rawData.Read<RawDataT>(RawData, Endian.Big);
            // The first wia_raw_data_t has raw_data_off set to 0x80 and raw_data_size set to 0x4FF80, but it actually contains 0x50000 bytes of data!
            RawData[0] = new(0, RawData[0].DataSize + 0x80, 0, RawData[0].Groups);

            using MemoryPoolStream groupData = decoder.Decompress(new SubStream(source, groupsComSize, groupsOffset));
            Groups = new RvzGroupT[groups];
            groupData.Read<RvzGroupT>(Groups, Endian.Big);

        }

        public void BinarySerialize(Stream dest)
        {
            RawData[0] = new(0x80, RawData[0].DataSize - 0x80, 0, RawData[0].Groups);
            ICompressionEncoder encoder = GetEncoder();
            dest.Write(DiscType, Endian.Big);
            dest.Write(Compression, Endian.Big);
            dest.Write(CompressionLevel, Endian.Big);
            dest.Write(ChunkSize, Endian.Big);
            dest.Write(DiscHead);
            dest.Write(Parts.Length, Endian.Big);
            dest.Write(0x30, Endian.Big);
            dest.Write<long>(292 + AlgorithmData.Length, Endian.Big);
            dest.Write(PartsHash);
            Span<byte> rawDataBytes = MemoryMarshal.Cast<RawDataT, byte>(RawData);
            using MemoryPoolStream databuffer = encoder.Compress(rawDataBytes);
            dest.Write(RawData.Length, Endian.Big);
            dest.Write<long>(292 + AlgorithmData.Length + (Parts.Length * 0x30), Endian.Big);
            dest.Write((uint)databuffer.Length, Endian.Big);
            Span<byte> groupBytes = MemoryMarshal.Cast<RvzGroupT, byte>(Groups);
            using MemoryPoolStream groupCom = encoder.Compress(groupBytes);
            dest.Write(Groups.Length, Endian.Big);
            dest.Write<long>(292 + AlgorithmData.Length + (Parts.Length * 0x30) + databuffer.Length, Endian.Big);
            dest.Write((uint)groupCom.Length, Endian.Big);
            dest.Write(AlgorithmData.Length, Endian.Big);
            dest.Write(AlgorithmData);
            databuffer.WriteTo(dest);
            groupCom.WriteTo(dest);
            RawData[0] = new(0, RawData[0].DataSize + 0x80, 0, RawData[0].Groups);
        }
    }
}

using AuroraLib.Core.Interfaces;
using System.Buffers;

namespace AuroraLib.DiscImage.RVZ
{
    public class Header : IBinaryObject
    {
        public ImageType Magic;
        public RvzVersion Version;
        public RvzVersion VersionCompatible;
        public uint DiscTSize;
        public readonly byte[] DiscHash;
        public long IsoFileSize;
        public long ThisFileSize;
        public readonly byte[] HeaderHash;

        public Header()
        {
            DiscHash = new byte[0x14];
            HeaderHash = new byte[0x14];
        }

        public Header(Stream source) : this() => BinaryDeserialize(source);

        public void BinaryDeserialize(Stream source)
        {
            Magic = source.Read<ImageType>(Endian.Big);
            Version = source.Read<RvzVersion>(Endian.Big);
            VersionCompatible = source.Read<RvzVersion>(Endian.Big);
            DiscTSize = source.Read<uint>(Endian.Big);
            source.Read(DiscHash);
            IsoFileSize = source.Read<long>(Endian.Big);
            ThisFileSize = source.Read<long>(Endian.Big);
            source.Read(HeaderHash);
        }

        public void BinarySerialize(Stream dest)
        {
            dest.Write(Magic);
            dest.Write(Version);
            dest.Write(VersionCompatible);
            dest.Write(DiscTSize);
            dest.Write(DiscHash);
            dest.Write(IsoFileSize);
            dest.Write(ThisFileSize);
            dest.Write(HeaderHash);
        }
    }
}

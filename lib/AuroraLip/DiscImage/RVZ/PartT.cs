using AuroraLib.Core.Interfaces;

namespace AuroraLib.DiscImage.RVZ
{
    public class PartT : IBinaryObject
    {
        public readonly byte[] PartKey;
        public PartDataT[] PartData;

        public PartT()
        {
            PartKey = new byte[0x10];
            PartData = new PartDataT[2];
        }

        public void BinaryDeserialize(Stream source)
        {
            source.Read(PartKey);
            source.Read<PartDataT>(PartData, Endian.Big);
        }

        public void BinarySerialize(Stream dest)
        {
            dest.Write(PartKey);
            dest.Write<PartDataT>(PartData, Endian.Big);
        }
    }
}

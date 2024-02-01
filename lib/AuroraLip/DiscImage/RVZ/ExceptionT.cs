using AuroraLib.Core.Interfaces;

namespace AuroraLib.DiscImage.RVZ
{
    public class ExceptionT : IBinaryObject
    {
        public ushort Offset;
        public readonly byte[] Hash;

        public ExceptionT()
            => Hash = new byte[20];

        public void BinaryDeserialize(Stream source)
        {
            Offset = source.ReadUInt16(Endian.Big);
            source.Read(Hash);
        }

        public void BinarySerialize(Stream dest)
        {
            dest.Write(Offset);
            dest.Write(Hash);
        }
    }
}

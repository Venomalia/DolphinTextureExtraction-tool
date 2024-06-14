namespace AuroraLib.DiscImage.Revolution
{
    public class CMD
    {
        public uint ContentId;
        public ushort Index;
        public ContentType Type;
        public ulong Size;
        public readonly byte[] Hash = new byte[20];

        public CMD(Stream stream)
        {
            ContentId = stream.ReadUInt32(Endian.Big);
            Index = stream.ReadUInt16(Endian.Big);
            Type = stream.Read<ContentType>(Endian.Big);
            Size = stream.ReadUInt64(Endian.Big);
            stream.Read(Hash);
        }

        public void Write(Stream dest)
        {
            dest.Write(ContentId, Endian.Big);
            dest.Write(Index, Endian.Big);
            dest.Write(Type, Endian.Big);
            dest.Write(Size, Endian.Big);
            dest.Write(Hash);
        }

        public enum ContentType : ushort
        {
            Normal = 0x0001,
            DLC = 0x4001,
            Shared = 0x8001,
        }

        public byte[] GetContentIV()
        {
            byte[] iv_bits = BitConverter.GetBytes(Index);
            byte[] iv = new byte[16];
            iv[0] = iv_bits[1];
            iv[1] = iv_bits[0];
            return iv;
        }
    }
}

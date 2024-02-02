namespace AuroraLib.DiscImage.RVZ
{
    public readonly struct RvzGroupT
    {
        private readonly uint dataOffset;
        private readonly uint packtDataSize;
        public readonly uint PackedSize;

        public readonly uint DataOffset => dataOffset << 2;
        public readonly uint DataSize => packtDataSize & 0x7FFFFFFF;
        public readonly bool IsCompressed => (packtDataSize & 0x80000000) != 0;
    }
}

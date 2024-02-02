namespace AuroraLib.DiscImage.RVZ
{
    public readonly struct RawDataT
    {
        public readonly long DataOffset;
        public readonly long DataSize;
        public readonly uint GroupIndex;
        public readonly uint Groups;

        public readonly long DataEndOffset => DataOffset + DataSize;

        public RawDataT(long dataOffset, long dataSize, uint groupIndex, uint groups)
        {
            DataOffset = dataOffset;
            DataSize = dataSize;
            GroupIndex = groupIndex;
            Groups = groups;
        }
    }
}

namespace AuroraLib.DiscImage.RVZ
{
    public readonly struct PartDataT
    {
        public readonly uint FirstSector;
        public readonly uint Sectors;
        public readonly uint GroupIndex;
        public readonly uint Groups;

        public PartDataT(uint firstSector, uint sectors, uint groupIndex, uint groups)
        {
            FirstSector = firstSector;
            Sectors = sectors;
            GroupIndex = groupIndex;
            Groups = groups;
        }
    }
}

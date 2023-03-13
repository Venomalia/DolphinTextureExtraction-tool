namespace AFSLib
{
    internal struct StreamEntryInfo
    {
        public uint Offset;
        public string Name;
        public uint Size;
        public DateTime LastWriteTime;
        public uint UnknownAttribute;

        public bool IsNull => Offset == 0 || Size == 0;
    }
}
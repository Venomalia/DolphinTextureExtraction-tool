using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Compression;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Red Fly Studios Star Wars Force Unleashed Archive
    /// </summary>
    // From https://zenhax.com/viewtopic.php?f=9&t=7288
    // Thanks to Acewell, aluigi, AlphaTwentyThree, Chrrox
    public sealed class POD5 : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("POD5");

        public POD5()
        {
        }

        public POD5(string name) : base(name)
        {
        }

        public POD5(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);

            source.Seek(0x58, SeekOrigin.Begin);
            uint FileCount = source.ReadUInt32(Endian.Little);
            source.Seek(0x108, SeekOrigin.Begin);
            uint InfoTable = source.ReadUInt32(Endian.Little);
            uint StringTableOffset = (FileCount * 0x1c) + InfoTable;

            source.Seek(InfoTable, SeekOrigin.Begin);
            ZLib zLib = new();
            for (int i = 0; i < FileCount; i++)
            {
                uint NameOffsetForFile = source.ReadUInt32(Endian.Little);
                uint SizeForFile = source.ReadUInt32(Endian.Little);
                uint OffsetForFile = source.ReadUInt32(Endian.Little);
                uint CompressedSizeForFile = source.ReadUInt32(Endian.Little);
                uint Compressed = source.ReadUInt32(Endian.Little);
                uint Unknown1 = source.ReadUInt32(Endian.Little);
                uint Unknown2 = source.ReadUInt32(Endian.Little);
                long SavedPosition = source.Position;

                source.Seek(NameOffsetForFile + StringTableOffset, SeekOrigin.Begin);
                string Name = source.ReadString();

                //If Duplicate...
                if (Contains(Name))
                    Name += i;

                FileNode Sub = new (Name, new SubStream(source, SizeForFile, OffsetForFile));
                if (SizeForFile != CompressedSizeForFile)
                {
                    MemoryPoolStream decom = new((int)CompressedSizeForFile);
                    zLib.Decompress(Sub.Data, decom);
                    Sub.Data = decom;
                }
                Add(Sub);

                // Read the file, move on to the next one
                source.Seek(SavedPosition, SeekOrigin.Begin);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

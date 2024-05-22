using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Krome Studios Star Wars Force Unleashed
    /// </summary>
    public sealed class RKV2 : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("RKV2");

        public RKV2()
        {
        }

        public RKV2(string name) : base(name)
        {
        }

        public RKV2(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Deserialize(Stream source)
        {
            source.MatchThrow(_identifier);
            uint FileCount = source.ReadUInt32(Endian.Little);
            uint NameSize = source.ReadUInt32(Endian.Little);
            uint FullName_Files = source.ReadUInt32(Endian.Little);
            uint Dummy = source.ReadUInt32(Endian.Little);
            uint Info_Offset = source.ReadUInt32(Endian.Little);
            uint Dummy2 = source.ReadUInt32(Endian.Little);

            uint NameOffset = FileCount * 20 + Info_Offset;

            uint FullName_Offset = FileCount * 16 + (NameOffset + NameSize);

            source.Seek(Info_Offset, SeekOrigin.Begin);
            for (int i = 0; i < FileCount; i++)
            {
                uint NameOffsetForFile = (uint)source.ReadUInt32(Endian.Little);
                uint DummyForFile = (uint)source.ReadUInt32(Endian.Little);
                uint SizeForFile = (uint)source.ReadUInt32(Endian.Little);
                uint OffsetForFile = (uint)source.ReadUInt32(Endian.Little);
                uint CRCForFile = (uint)source.ReadUInt32(Endian.Little);
                long FilePosition = source.Position;

                source.Seek(NameOffsetForFile + NameOffset, SeekOrigin.Begin);
                string Name = source.ReadString();

                FileNode Sub = new(Name, new SubStream(source, SizeForFile, OffsetForFile));
                //If Duplicate...
                if (Contains(Name))
                    Sub.Name += i;
                Add(Sub);

                // Read the file, move on to the next one
                source.Seek(FilePosition, SeekOrigin.Begin);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

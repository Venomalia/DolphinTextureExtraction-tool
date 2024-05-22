using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;
using static AuroraLib.Texture.Formats.TXTRCC;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Square Enix FFCC Font data archive.
    /// </summary>
    public sealed class FONT : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("FONT");

        public FONT()
        {
        }

        public FONT(string name) : base(name)
        {
        }

        public FONT(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0xD0 && stream.Match(_identifier) && stream.ReadUInt32(Endian.Big) + 0x10 == stream.Length;

        protected override void Deserialize(Stream source)
        {
            CCPropertieNote root = source.Read<CCPropertieNote>(Endian.Big);

            long contentSize = source.Position + root.ContentSize - 0x10;
            while (source.Position < contentSize)
            {
                long offset = source.Position;
                CCPropertieNote propertie = source.Read<CCPropertieNote>(Endian.Big);
                FileNode file = new(propertie.Identifier.ToString(), new SubStream(source, propertie.ContentSize + 0x10, offset));

                if (propertie.Identifier == 1381259348)
                {
                    CCPropertieNote nameNote = source.Read<CCPropertieNote>(Endian.Big);
                    file.Name = source.ReadString((int)nameNote.ContentSize);
                }
                Add(file);
                source.Seek(offset + propertie.ContentSize + 0x10, SeekOrigin.Begin);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

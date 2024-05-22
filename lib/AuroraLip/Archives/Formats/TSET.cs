using AuroraLib.Common.Node;
using AuroraLib.Core.Interfaces;
using static AuroraLib.Texture.Formats.TXTRCC;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Square Enix FFCC data Archive
    /// </summary>
    public sealed class TSET : ArchiveNode, IHasIdentifier
    {
        public override bool CanWrite => false;

        public IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("TSET");
        private static readonly Identifier32 _identifier2 = new("TEX ");
        private static readonly Identifier32 _identifier3 = new("OTM ");

        public TSET()
        {
        }

        public TSET(string name) : base(name)
        {
        }

        public TSET(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
        {
            CCPropertieNote root = stream.Read<CCPropertieNote>(Endian.Big);

            return (root.Identifier == _identifier || root.Identifier == _identifier2 || (root.Identifier == _identifier3 && stream.At(0x20, s => s.Match(_identifier)))) && root.ContentSize + 0x10 == stream.Length;
        }

        protected override void Deserialize(Stream source)
        {
            CCPropertieNote root = source.Read<CCPropertieNote>(Endian.Big);
            if (root.Identifier != _identifier)
            {
                source.Seek(0x20, SeekOrigin.Begin);
                root = source.Read<CCPropertieNote>(Endian.Big);
            }

            long contentSize = source.Position + root.ContentSize - 0x10;
            while (source.Position < contentSize)
            {
                long offset = source.Position;
                CCPropertieNote txtr = source.Read<CCPropertieNote>(Endian.Big);
                if (txtr.Identifier != 1381259348)
                {
                    return;
                }

                CCPropertieNote propertie = source.Read<CCPropertieNote>(Endian.Big);
                string name = source.ReadString((int)propertie.ContentSize);

                FileNode file = new(name, new SubStream(source, txtr.ContentSize + 0x10, offset));
                if (Contains(file))
                    file.Name += Count;
                Add(file);
                source.Seek(offset + txtr.ContentSize + 0x10, SeekOrigin.Begin);
            }
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();
    }
}

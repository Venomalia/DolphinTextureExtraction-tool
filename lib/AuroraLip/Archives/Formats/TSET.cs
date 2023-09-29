using AuroraLib.Common;
using AuroraLib.Core.Interfaces;
using static AuroraLib.Texture.Formats.TXTRCC;

namespace AuroraLib.Archives.Formats
{
    public class TSET : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("TSET");
        private static readonly Identifier32 _identifier2 = new("TEX ");
        private static readonly Identifier32 _identifier3 = new("OTM ");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
        {
            CCPropertieNote root = stream.Read<CCPropertieNote>(Endian.Big);

            return (root.Identifier == _identifier || root.Identifier == _identifier2 || (root.Identifier == _identifier3 && stream.At(0x20, s => s.Match(_identifier)))) && root.ContentSize + 0x10 == stream.Length;
        }

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };

            CCPropertieNote root = stream.Read<CCPropertieNote>(Endian.Big);
            if (root.Identifier != _identifier)
            {
                stream.Seek(0x20, SeekOrigin.Begin);
                root = stream.Read<CCPropertieNote>(Endian.Big);
            }

            long contentSize = stream.Position + root.ContentSize - 0x10;
            while (stream.Position < contentSize)
            {
                long offset = stream.Position;
                CCPropertieNote txtr = stream.Read<CCPropertieNote>(Endian.Big);
                if (txtr.Identifier != 1381259348)
                {
                    return;
                }

                CCPropertieNote propertie = stream.Read<CCPropertieNote>(Endian.Big);
                string name = stream.ReadString((int)propertie.ContentSize);

                if (Root.Items.ContainsKey(name))
                {
                    name += Root.Items.Count;
                }
                Root.AddArchiveFile(stream, txtr.ContentSize + 0x10, offset, name);
                stream.Seek(offset + txtr.ContentSize + 0x10, SeekOrigin.Begin);
            }
        }

        protected override void Write(Stream ArchiveFile) => throw new NotImplementedException();
    }
}

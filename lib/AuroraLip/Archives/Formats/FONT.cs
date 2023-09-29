using AuroraLib.Common;
using AuroraLib.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AuroraLib.Texture.Formats.TXTRCC;

namespace AuroraLib.Archives.Formats
{
    public class FONT : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("FONT");

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Length > 0xD0 && stream.Match(_identifier) && stream.ReadUInt32(Endian.Big) + 0x10 == stream.Length;

        protected override void Read(Stream stream)
        {
            Root = new ArchiveDirectory() { OwnerArchive = this };

            CCPropertieNote root = stream.Read<CCPropertieNote>(Endian.Big);

            long contentSize = stream.Position + root.ContentSize - 0x10;
            while (stream.Position < contentSize)
            {
                long offset = stream.Position;
                CCPropertieNote propertie = stream.Read<CCPropertieNote>(Endian.Big);
                string name;
                if (propertie.Identifier == 1381259348)
                {
                    CCPropertieNote nameNote = stream.Read<CCPropertieNote>(Endian.Big);
                    name = stream.ReadString((int)nameNote.ContentSize);
                }
                else
                {
                    name = propertie.Identifier.ToString();
                }

                if (Root.Items.ContainsKey(name))
                {
                    name += Root.Items.Count;
                }

                Root.AddArchiveFile(stream, propertie.ContentSize + 0x10, offset, name);
                stream.Seek(offset + propertie.ContentSize + 0x10, SeekOrigin.Begin);
            }
        }

        protected override void Write(Stream ArchiveFile) => throw new NotImplementedException();
    }
}

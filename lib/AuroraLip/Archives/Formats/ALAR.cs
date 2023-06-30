using AuroraLib.Common;
using AuroraLib.Common.Struct;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Aqualead Archive
    /// </summary>
    // base on https://zenhax.com/viewtopic.php?t=16613
    public class ALAR : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new("ALAR");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            byte flag = stream.ReadUInt8();
            byte unk = stream.ReadUInt8();
            ushort entries = stream.ReadUInt16(Endian.Big);
            ushort unk_2 = stream.ReadUInt16(Endian.Big);
            ushort unk_3 = stream.ReadUInt16(Endian.Big);
            ushort unk_4 = stream.ReadUInt16(Endian.Big);
            ushort unk_5 = stream.ReadUInt16(Endian.Big);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            switch (flag)
            {
                case 2:
                    for (int i = 0; i < entries; i++)
                    {
                        uint mistery = stream.ReadUInt32(Endian.Big);
                        uint offset = stream.ReadUInt32(Endian.Big);
                        uint size = stream.ReadUInt32(Endian.Big);
                        uint pad = stream.ReadUInt32(Endian.Big);

                        long pos = stream.Position;
                        stream.Seek(offset - 0x22, SeekOrigin.Begin);
                        string name = stream.ReadString(0x20);

                        if (Root.Items.ContainsKey(name))
                            name = name + i;
                        Root.AddArchiveFile(stream, size, offset, name);
                        stream.Seek(pos, SeekOrigin.Begin);
                    }
                    break;

                case 3:
                    ushort unk_6 = stream.ReadUInt16(Endian.Big);
                    List<ushort> entrie_pos = new List<ushort>();
                    for (int i = 0; i < entries; i++)
                    {
                        entrie_pos.Add(stream.ReadUInt16(Endian.Big));
                    }
                    foreach (ushort entrie in entrie_pos)
                    {
                        stream.Seek(entrie, SeekOrigin.Begin);
                        uint id = stream.ReadUInt32(Endian.Big);
                        uint offset = stream.ReadUInt32(Endian.Big);
                        uint size = stream.ReadUInt32(Endian.Big);
                        string lixo = stream.ReadString(6);
                        string name = stream.ReadString();

                        if (Root.Items.ContainsKey(name))
                            name = name + id;
                        Root.AddArchiveFile(stream, size, offset, name);
                    }
                    break;

                default:
                    throw new Exception($"{nameof(ALAR)} unknown flag:{flag}");
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

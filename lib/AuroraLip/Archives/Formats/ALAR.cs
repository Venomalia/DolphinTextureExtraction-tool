using AuroraLib.Common;
using AuroraLib.Core.Buffers;
using AuroraLib.Core.Interfaces;
using AuroraLib.Core.IO;

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

        public bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => stream.Match(_identifier);

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);
            byte version = stream.ReadUInt8();
            EntryFlags entryFlags = stream.Read<EntryFlags>();
            ushort entries = stream.ReadUInt16(Endian.Big);
            uint lowId = stream.ReadUInt32(Endian.Big);
            uint highId = stream.ReadUInt32(Endian.Big);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            switch (version)
            {
                case 2:
                    for (int i = 0; i < entries; i++)
                    {
                        uint id = stream.ReadUInt32(Endian.Big);
                        uint offset = stream.ReadUInt32(Endian.Big);
                        uint size = stream.ReadUInt32(Endian.Big);
                        uint pad = stream.ReadUInt32(Endian.Big);

                        long pos = stream.Position;
                        stream.Seek(offset - 0x22, SeekOrigin.Begin);
                        string name = stream.ReadString(0x20);

                        if (Root.Items.ContainsKey(name))
                            name += i;
                        Root.AddArchiveFile(stream, size, offset, name);
                        stream.Seek(pos, SeekOrigin.Begin);
                    }
                    break;

                case 3:
                    ushort dataTabelOffset = stream.ReadUInt16(Endian.Big);
                    SpanBuffer<ushort> entrieOffsets = new(entries);
                    stream.Read<ushort>(entrieOffsets, Endian.Big);
                    foreach (ushort entrieOffset in entrieOffsets)
                    {
                        stream.Seek(entrieOffset, SeekOrigin.Begin);
                        uint id = stream.ReadUInt32(Endian.Big);
                        uint offset = stream.ReadUInt32(Endian.Big);
                        uint size = stream.ReadUInt32(Endian.Big);
                        string lixo = stream.ReadString(6);
                        string name = stream.ReadString();

                        if (Root.Items.ContainsKey(name))
                            name += id;
                        Root.AddArchiveFile(stream, size, offset, name);
                    }
                    entrieOffsets.Dispose();
                    break;

                default:
                    throw new Exception($"{nameof(ALAR)} unknown version:{version}");
            }
        }

        [Flags]
        public enum EntryFlags : byte
        {
            IsResident = 1,
            IsPrepare = 2,
            Unknown = 32,
            Unknown2 = 64,
            HasName = 128
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }
    }
}

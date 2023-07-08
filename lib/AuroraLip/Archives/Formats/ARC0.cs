using AuroraLib.Common;
using AuroraLib.Core.Interfaces;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// Brawl ARC0 Archive
    /// </summary>
    // ref https://github.com/soopercool101/BrawlCrate/blob/a0e5638c34bba0de783ece169d483ad7e7dcb016/BrawlLib/SSBB/ResourceNodes/Archives/ARCNode.cs
    public class ARC0 : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => _identifier;

        private static readonly Identifier32 _identifier = new((byte)'A', (byte)'R', (byte)'C', 0);

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Match(_identifier) && stream.ReadByte() == 0;

        protected override void Read(Stream stream)
        {
            stream.MatchThrow(_identifier);

            ushort version = stream.ReadUInt16(Endian.Big); //257
            ushort files = stream.ReadUInt16(Endian.Big);
            ulong unk = stream.ReadUInt64(Endian.Big);
            string name = stream.ReadString(48);

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < files; i++)
            {
                EntryType type = (EntryType)stream.ReadUInt16(Endian.Big);
                ushort index = stream.ReadUInt16(Endian.Big);
                uint size = stream.ReadUInt32(Endian.Big);
                byte groupIndex = (byte)stream.ReadByte();
                byte pad = (byte)stream.ReadByte();
                short redirectIndex = stream.ReadInt16(Endian.Big);

                uint[] padding = new uint[5];
                for (int p = 0; p < 5; p++)
                    padding[p] = stream.ReadUInt32(Endian.Big);

                Root.AddArchiveFile(stream, size, $"{type}_{i}.pcs");

                stream.Align(size, SeekOrigin.Current, 32);
            }
        }

        protected override void Write(Stream ArchiveFile)
        {
            throw new NotImplementedException();
        }

        public enum EntryType : short
        {
            None = 0x0,
            MiscData = 0x1,
            ModelData = 0x2,
            TextureData = 0x3,
            AnimationData = 0x4,
            SceneData = 0x5,
            Type6 = 0x6,
            GroupedArchive = 0x7,
            EffectData = 0x8
        }
    }
}

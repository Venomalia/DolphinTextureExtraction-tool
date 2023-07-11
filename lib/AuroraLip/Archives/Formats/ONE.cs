using AuroraLib.Common;
using AuroraLib.Core.Interfaces;
using System.Runtime.CompilerServices;

namespace AuroraLib.Archives.Formats
{
    public class ONE_SH : Archive, IHasIdentifier, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public virtual IIdentifier Identifier => Magic;

        public static readonly Identifier32 Magic = new("One ");

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 0x20 && stream.ReadUInt32() == 0 && stream.At(0xC, s => s.Match(Magic));

        protected override void Read(Stream stream)
        {
            RwHeader rwHeader = stream.Read<RwHeader>();
            Header header = stream.Read<Header>();
            stream.Seek(0xb0, SeekOrigin.Begin);
            Entry[] entrys = stream.For(header.Entrys, s => new Entry(stream));

            Root = new ArchiveDirectory() { OwnerArchive = this };

            for (int i = 0; i < entrys.Length; i++)
            {
                uint size = i + 1 == entrys.Length ? (uint)(stream.Length - entrys[i].Offset - Unsafe.SizeOf<RwHeader>()) : entrys[i + 1].Offset - entrys[i].Offset;
                Root.AddArchiveFile(stream, size, entrys[i].Offset + Unsafe.SizeOf<RwHeader>(), entrys[i].Name + (entrys[i].Flag == 1 ? ".prs" : string.Empty));
            }
        }

        protected override void Write(Stream stream) => throw new NotImplementedException();

        public struct RwHeader
        {
            public Typs typ;
            public uint FileSize;
            public RwVersion RwVersion;

            public enum Typs
            {
                ONE = 0,
                BSP = 11,
                DIF = 16,
                TXD = 22,
                RG1 = 41,
            }
        }

        public struct Header
        {
            public Identifier32 Identifier;
            public Identifier64 Version;
            public uint Null2;
            public uint Entrys;
        }

        private struct Entry
        {
            public string Name;
            public uint Unk;
            public uint Size;
            public uint Offset;
            public uint Flag;

            public Entry(Stream stream)
            {
                Name = stream.ReadString(0x28);
                Unk = stream.ReadUInt32();
                Size = stream.ReadUInt32();
                Offset = stream.ReadUInt32();
                Flag = stream.ReadUInt32();
            }
        }

        /// <summary>
        /// Stores the RenderWare version.
        /// </summary>
        public struct RwVersion
        {
            // V is Version
            // J is Major build
            // N is Minor build
            // R is Revision
            // VVJJ JJNN NNRR RRRR
            private ushort _version;

            public ushort Build;

            public byte Version
            {
                get => (byte)((_version >> 14) + 3);
                set => _version = (ushort)((_version & 0x3FFF) | (value - 3 << 14));
            }

            public byte Major
            {
                get => (byte)((_version & 0x3C00) >> 10);
                set => _version = (ushort)((_version & 0xC3FF) | (value << 10));
            }

            public byte Minor
            {
                get => (byte)((_version & 0x3C0) >> 6);
                set => _version = (ushort)((_version & 0x3C0) | (value << 6));
            }

            public byte Revision
            {
                get => (byte)(_version & 0x3F);
                set => _version = (ushort)((_version & 0xFFC0) | value);
            }
        }
    }
}

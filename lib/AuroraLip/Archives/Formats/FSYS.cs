using AuroraLib.Common;
using AuroraLib.Compression.Formats;

namespace AuroraLib.Archives.Formats
{
    //base https://github.com/PekanMmd/Pokemon-XD-Code/
    public class FSYS : Archive, IMagicIdentify, IFileAccess
    {
        public bool CanRead => true;

        public bool CanWrite => false;

        public string Magic => magic;

        private const string magic = "FSYS";

        private static readonly LZSS lZSS = new LZSS(12, 4, 2);

        public bool IsMatch(Stream stream, in string extension = "")
            => stream.Length > 112 && stream.MatchString(Magic);

        protected override void Read(Stream stream)
        {
            if (!IsMatch(stream))
                throw new InvalidIdentifierException(Magic);
            Header header = stream.Read<Header>(Endian.Big);

            stream.Seek(header.FileInfoOffset, SeekOrigin.Begin);
            uint[] FileInfosOffsets = stream.For((int)header.Entries, s => s.ReadUInt32(Endian.Big));

            stream.Seek(header.StringTableOffset, SeekOrigin.Begin);
            string[] Filenames = stream.For((int)header.Entries, s => s.ReadString());

            Root = new ArchiveDirectory() { OwnerArchive = this };
            for (int i = 0; i < FileInfosOffsets.Length; i++)
            {
                stream.Seek(FileInfosOffsets[i], SeekOrigin.Begin);
                var Info = stream.Read<FileInfo>(Endian.Big);

                if (Info.IsCompressed())
                {
                    //<- 0x10 LZSS Header
                    stream.Seek(Info.FileStartPointer + 0x10, SeekOrigin.Begin);
                    MemoryStream DeStream = lZSS.Decompress(stream, (int)Info.DecompressedSize);
                    Root.AddArchiveFile(DeStream, $"{Info.ResourceID:X8}_{Filenames[i]}.{Info.FileFormat}");

                }
                else
                {
                    Root.AddArchiveFile(stream, Info.DecompressedSize, Info.FileStartPointer, $"{Info.ResourceID:X8}_{Filenames[i]}.{Info.FileFormat}");
                }

            }
        }

        protected override void Write(Stream stream)
        {
            throw new NotImplementedException();
        }

        private struct Header
        {
            //magic
            public uint Version;
            public uint Id;
            public uint Entries;


            public uint flags; // 0x01: Try loading debug/<filename> if exists; 0x40000000: (checked ingame, but unknown)
            public uint unk2;
            public uint unk3;
            public uint HeaderSize;

            public uint FileSize;
            public uint pad1;
            public uint pad2;
            public uint pad3;

            public uint pad4;
            public uint pad5;
            public uint pad6;
            public uint pad7;

            public uint FileInfoOffset;
            public uint StringTableOffset;
            public uint FileTableOffset;
            public uint pad8;

            public uint pad9;
            public uint pad10;
            public uint pad11;
            public uint pad12;

            public uint FileDetailsOffset;
            public uint unk4;
            public uint unk5;
            public uint unk6;
        }

        private struct FileInfo
        {
            public uint ResourceID;
            public uint FileStartPointer;
            public uint DecompressedSize;
            public uint flags;

            public uint flags2;
            public uint CompressedSize;
            public uint pad2;
            public uint FullFilenameOffset;

            public uint FileFormatRaw;
            public uint FilenameOffset;

            // TODO: Confirm this? There is some sort of system in that value
            public FileFormat FileFormatFromResourceID
            {
                get => (FileFormat)(ResourceID >> 8 & 0xFF);
                set => ResourceID = (ResourceID & 0xFFFF00FF) & ((uint)value << 8);
            }

            public ushort FileID
            {
                get => (ushort)(ResourceID >> 16);
                set => ResourceID = (ResourceID & 0x0000FFFF) & ((uint)value << 16);
            }

            public FileFormat FileFormat
            {
                get => (FileFormat)FileFormatRaw;
                set
                {
                    FileFormatFromResourceID = value;;
                    FileFormatRaw = (uint)value;
                }
            }

            public bool IsCompressed() => (flags & 0x80000000) != 0;
        }

        /// <summary>
        /// File formats supported in the <tt>.fsys</tt> file format.
        /// The game usually does post-processing after loading the file in memory
        /// (like converting offsets to proper pointers, register the data in the proper system).
        /// </summary>
        private enum FileFormat : byte
        {
            /// <summary>
            /// Raw data the game loads, but otherwhise doesn't process afterwards without other game code trying to make use of it.
            /// </summary>
            BIN = 0x00,

            /// <summary>
            /// Floor (Room) model in modified HAL DAT format.
            /// File extension normally is <tt>.dat</tt>.
            /// </summary>
            FLOORDAT = 0x01,

            /// <summary>
            /// Model (like a character or an item box) in modified HAL DAT format.
            /// File extension normally is <tt>.dat</tt>.
            /// </summary>
            MODELDAT = 0x02,

            /// <summary>
            /// Collision, includes triggers and such.
            /// </summary>
            CCD = 0x03,

            /// <summary>
            /// Shorter music files for fanfares etc.
            /// </summary>
            SAMP = 0x04,

            /// <summary>
            /// String table for a language.
            /// </summary>
            MSG = 0x05,

            /// <summary>
            /// Font.
            /// </summary>
            FNT = 0x06,

            /// <summary>
            /// Script data and code.
            /// </summary>
            SCD = 0x07,

            /// <summary>
            /// Multiple .dat models in one archive
            /// (TODO: IS dummied out in GXXP01 and no file uses this, check the other games)
            /// </summary>
            DATS = 0x08,

            /// <summary>
            /// Texture. Internally known as GStexture.
            /// File extension is <tt>.gtx</tt>.
            /// </summary>
            GTX = 0x09,

            /// <summary>
            /// Particle data.
            /// </summary>
            GPT1 = 0x0A,

            /// <summary>
            /// Relocatable code (like a DLL; usually just data that gets registered ingame),
            /// except is persists (doesn't unlink when it is told to be removed).
            /// </summary>
            RELP = 0x0B,

            /// <summary>
            /// Camera data in modified HAL DAT format.
            /// </summary>
            CAM = 0x0C,

            /// <summary>
            /// Relocatable code (like a DLL; usually just data that gets registered ingame).
            /// </summary>
            REL = 0x0E,

            /// <summary>
            /// Character battle model.
            /// </summary>
            PKX = 0x0F,

            /// <summary>
            /// Move animation.
            /// </summary>
            WZX = 0x10,

            GSFILE11 = 0x11,

            /// <summary>
            /// Music file header.
            /// </summary>
            ISD = 0x14,

            /// <summary>
            /// Music file data.
            /// </summary>
            ISH = 0x15,

            /// <summary>
            /// THP (video) header
            /// </summary>
            THH = 0x16,

            /// <summary>
            /// THP (video) data
            /// </summary>
            THD = 0x17,

            /// <summary>
            /// Multi Texture.
            /// </summary>
            GSW = 0x18,

            /// <summary>
            /// Animated Texture.
            /// Official file extension is currently unknown.
            /// </summary>
            GSAGTX = 0x19,

            /// <summary>
            /// Battle trainer data (possible fights, like the trainer and their pokemon they send out).
            /// </summary>
            DECK = 0x1A,
        }
    }
}

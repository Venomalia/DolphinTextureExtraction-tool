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
                    Root.AddArchiveFile(DeStream, $"{Info.FileID}_{Filenames[i]}.{Info.FileFormat}");

                }
                else
                {
                    Root.AddArchiveFile(stream, Info.DecompressedSize, Info.FileStartPointer, $"{Info.FileID}_{Filenames[i]}.{Info.FileFormat}");
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

            public uint unk1;//2147483648
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
            public ushort FileID;
            private FileFormat fileFormat;
            public byte pad0;
            public uint FileStartPointer;
            public uint DecompressedSize;
            public uint flags;//0x80000000 == Compressed

            public uint flags2;
            public uint CompressedSize;
            public uint pad2;
            public uint FullFilenameOffse;

            private uint fileFormatIndex;
            public uint FilenameOffse;


            public FileFormat FileFormat
            {
                get => fileFormat;
                set
                {
                    fileFormat = value;
                    fileFormatIndex = value == 0 ? 0 : (uint)value / 2;
                }
            }
            public uint FileFormatIndex => fileFormatIndex;
            public bool IsCompressed() => CompressedSize != DecompressedSize;
        }

        //copied from here so we don't need to do our own thing. https://github.com/rotobash/pokemon-ngc-rando/blob/e97451167c337b0a26595ca7702d18101e71374f/Common/Contracts/FileTypes.cs 
        private enum FileFormat : byte
        {
            None = 0x00,
            RDAT = 0x02, // room model in hal dat format (unknown if it uses a different file extension)
            DAT = 0x04,// character model in hal dat format
            CCD = 0x06,// collision file
            SAMP = 0x08, // shorter music files for fanfares etc.
            MSG = 0x0a, // string table
            FNT = 0x0c, // font
            SCD = 0x0e, // script data
            DATS = 0x10, // multiple .dat models in one archive
            GTX = 0x12, // texture
            GPT1 = 0x14, // particle data
            CAM = 0x18, // camera data
            REL = 0x1c, // relocation table
            PKX = 0x1e, // character battle model (same as dat with additional header information)
            WZX = 0x20, // move animation
            ISD = 0x28, // audio file header
            ISH = 0x2a, // audio file
            THH = 0x2c, // thp media header
            THD = 0x2e, // thp media data
            GSW = 0x30, // multi texture
            ATX = 0x32, // animated texture (official file extension is currently unknown)
            BIN = 0x34, // binary data
        }
    }
}

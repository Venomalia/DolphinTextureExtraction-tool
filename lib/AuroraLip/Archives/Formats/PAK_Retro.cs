using AuroraLib.Common;
using AuroraLib.Compression.Formats;
using IronCompress;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// The .pak format used in Metroid Prime and Metroid Prime 2
    /// </summary>
    // base https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime)#Header
    public class PAK_Retro : Archive, IFileAccess
    {
        public virtual bool CanRead => true;

        public virtual bool CanWrite => false;

        public const string Extension = ".pak";

        public virtual bool IsMatch(Stream stream, in string extension = "")
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, in string extension = "")
            => extension.ToLower().Equals(Extension) && stream.ReadUInt16(Endian.Big) == 3 && stream.ReadUInt16(Endian.Big) == 5 && stream.ReadUInt32(Endian.Big) == 0;

        protected override void Read(Stream stream)
        {
            //Header
            ushort VersionMajor = stream.ReadUInt16(Endian.Big);
            ushort VersionMinor = stream.ReadUInt16(Endian.Big);
            uint Padding = stream.ReadUInt32(Endian.Big);
            if (VersionMajor != 3 && VersionMinor != 5 && Padding != 0)
                throw new InvalidIdentifierException($"{VersionMajor},{VersionMinor},{Padding}");

            //NameTabel
            uint Sections = stream.ReadUInt32(Endian.Big);
            Dictionary<uint, NameEntry> NameTable = new();
            for (int i = 0; i < Sections; i++)
            {
                NameEntry entry = new(stream);
                if (!NameTable.ContainsKey(entry.ID))
                {
                    NameTable.Add(entry.ID, entry);
                }
                else
                {
                    Events.NotificationEvent?.Invoke(NotificationType.Warning, $"{nameof(PAK_Retro)},{entry.ID} is already in name table. string:{entry.Name}.");
                }
            }

            //ResourceTable
            Sections = stream.ReadUInt32(Endian.Big);
            AssetEntry[] AssetTable = stream.For((int)Sections, s => new AssetEntry(stream));

            //DataTable
            Root = new ArchiveDirectory() { OwnerArchive = this };
            foreach (AssetEntry entry in AssetTable)
            {
                string name;
                if (NameTable.TryGetValue(entry.ID, out NameEntry nameEntry))
                {
                    name = $"{entry.ID}_{nameEntry.Name}.{nameEntry.Type}";
                }
                else
                {
                    name = $"{entry.ID}.{entry.Type}";
                }
                if (Root.Items.ContainsKey(name))
                {
                    continue;
                }

                if (entry.Compressed)
                {
                    stream.Seek(entry.Offset, SeekOrigin.Begin);
                    uint DeSize = stream.ReadUInt32(Endian.Big);
                    Stream es = Decompress(new SubStream(stream, entry.Size - 4), DeSize);
                    ArchiveFile Sub = new() { Parent = Root, Name = name, FileData = es };
                    Root.Items.Add(Sub.Name, Sub);
                }
                else
                {
                    Root.AddArchiveFile(stream, entry.Size, entry.Offset, name);
                }
            }
        }

        protected static MemoryStream Decompress(Stream input, uint decompressedSize)
        {
            //prime 1, DKCR = Zlip
            //prime 2,3 = LZO1X-999 
            if (input.Read<ZLib.Header>().Validate())
            {
                input.Seek(0, SeekOrigin.Begin);
                ZLib zLib = new();
                return zLib.Decompress(input.Read((int)input.Length), (int)decompressedSize);
            }
            else
            {
                input.Seek(0, SeekOrigin.Begin);
                return DecompressSegmentedLZO(input, decompressedSize);
            }
        }

        protected static MemoryStream DecompressSegmentedLZO(Stream input, uint decompressedSize)
        {
            const int segmentSize = 0x4000; // The decompressed size of each segment
            int numSegments = (int)Math.Ceiling((double)decompressedSize / segmentSize); // The number of segments in the file
            MemoryStream output = new((int)decompressedSize); // A stream to hold the decompressed data
            Iron iron = new();

            for (int i = 0; i < numSegments; i++)
            {
                // Calculate the size of the current segment
                int segmentLength = Math.Min(segmentSize, (int)decompressedSize - i * segmentSize);

                short blockSize = input.ReadInt16(Endian.Big);
                //Copy Block if not compressed
                if (blockSize < 0)
                {
                    blockSize -= blockSize;
                    output.Write(input.Read(blockSize), blockSize);
                    continue;
                }

                // Decompress the data for this segment
                using (IronCompressResult uncompressed = iron.Decompress(Codec.LZO, input.Read(blockSize), segmentLength))
                {
                    output.Write(uncompressed.AsSpan());
                }
            }
            return output;
        }

        protected override void Write(Stream ArchiveFile) => throw new NotImplementedException();

        protected struct NameEntry
        {
            public string Type;

            public uint ID;

            public string Name;

            public NameEntry(Stream stream)
            {
                Type = stream.ReadString(4);
                ID = stream.ReadUInt32(Endian.Big);
                int Length = (int)stream.ReadUInt32(Endian.Big);
                Name = stream.ReadString(Length);
            }
        }

        protected struct AssetEntry
        {
            public string Type;

            public uint ID;

            public bool Compressed;

            public uint Size;

            public uint Offset;

            public AssetEntry(Stream stream)
            {
                Compressed = stream.ReadUInt32(Endian.Big) == 1;
                Type = stream.ReadString(4);
                ID = stream.ReadUInt32(Endian.Big);
                Size = stream.ReadUInt32(Endian.Big);
                Offset = stream.ReadUInt32(Endian.Big);
            }
        }
    }
}

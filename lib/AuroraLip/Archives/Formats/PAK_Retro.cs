using AuroraLib.Common;
using AuroraLib.Common.Node;
using AuroraLib.Compression.Algorithms;
using AuroraLib.Core.Exceptions;

namespace AuroraLib.Archives.Formats
{
    /// <summary>
    /// The .pak format used in Metroid Prime and Metroid Prime 2
    /// </summary>
    // base https://www.metroid2002.com/retromodding/wiki/PAK_(Metroid_Prime)#Header
    public class PAK_Retro : ArchiveNode
    {
        public override bool CanWrite => false;

        public PAK_Retro()
        {
        }

        public PAK_Retro(string name) : base(name)
        {
        }

        public PAK_Retro(FileNode source) : base(source)
        {
        }

        public override bool IsMatch(Stream stream, ReadOnlySpan<char> extension = default)
            => Matcher(stream, extension);

        public static bool Matcher(Stream stream, ReadOnlySpan<char> extension = default)
            => extension.Contains(".pak", StringComparison.InvariantCultureIgnoreCase) && stream.ReadUInt16(Endian.Big) == 3 && stream.ReadUInt16(Endian.Big) == 5 && stream.ReadUInt32(Endian.Big) == 0;

        protected override void Deserialize(Stream source)
        {
            //Header
            ushort VersionMajor = source.ReadUInt16(Endian.Big);
            ushort VersionMinor = source.ReadUInt16(Endian.Big);
            uint Padding = source.ReadUInt32(Endian.Big);
            if (VersionMajor != 3 && VersionMinor != 5 && Padding != 0)
                throw new InvalidIdentifierException($"{VersionMajor},{VersionMinor},{Padding}");

            //NameTabel
            uint Sections = source.ReadUInt32(Endian.Big);
            Dictionary<uint, NameEntry> NameTable = new();
            for (int i = 0; i < Sections; i++)
            {
                NameEntry entry = new(source);
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
            Sections = source.ReadUInt32(Endian.Big);
            AssetEntry[] AssetTable = source.For((int)Sections, s => new AssetEntry(source));

            //DataTable
            foreach (AssetEntry entry in AssetTable)
            {
                string name = NameTable.TryGetValue(entry.ID, out NameEntry nameEntry)
                    ? $"{entry.ID}_{nameEntry.Name}.{nameEntry.Type}"
                    : $"{entry.ID}.{entry.Type}";

                if (Contains(name))
                    continue;

                if (entry.Compressed)
                {
                    source.Seek(entry.Offset, SeekOrigin.Begin);
                    uint DeSize = source.ReadUInt32(Endian.Big);
                    Stream es = Decompress(new SubStream(source, entry.Size - 4), DeSize);
                    FileNode file = new(name, es);
                    Add(file);
                }
                else
                {
                    FileNode file = new(name, new SubStream(source, entry.Size, entry.Offset));
                    Add(file);
                }
            }
        }

        protected static Stream Decompress(Stream input, uint decompressedSize)
        {
            //prime 1, DKCR = Zlip
            //prime 2,3 = LZO1X-999
            ZLib.Header header = input.Read<ZLib.Header>();
            if (header.Validate() && header.HasDictionary == false && header.CompressionInfo == 7)
            {
                input.Seek(0, SeekOrigin.Begin);
                Stream decompressed_stream = new MemoryPoolStream();
                new ZLib().Decompress(input, decompressed_stream, (int)input.Length);
                return decompressed_stream;
            }
            else
            {
                input.Seek(0, SeekOrigin.Begin);
                return DecompressSegmentedLZO(input, decompressedSize);
            }
        }

        protected static Stream DecompressSegmentedLZO(Stream input, uint decompressedSize)
        {
            const int segmentSize = 0x4000; // The decompressed size of each segment
            int numSegments = (int)Math.Ceiling((double)decompressedSize / segmentSize); // The number of segments in the file
            MemoryPoolStream output = new((int)decompressedSize); // A stream to hold the decompressed data

            for (int i = 0; i < numSegments; i++)
            {
                // Calculate the size of the current segment
                int segmentLength = Math.Min(segmentSize, (int)decompressedSize - i * segmentSize);
                short blockSize = input.ReadInt16(Endian.Big);

                //Copy Block if not compressed
                if (blockSize < 0)
                {
                    using MemoryPoolStream buffer = new(input, blockSize);
                    buffer.WriteTo(output);
                    continue;
                }
                else
                {
                    // Decompress the data for this segment
                    using MemoryPoolStream buffer = new(input, blockSize);
                    LZO.DecompressHeaderless(buffer, output);
                }
            }
            output.Position = 0;
            return output;
        }

        protected override void Serialize(Stream dest) => throw new NotImplementedException();

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
